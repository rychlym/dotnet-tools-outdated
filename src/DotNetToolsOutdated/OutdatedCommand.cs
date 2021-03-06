using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DotNetToolsOutdated.Extensions;
using DotNetToolsOutdated.JsonModels;
using DotNetToolsOutdated.Models;
using McMaster.Extensions.CommandLineUtils;
using Utf8Json;

namespace DotNetToolsOutdated
{
    [Command(
        Name = "dotnet-tools-outdated",
        FullName = "dotnet-tools-outdated",
        Description = "Checks whether any of installed .NET Core tools are outdated.",
        ExtendedHelpText = "\nThe outdated packages can be re-installed by dotnet tool uninstall -g package_name followed by dotnet tool install -g package_name command."
    )]
    [HelpOption]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class OutdatedCommand
    {

        [Option("-t|--toolPath", "Custom location of the (globally) installed .NET Core tools", CommandOptionType.SingleValue)]
        public string ToolPath { get; set; }

        [Option("-n|--name", "Check just one package with the given name", CommandOptionType.SingleValue)]
        public string PackageName { get; set; }

        [Option("-f|--format", "Output format. xml, json, or table are the valid values. (Default: table)", CommandOptionType.SingleValue)]
        public string Format { get; set; }

        [Option("-ni|--noIndent", "No indent (For the json format so far)", CommandOptionType.NoValue)]
        public bool NoIndent { get; set; }

        [Option("-o|--output", "Output file path. (Default: stdout)", CommandOptionType.SingleValue)]
        public string OutputPath { get; set; }

        [Option("--utf8", "Output UTF-8 instead of system default encoding. (no bom)", CommandOptionType.NoValue)]
        public bool IsUtf8 { get; set; }


        [Option("-pre|--prerelease", "Check also pre-released versions", CommandOptionType.NoValue)]
        public bool CheckPrelease { get; set; }


        private async Task OnExecuteAsync()
        {
            if (ToolPath == null)
            {
                var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                ToolPath = Path.Combine(userProfilePath, $".dotnet{Path.DirectorySeparatorChar}tools{Path.DirectorySeparatorChar}.store");
            }

            var httpClient = new HttpClient();
            var resultsCnt = 0;
            var outdatedCnt = 0;
            var unlistedCnt = 0;

            var dirs = Directory.GetDirectories(ToolPath);
            if (dirs != null)
            {
                var pkgs = new List<OutdatedResponse>();
                // fetch the package name
                foreach (var dir in dirs)
                {
                    var pkgName = Path.GetFileName(dir);
                    if (pkgName.Equals(".stage", StringComparison.Ordinal)) continue;
                    pkgs.Add(new OutdatedResponse() { PackageName = pkgName, Directory = dir });
                }

                // tasks for awaiting when all done together
                var apiGetResponseOkContinuedTasks = new List<Task>();
                var pkgResponseReadTasks = new List<Task<string>>();

                // prerelease parameter value
                var prereleasePar = CheckPrelease ? "true" : "false";
                foreach (var pkg in pkgs)
                {
                    if (!string.IsNullOrEmpty(PackageName) && !PackageName.Equals(pkg.PackageName, StringComparison.Ordinal)) continue;

                    // fetch the installed version
                    var verDirs = Directory.GetDirectories(pkg.Directory);
                    if (verDirs == null)
                    {
                        Console.WriteLine($"{pkg.PackageName} package - no sub-dirs about version");
                        continue;
                    }

                    if (verDirs.Length > 1)
                    {
                        Console.WriteLine($"ambiguity for {pkg.PackageName} package - seems to be more version sub-dirs");
                        continue;
                    }
                    if (verDirs.Length == 1) pkg.CurrentVer = Path.GetFileName(verDirs[0]);

                    // start tasks for fetching the available version
                    var url = $"https://api-v2v3search-0.nuget.org/autocomplete?id={pkg.PackageName}&prerelease={prereleasePar}";
                    var pureHttpGetTask = httpClient.GetAsync(url);
                    pkg.processing.ApiGetTaskOkContinued = pureHttpGetTask.ContinueWith(t =>
                        {
                            // on run to completion continue with async processing of the response message ..
                            HttpResponseMessage response = t.Result;
                            if (response.IsSuccessStatusCode)
                            {
                                pkg.processing.OkResponseReadTask = response.Content.ReadAsStringAsync();
                                pkgResponseReadTasks.Add(pkg.processing.OkResponseReadTask);
                            }
                            else
                            {
                                Console.WriteLine($"No results found for {pkg.PackageName}");
                            }
                        },
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                    // on faulted output a message to console
                    pureHttpGetTask.ContinueWith(
                        t => Console.WriteLine($"Task finding results for {pkg.PackageName} has faulted!"),
                        TaskContinuationOptions.OnlyOnFaulted)
                    .RunAsyncAndForget();

                    apiGetResponseOkContinuedTasks.Add(pkg.processing.ApiGetTaskOkContinued);
                }

                // await tasks for fetching the available version
                await Task.WhenAll(apiGetResponseOkContinuedTasks);
                // there should be same amount of response read tasks, which should be awaited
                await Task.WhenAll(pkgResponseReadTasks);

                // finish fetching the available version
                foreach (var pkg in pkgs)
                {
                    // no response read task at all => do not finish processing the available version
                    if (pkg.processing.OkResponseReadTask == null) continue;

                    // check error/ cancellation of the response read task
                    if (!pkg.processing.OkResponseReadTask.IsCompletedSuccessfully)
                    {
                        if (pkg.processing.OkResponseReadTask.IsFaulted)
                        {
                            Console.WriteLine($"Error parsing the response for {pkg.PackageName}");
                        }
                        else if (pkg.processing.OkResponseReadTask.IsCanceled)
                        {
                            Console.WriteLine($"Cancellation parsing the response for {pkg.PackageName}");
                        }
                        continue;
                    }

                    // the response read task was completed successfully
                    var versionsResponseStr = pkg.processing.OkResponseReadTask.Result;

                    // set the available version
                    var versionsResponse = JsonSerializer.Deserialize<VersionsResponse>(versionsResponseStr);
                    pkg.AvailableVer = (versionsResponse.Versions.Length > 0) ? versionsResponse.Versions[versionsResponse.Versions.Length - 1] : "";

                    // since now all is fetched ok
                    pkg.processing.ProcessedOk = true;
                    resultsCnt++;

                    if (pkg.IsOutdated)
                    {
                        // the package is determined as outdated 
                        pkg.processing.ProcessedOkOutdated = true;
                        if (pkg.BecomeUnlisted) unlistedCnt++;
                        outdatedCnt++;
                    }
                }

                PrintOutdatedResults(pkgs);
            }

        }


        private void PrintOutdatedResults(List<OutdatedResponse> pkgs)
        {
            var fmt = !string.IsNullOrEmpty(Format) ? Format : "table";
            switch (fmt)
            {
                case "json":
                    PrintJson(pkgs);
                    break;
                case "xml":
                    PrintXml(pkgs);
                    break;
                case "table":
                default:
                    PrintTable(pkgs);
                    break;
            }
        }


        Stream OpenOutputStream(bool overwrite)
        {
            if (!string.IsNullOrEmpty(OutputPath))
            {
                if (overwrite)
                {
                    return File.Create(OutputPath);
                }
                else
                {
                    var ret = File.OpenWrite(OutputPath);
                    ret.Seek(0, SeekOrigin.End);
                    return ret;
                }
            }
            else
            {
                return Console.OpenStandardOutput(4096);
            }
        }


        private void PrintJson(List<OutdatedResponse> pkgs)
        {
            var outEncoding = GetOutputEncoding();
            using (var outStream = OpenOutputStream(true))
            using (var wr = new StreamWriter(outStream, outEncoding))
            {
                var outdatedPackages = new { outdatedPackages = pkgs };
                var bytes = JsonSerializer.Serialize(outdatedPackages);
                if (NoIndent)
                {
                    if (outEncoding.WebName == "utf-8")
                    {
                        wr.Write(bytes);
                    }
                    else
                    {
                        wr.Write(Encoding.UTF8.GetString(bytes));
                    }
                }
                else
                {
                    var outputStr = JsonSerializer.PrettyPrint(bytes);
                    wr.Write(outputStr);
                }

                //var jwr = new JsonWriter();
                ////jwr.Indentation = 2;
                ////jwr.Formatting = Newtonsoft.Json.Formatting.Indented;
                //jwr.WriteBeginObject();
                //jwr.WritePropertyName("outdatedPackages");
                //jwr.WriteBeginArray();
                //foreach (var pkg in pkgs)
                //{
                //    if (pkg.processing.ProcessedOkOutdated)
                //    {
                //        resultsCnt++;
                //        jwr.WriteBeginObject();
                //        jwr.WritePropertyName("name");
                //        jwr.WriteString(pkg.PackageName);
                //        jwr.WriteValueSeparator();
                //        jwr.WritePropertyName("currentVersion");
                //        jwr.WriteString(pkg.CurrentVer);
                //        jwr.WriteValueSeparator();
                //        jwr.WritePropertyName("availableVersion");
                //        jwr.WriteString(pkg.AvailableVer);
                //        jwr.WriteValueSeparator();
                //        jwr.WriteEndObject();
                //        jwr.WriteValueSeparator();
                //    }
                //}
                //jwr.WriteEndArray();
                //jwr.WriteEndObject();
                //if (NoIndent)
                //{
                //    wr.Write(jwr.ToString());
                //}
                //else
                //{
                //    var outputStr = JsonSerializer.PrettyPrint(jwr.ToString());
                //    wr.Write(outputStr);
                //}
            }
        }


        private void PrintXml(List<OutdatedResponse> pkgs)
        {
            var resultsCnt = 0;

            var xwrSettings = new XmlWriterSettings();
            xwrSettings.Indent = true;
            using (var outStream = OpenOutputStream(true))
            using (var wr = new StreamWriter(outStream, GetOutputEncoding()))
            using (var xwr = XmlWriter.Create(wr, xwrSettings))
            {
                xwr.WriteStartDocument();
                xwr.WriteStartElement("outdated");
                foreach (var pkg in pkgs)
                {
                    if (pkg.processing.ProcessedOkOutdated)
                    {
                        resultsCnt++;
                        xwr.WriteStartElement("package");
                        xwr.WriteAttributeString("name", pkg.PackageName);
                        xwr.WriteElementString("currentVersion", pkg.CurrentVer);
                        xwr.WriteElementString("availableVersion", pkg.AvailableVer);
                        xwr.WriteEndElement();
                    }
                }
                xwr.WriteEndElement();
                xwr.WriteEndDocument();
            }
        }


        private void PrintTable(List<OutdatedResponse> pkgs)
        {
            var maxNameLen = 0;
            var maxInstVerLen = 0;
            var maxAvailVerLen = 0;
            var resultsCnt = 0;

            // preparation for the paddings determination
            foreach (var pkg in pkgs)
            {
                if (pkg.processing.ProcessedOkOutdated)
                {
                    resultsCnt++;
                    if (maxNameLen < pkg.PackageName.Length) maxNameLen = pkg.PackageName.Length;
                    if (maxInstVerLen < pkg.CurrentVer.Length) maxInstVerLen = pkg.CurrentVer.Length;
                    if (maxAvailVerLen < pkg.AvailableVer.Length) maxAvailVerLen = pkg.AvailableVer.Length;
                }
            }

            if (resultsCnt > 0)
            {
                const string packageIdStr = "Package Id";
                const string currentVerStr = "Current";
                const string availVerStr = "Available";
                if (maxNameLen < packageIdStr.Length) maxNameLen = packageIdStr.Length;
                if (maxInstVerLen < currentVerStr.Length) maxInstVerLen = currentVerStr.Length;
                if (maxAvailVerLen < availVerStr.Length) maxAvailVerLen = availVerStr.Length;


                using (var outStream = OpenOutputStream(true))
                using (var tw = new StreamWriter(outStream, GetOutputEncoding()))
                {
                    tw.WriteLine($"{packageIdStr.PadRight(maxNameLen)} {currentVerStr.PadRight(maxInstVerLen)} {availVerStr.PadRight(maxAvailVerLen)}");
                    tw.WriteLine(new string('-', maxNameLen + maxInstVerLen + maxAvailVerLen + 2));
                    foreach (var outdatedPkg in pkgs)
                    {
                        if (outdatedPkg.processing.ProcessedOkOutdated)
                        {
                            tw.WriteLine($"{outdatedPkg.PackageName.PadRight(maxNameLen)} {outdatedPkg.CurrentVer.PadRight(maxInstVerLen)} {outdatedPkg.AvailableVer.PadRight(maxAvailVerLen)}");
                        }
                    }
                }
            }
        }
 

        Encoding GetOutputEncoding()
            => IsUtf8 ? new UTF8Encoding(false) : Encoding.Default;


        private static string GetVersion()
            => typeof(OutdatedCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }


}