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
using Newtonsoft.Json;

namespace DotNetToolsOutdated
{
    [Command(
        Name = "dotnet tools outdated",
        FullName = "dotnet-tools-outdated",
        Description = "Checks if any of installed .NET Core CLI tools are outdated"
    )]
    [HelpOption]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class OutdatedCommand
    {

        [Option("-t|--toolPath", "Custom path to the installed .NET CLI packages.", CommandOptionType.SingleValue)]
        public string ToolPath { get; set; }

        [Option("-n|--name", "Check just one package with the given name.", CommandOptionType.SingleValue)]
        public string PackageName { get; set; }

        [Option("-f|--format", "Output format. xml, json, or table are the valid values. (Default: table)", CommandOptionType.SingleValue)]
        public string Format { get; set; }

        [Option("-u|--utf8", "Output UTF-8 instead of system default encoding. (no bom)", CommandOptionType.NoValue)]
        public bool IsUtf8 { get; set; }

        [Option("-o|--output", "Output file path. (Default: stdout)", CommandOptionType.SingleValue)]
        public string OutputPath { get; set; }


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

            var dirs = Directory.GetDirectories(ToolPath);
            if (dirs != null)
            {
                var outdatedPkgs = new List<OutdatedResponse>();
                // fetch the package name
                foreach (var dir in dirs)
                {
                    var pkgName = Path.GetFileName(dir);
                    if (pkgName.Equals(".stage", StringComparison.Ordinal)) continue;
                    outdatedPkgs.Add(new OutdatedResponse() { PackageName = pkgName, Directory = dir });
                }

                // tasks for awaiting when all done together
                var httpClientGetTasks = new List<Task<HttpResponseMessage>>();
                var httpResponseReadTasks = new List<Task<PackageResponse>>();

                foreach (var outdatedPkg in outdatedPkgs)
                {
                    if (!string.IsNullOrEmpty(PackageName) && !PackageName.Equals(outdatedPkg.PackageName, StringComparison.Ordinal)) continue;

                    // fetch the installed version
                    var verDirs = Directory.GetDirectories(outdatedPkg.Directory);
                    if (verDirs == null)
                    {
                        Console.WriteLine($"{outdatedPkg.PackageName} package - no sub-dirs about version");
                        continue;
                    }

                    if (verDirs.Length > 1)
                    {
                        Console.WriteLine($"ambiguity for {outdatedPkg.PackageName} package - seems to be more version sub-dirs");
                        continue;
                    }
                    if (verDirs.Length == 1) outdatedPkg.InstalledVer = Path.GetFileName(verDirs[0]);

                    // start tasks for fetching the available version
                    var url = $"https://api.nuget.org/v3/registration3/{outdatedPkg.PackageName}/index.json";
                    outdatedPkg.processing.NugetApiGetTask = httpClient.GetAsync(url);
                    httpClientGetTasks.Add(outdatedPkg.processing.NugetApiGetTask);
                    outdatedPkg.processing.NugetApiGetTask.ContinueWith(t =>
                        {
                            // continue with async processing of the response message
                            HttpResponseMessage response = t.Result;
                            if (response.IsSuccessStatusCode)
                            {
                                outdatedPkg.processing.ResponseReadTask = response.Content.ReadAsAsync<PackageResponse>();
                                httpResponseReadTasks.Add(outdatedPkg.processing.ResponseReadTask);
                            }
                            else
                            {
                                Console.WriteLine($"No results found for {outdatedPkg.PackageName}");
                            }
                        }, TaskContinuationOptions.OnlyOnRanToCompletion).RunAsynchronously();
                }

                // await tasks for fetching the available version
                await Task.WhenAll(httpClientGetTasks);
                await Task.WhenAll(httpResponseReadTasks);

                // finish fetching the available version
                foreach (var outdatedPkg in outdatedPkgs)
                {
                    // no response read task at all => do not finish processing the available version
                    if (outdatedPkg.processing.ResponseReadTask == null) continue;

                    // check error/ cancellation of the response read task
                    if (!outdatedPkg.processing.ResponseReadTask.IsCompletedSuccessfully)
                    {
                        if (outdatedPkg.processing.ResponseReadTask.IsFaulted)
                        {
                            Console.WriteLine($"Error parsing the response for {outdatedPkg.PackageName}");
                        }
                        else if (outdatedPkg.processing.ResponseReadTask.IsCanceled)
                        {
                            Console.WriteLine($"Cancellation parsing the response for {outdatedPkg.PackageName}");
                        }
                        continue;
                    }

                    // the response read task was completed successfully
                    var pkgResponse = outdatedPkg.processing.ResponseReadTask.Result;

                    // set the available version
                    outdatedPkg.AvailableVer = pkgResponse.Items[0].Upper;

                    // since now all is fetched ok
                    outdatedPkg.processing.ProcessedOk = true;
                    resultsCnt++;

                    if (outdatedPkg.IsOutdated)
                    {
                        // the package is determined as outdated 
                        outdatedPkg.processing.ProcessedOkOutdated = true;
                        outdatedCnt++;
                    }
                }

                PrintOutdatedResults(outdatedPkgs);
            }

        }


        private void PrintOutdatedResults(List<OutdatedResponse> outdatedPkgs)
        {
            var fmt = !string.IsNullOrEmpty(Format) ? Format : "table";
            switch (fmt)
            {
                case "json":
                    PrintJson(outdatedPkgs);
                    break;
                case "xml":
                    PrintXml(outdatedPkgs);
                    break;
                case "table":
                default:
                    PrintTable(outdatedPkgs);
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


        private void PrintJson(List<OutdatedResponse> outdatedPkgs)
        {
            var resultsCnt = 0;
            using (var outStream = OpenOutputStream(true))
            using (var wr = new StreamWriter(outStream, GetOutputEncoding()))
            using (var jwr = new JsonTextWriter(wr))
            {
                jwr.Indentation = 2;
                jwr.Formatting = Newtonsoft.Json.Formatting.Indented;
                jwr.WriteStartObject();
                jwr.WritePropertyName("outdatedPackages");
                jwr.WriteStartArray();
                foreach (var outdatedPkg in outdatedPkgs)
                {
                    if (outdatedPkg.processing.ProcessedOkOutdated)
                    {
                        resultsCnt++;
                        jwr.WriteStartObject();
                        jwr.WritePropertyName("name");
                        jwr.WriteValue(outdatedPkg.PackageName);
                        jwr.WritePropertyName("installedVersion");
                        jwr.WriteValue(outdatedPkg.InstalledVer);
                        jwr.WritePropertyName("availableVersion");
                        jwr.WriteValue(outdatedPkg.AvailableVer);
                        jwr.WriteEndObject();
                    }
                }
                jwr.WriteEndArray();
                jwr.WriteEndObject();
            }
        }


        private void PrintXml(List<OutdatedResponse> outdatedPkgs)
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
                foreach (var outdatedPkg in outdatedPkgs)
                {
                    if (outdatedPkg.processing.ProcessedOkOutdated)
                    {
                        resultsCnt++;
                        xwr.WriteStartElement("package");
                        xwr.WriteAttributeString("name", outdatedPkg.PackageName);
                        xwr.WriteElementString("installedVersion", outdatedPkg.InstalledVer);
                        xwr.WriteElementString("availableVersion", outdatedPkg.AvailableVer);
                        xwr.WriteEndElement();
                    }
                }
                xwr.WriteEndElement();
                xwr.WriteEndDocument();
            }
        }


        private void PrintTable(List<OutdatedResponse> outdatedPkgs)
        {
            var maxNameLen = 0;
            var maxInstVerLen = 0;
            var maxAvailVerLen = 0;
            var resultsCnt = 0;

            // preparation for the paddings determination
            foreach (var outdatedPkg in outdatedPkgs)
            {
                if (outdatedPkg.processing.ProcessedOkOutdated)
                {
                    resultsCnt++;
                    if (maxNameLen < outdatedPkg.PackageName.Length) maxNameLen = outdatedPkg.PackageName.Length;
                    if (maxInstVerLen < outdatedPkg.InstalledVer.Length) maxInstVerLen = outdatedPkg.InstalledVer.Length;
                    if (maxAvailVerLen < outdatedPkg.AvailableVer.Length) maxAvailVerLen = outdatedPkg.AvailableVer.Length;
                }
            }

            if (resultsCnt > 0)
            {
                const string packageIdStr = "Package Id";
                const string installedVerStr = "Installed";
                const string availVerStr = "Available";
                if (maxNameLen < packageIdStr.Length) maxNameLen = packageIdStr.Length;
                if (maxInstVerLen < installedVerStr.Length) maxInstVerLen = installedVerStr.Length;
                if (maxAvailVerLen < availVerStr.Length) maxAvailVerLen = availVerStr.Length;


                using (var outStream = OpenOutputStream(true))
                using (var tw = new StreamWriter(outStream, GetOutputEncoding()))
                {
                    tw.WriteLine($"{packageIdStr.PadRight(maxNameLen)} {installedVerStr.PadRight(maxInstVerLen)} {availVerStr.PadRight(maxAvailVerLen)}");
                    tw.WriteLine(new string('-', maxNameLen + maxInstVerLen + maxAvailVerLen + 2));
                    foreach (var outdatedPkg in outdatedPkgs)
                    {
                        if (outdatedPkg.processing.ProcessedOkOutdated)
                        {
                            tw.WriteLine($"{outdatedPkg.PackageName.PadRight(maxNameLen)} {outdatedPkg.InstalledVer.PadRight(maxInstVerLen)} {outdatedPkg.AvailableVer.PadRight(maxAvailVerLen)}");
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