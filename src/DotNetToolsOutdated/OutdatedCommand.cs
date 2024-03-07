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
using DotNetToolsOutdated.Ut;
using McMaster.Extensions.CommandLineUtils;
using Utf8Json;
using Utf8Json.Resolvers;

namespace DotNetToolsOutdated;

[Command(
    Name = "dotnet-tools-outdated",
    FullName = "dotnet-tools-outdated",
    Description = "Checks whether any of installed .NET command-line tools is outdated.",
    ExtendedHelpText = "\nIt checks globaly installed tools. It also checks locally installed tools, when being run from a folder of locally installed tools. The outdated global packages can be then re-installed by dotnet tool uninstall -g package_name followed by dotnet tool install -g package_name command. (For the locally installed packages, check the appropriate manifest file and use the above commands without the -g option)"
)]
[HelpOption]
[VersionOptionFromMember(MemberName = nameof(GetVersion))]
class OutdatedCommand
{
    [Option("-n|--name", "Check (and show) only one particular package", CommandOptionType.SingleValue)]
    public string PackageName { get; set; }

    [Option("-f|--format", "Output format. Valid values are xml, json, or table. (Default: table)", CommandOptionType.SingleValue)]
    public string Format { get; set; }

    [Option("--outPkgRegardlessState", "Otput the both up-to-date/outdated state packages for the json and xml format. (The table format can show only the outdated packages)", CommandOptionType.NoValue)]
    public bool OutPkgRegardlessState { get; set; }

    [Option("-ni|--noIndent", "No indenttation (for the json and xml format)", CommandOptionType.NoValue)]
    public bool NoIndent { get; set; }

    [Option("-o|--output", "Output file path. (Default: stdout)", CommandOptionType.SingleValue)]
    public string OutputPath { get; set; }

    [Option("--utf8", "Output with UTF-8 instead of the system default encoding. (no bom)", CommandOptionType.NoValue)]
    public bool IsUtf8 { get; set; }

    [Option("-pre|--prerelease", "Check also pre-released versions", CommandOptionType.NoValue)]
    public bool CheckPrelease { get; set; }

    [Option("-s|--showStat", "Show statistics info row (sums of available and outdated packages)", CommandOptionType.NoValue)]
    public bool ShowStats { get; set; }

    [Option("-gt|--globalToolsPath", "Use custom location of the globally installed .NET tools", CommandOptionType.SingleValue)]
    public string GlobalToolsPath { get; set; }

    private async Task OnExecuteAsync()
    {
        var httpClient = new HttpClient();
        var resultsCnt = 0;
        var outdatedCnt = 0;
        var unlistedCnt = 0;

        // prerelease parameter value
        var prereleasePar = CheckPrelease ? "true" : "false";
        var pkgs = new List<OutdatedResponse>();

        // local tools
        string currentDir = Environment.CurrentDirectory;
        while (currentDir != null)
        {
            if (Directory.Exists(currentDir))
            {
                var probedManifestSubDir = Path.Combine(currentDir, ".config");
                var probedManifestFilePath = Path.Combine(probedManifestSubDir, "dotnet-tools.json");
                if (File.Exists(probedManifestFilePath))
                {
                    // the local dotnet tools manifest file has been found => parse it
                    try
                    {
                        var manifestBytes = await File.ReadAllTextAsync(probedManifestFilePath);
                        var manifest = JsonSerializer.Deserialize<LocalManifest>(manifestBytes);
                        var manifestRef = new LocalManifestRefInfo(probedManifestFilePath, manifest.version, manifest.isRoot);
                        foreach (var entry in manifest.tools)
                        {
                            pkgs.Add(new OutdatedResponse() { PackageName = entry.Key, CurrentVer = entry.Value.version, Directory = currentDir, LocalManifestRefInfo = manifestRef });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Errog getting the manifest file: {probedManifestFilePath}. {ex.Message}");
                    }
                }
            }

            currentDir = Utils.IsRootPath(currentDir) ? null : Path.GetDirectoryName(currentDir);
        }
        var localPackagesCount = pkgs.Count;

        // global tools
        if (GlobalToolsPath == null)
        {
            var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            GlobalToolsPath = Path.Combine(userProfilePath, $".dotnet{Path.DirectorySeparatorChar}tools{Path.DirectorySeparatorChar}.store");
        }
        var dirs = Directory.GetDirectories(GlobalToolsPath);
        if (dirs != null)
        {
            // fetch the package name (1st phase)
            foreach (var dir in dirs)
            {
                var pkgName = Path.GetFileName(dir);
                if (pkgName.Equals(".stage", StringComparison.Ordinal)) continue;
                pkgs.Add(new OutdatedResponse() { PackageName = pkgName, Directory = dir });
            }
        }
        for (var i = localPackagesCount; i < pkgs.Count; i++)
        {
            // 2nd phase of populating fields for global packages
            var pkg = pkgs[i];

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
        }

        // prepare unique list of package names as the keys of the dictionary and then the values will be the tasks fetching the info
        Dictionary<string, OutdatedResponseProcessing> packageTasksDict = new();

        // tasks for awaiting when all done together
        var apiGetResponseOkContinuedTasks = new List<Task>();
        var pkgResponseReadTasks = new List<Task<string>>();
        foreach (var pkg in pkgs)
        {
            var packageName = pkg.PackageName;
            if (IsOnePackageModeAndPackageDontMatchChosen(packageName)) continue;

            // processing will be applied
            if (packageTasksDict.ContainsKey(packageName)) continue;

            var processing = new OutdatedResponseProcessing();
            packageTasksDict.Add(packageName, processing);

            // start tasks for fetching the available version
            var url = $"https://api-v2v3search-0.nuget.org/autocomplete?id={packageName}&prerelease={prereleasePar}";
            var pureHttpGetTask = httpClient.GetAsync(url);
            processing.ApiGetTaskOkContinued = pureHttpGetTask.ContinueWith(t =>
                {
                    // on run to completion continue with async processing of the response message ..
                    HttpResponseMessage response = t.Result;
                    if (response.IsSuccessStatusCode)
                    {
                        processing.OkResponseReadTask = response.Content.ReadAsStringAsync();
                        pkgResponseReadTasks.Add(processing.OkResponseReadTask);
                    }
                    else
                    {
                        Console.WriteLine($"No results found for {packageName}");
                    }
                },
                TaskContinuationOptions.OnlyOnRanToCompletion);
            // on faulted output a message to console
            pureHttpGetTask.ContinueWith(
                t => Console.WriteLine($"Task finding results for {packageName} has faulted!"),
                TaskContinuationOptions.OnlyOnFaulted)
            .RunAsyncAndForget();

            apiGetResponseOkContinuedTasks.Add(processing.ApiGetTaskOkContinued);
        }

        // await tasks for fetching the available version
        await Task.WhenAll(apiGetResponseOkContinuedTasks);
        // there should be same amount of response read tasks, which should be awaited
        await Task.WhenAll(pkgResponseReadTasks);

        // finish fetching the available version
        foreach (var key in packageTasksDict.Keys)
        {
            var processing = packageTasksDict[key];

            // no response read task at all => do not finish processing the available version
            if (processing.OkResponseReadTask == null) continue;

            // check error/ cancellation of the response read task
            if (!processing.OkResponseReadTask.IsCompletedSuccessfully)
            {
                if (processing.OkResponseReadTask.IsFaulted)
                {
                    Console.WriteLine($"Error parsing the response for {key}");
                }
                else if (processing.OkResponseReadTask.IsCanceled)
                {
                    Console.WriteLine($"Cancellation parsing the response for {key}");
                }
                continue;
            }

            // the response read task was completed successfully
            var versionsResponseStr = processing.OkResponseReadTask.Result;

            // set the available version
            var versionsResponse = JsonSerializer.Deserialize<VersionsResponse>(versionsResponseStr).Versions;
            processing.ResultVersionsResponse = versionsResponse;
        }

        foreach (var pkg in pkgs)
        {
            // no response read task at all => do not finish processing the available version
            var packageName = pkg.PackageName;
            if (IsOnePackageModeAndPackageDontMatchChosen(packageName)) continue;

            var processing = packageTasksDict[packageName];
            if (processing.OkResponseReadTask == null) continue;
            pkg.AvailableVer = processing.ResultAvailableVer;

            // since now all is fetched ok
            pkg.processingResult.ProcessedOk = true;
            resultsCnt++;

            if (pkg.IsOutdated)
            {
                // the package is determined as outdated 
                pkg.processingResult.ProcessedOkOutdated = true;
                if (pkg.BecomeUnlisted) unlistedCnt++;
                outdatedCnt++;
            }
        }

        var printedFmt = PrintOutdatedResults(pkgs);

        // statistics row
        if (ShowStats)
        {
            if (outdatedCnt > 0 || printedFmt != "table") Console.WriteLine();
            var localStr = localPackagesCount > 0 ? $"{localPackagesCount} local and " : "";
            var packageNameStr = string.IsNullOrEmpty(PackageName) ? "" : PackageName + " ";
            var globalPackagesCount = pkgs.Count - localPackagesCount;
            Console.WriteLine($"{localStr}{globalPackagesCount} global packages available. Found {outdatedCnt} outdated {packageNameStr}packages.");
        }
    }

    private bool IsOnePackageModeAndPackageDontMatchChosen(string packageName)
    {
        return (!string.IsNullOrEmpty(PackageName) && !PackageName.Equals(packageName, StringComparison.Ordinal));
    }

    private string PrintOutdatedResults(List<OutdatedResponse> pkgs)
    {
        var fmt = !string.IsNullOrEmpty(Format) ? Format : "table";
        var isConsoleOutput = string.IsNullOrEmpty(OutputPath);
        switch (fmt)
        {
            case "json":
                PrintJson(pkgs);
                if (isConsoleOutput) Console.WriteLine();
                break;
            case "xml":
                PrintXml(pkgs);
                if (isConsoleOutput) Console.WriteLine();
                break;
            case "table":
            default:
                PrintTable(pkgs);
                break;
        }
        return fmt;
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
        using (var wr = new BinaryWriter(outStream, outEncoding))
        {
            var packagesList = new List<OutdatedResponse>();
            foreach (var pkg in pkgs) if (IsPackageToPrintOut(pkg)) packagesList.Add(pkg);
            var packagesToSerialize = new OutJsonRoot() { Packages = packagesList, Version = GetVersion(), OutPkgRegardlessState = OutPkgRegardlessState };

            var resolver = StandardResolver.ExcludeNullCamelCase;
            var bytes = JsonSerializer.Serialize(packagesToSerialize, resolver);
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
        }
    }

    private void PrintXml(List<OutdatedResponse> pkgs)
    {
        var resultsCnt = 0;
        var xwrSettings = new XmlWriterSettings { Indent = !NoIndent, Encoding = GetOutputEncoding() };
        using (var outStream = OpenOutputStream(true))
        using (var xwr = XmlWriter.Create(outStream, xwrSettings))
        {
            xwr.WriteStartDocument();
            xwr.WriteStartElement("dotnet-tools-outdated");
            xwr.WriteAttributeString("version", GetVersion());
            xwr.WriteAttributeString("outPkgRegardlessState", Utils.ToNerdCaps(OutPkgRegardlessState.ToString()));
            foreach (var pkg in pkgs)
            {
                if (IsPackageToPrintOut(pkg))
                {
                    resultsCnt++;
                    xwr.WriteStartElement("package");
                    xwr.WriteAttributeString("name", pkg.PackageName);
                    xwr.WriteAttributeString("outdated", Utils.ToNerdCaps(pkg.IsOutdated.ToString()));
                    xwr.WriteElementString("currentVer", pkg.CurrentVer);
                    xwr.WriteElementString("availableVer", pkg.AvailableVer);
                    xwr.WriteElementString("becomeUnlisted", Utils.ToNerdCaps(pkg.BecomeUnlisted.ToString()));
                    xwr.WriteElementString("directory", pkg.Directory);
                    if (pkg.LocalManifestRefInfo != null)
                    {
                        xwr.WriteStartElement("localManifestRefInfo");
                        xwr.WriteAttributeString("filePath", pkg.LocalManifestRefInfo.FilePath);
                        xwr.WriteAttributeString("isRoot", Utils.ToNerdCaps(pkg.LocalManifestRefInfo.IsRoot.ToString()));
                        xwr.WriteAttributeString("version", pkg.LocalManifestRefInfo.Version.ToString());
                        xwr.WriteEndElement();
                    }
                    xwr.WriteEndElement();
                }
            }
            xwr.WriteEndElement();
            xwr.WriteEndDocument();
        }
    }

    /// <summary>
    /// Print info about the particular package out to the JSON or XML
    /// </summary>
    /// <param name="pkg">the package</param>
    private bool IsPackageToPrintOut(OutdatedResponse pkg)
    {
        return string.IsNullOrEmpty(PackageName)
            ? OutPkgRegardlessState || pkg.processingResult.ProcessedOkOutdated
            : PackageName.Equals(pkg.PackageName, StringComparison.Ordinal) && (OutPkgRegardlessState || pkg.processingResult.ProcessedOkOutdated);
    }

    private void PrintTable(List<OutdatedResponse> pkgs)
    {
        var maxNameLen = 0;
        var maxInstVerLen = 0;
        var maxAvailVerLen = 0;
        var maxLocalNameLen = 0;
        var maxLocalInstVerLen = 0;
        var maxLocalAvailVerLen = 0;
        var maxLocalManifestLen = 0;
        var resultsCnt = 0;

        // preparation for the paddings determination
        foreach (var pkg in pkgs)
        {
            if (pkg.processingResult.ProcessedOkOutdated)
            {
                resultsCnt++;
                if (pkg.LocalManifestRefInfo == null)
                {
                    if (maxNameLen < pkg.PackageName.Length) maxNameLen = pkg.PackageName.Length;
                    if (maxInstVerLen < pkg.CurrentVer.Length) maxInstVerLen = pkg.CurrentVer.Length;
                    if (maxAvailVerLen < pkg.AvailableVer.Length) maxAvailVerLen = pkg.AvailableVer.Length;
                }
                else
                {
                    if (maxLocalNameLen < pkg.PackageName.Length) maxLocalNameLen = pkg.PackageName.Length;
                    if (maxLocalInstVerLen < pkg.CurrentVer.Length) maxLocalInstVerLen = pkg.CurrentVer.Length;
                    if (maxLocalAvailVerLen < pkg.AvailableVer.Length) maxLocalAvailVerLen = pkg.AvailableVer.Length;
                    if (maxLocalManifestLen < pkg.LocalManifestRefInfo.FilePath.Length) maxLocalManifestLen = pkg.LocalManifestRefInfo.FilePath.Length;
                }
            }
        }

        if (resultsCnt > 0)
        {
            // headers
            const string packageIdStr = "Package Id";
            const string currentVerStr = "Current";
            const string availVerStr = "Available";
            const string manifestStr = "Manifest";
            if (maxNameLen < packageIdStr.Length) maxNameLen = packageIdStr.Length;
            if (maxInstVerLen < currentVerStr.Length) maxInstVerLen = currentVerStr.Length;
            if (maxAvailVerLen < availVerStr.Length) maxAvailVerLen = availVerStr.Length;
            if (maxLocalNameLen < packageIdStr.Length) maxLocalNameLen = packageIdStr.Length;
            if (maxLocalInstVerLen < currentVerStr.Length) maxLocalInstVerLen = currentVerStr.Length;
            if (maxLocalAvailVerLen < availVerStr.Length) maxLocalAvailVerLen = availVerStr.Length;
            if (maxLocalManifestLen < manifestStr.Length) maxLocalManifestLen = manifestStr.Length;

            using (var outStream = OpenOutputStream(true))
            using (var tw = new StreamWriter(outStream, GetOutputEncoding()))
            {
                var globalOutdated = 0;
                var localOutdated = 0;
                foreach (var outdatedPkg in pkgs)
                {
                    if (outdatedPkg.processingResult.ProcessedOkOutdated)
                    {
                        if (outdatedPkg.LocalManifestRefInfo != null)
                        {
                            if (localOutdated == 0)
                            {
                                // local packages header
                                tw.WriteLine($"{packageIdStr.PadRight(maxLocalNameLen)} {currentVerStr.PadRight(maxLocalInstVerLen)} {availVerStr.PadRight(maxLocalAvailVerLen)} {manifestStr}");
                                tw.WriteLine(new string('-', maxLocalNameLen + maxLocalInstVerLen + maxLocalAvailVerLen + maxLocalManifestLen + 3));
                            }

                            //local package
                            tw.WriteLine($"{outdatedPkg.PackageName.PadRight(maxLocalNameLen)} {outdatedPkg.CurrentVer.PadRight(maxLocalInstVerLen)} {outdatedPkg.AvailableVer.PadRight(maxLocalAvailVerLen)} {outdatedPkg.LocalManifestRefInfo.FilePath.PadRight(maxLocalManifestLen)}");
                            localOutdated++;
                        }
                        else
                        {
                            if (globalOutdated == 0)
                            {
                                // global packages header
                                if (localOutdated > 0) tw.WriteLine();
                                tw.WriteLine($"{packageIdStr.PadRight(maxNameLen)} {currentVerStr.PadRight(maxInstVerLen)} {availVerStr.PadRight(maxAvailVerLen)}");
                                tw.WriteLine(new string('-', maxNameLen + maxInstVerLen + maxAvailVerLen + 2));
                            }

                            //global package
                            tw.WriteLine($"{outdatedPkg.PackageName.PadRight(maxNameLen)} {outdatedPkg.CurrentVer.PadRight(maxInstVerLen)} {outdatedPkg.AvailableVer}");
                            globalOutdated++;
                        }
                    }
                }
            }
        }
    }

    Encoding GetOutputEncoding()
        => IsUtf8 ? new UTF8Encoding(false) : Encoding.Default;

    private static string GetVersion()
    {
        var infoVersion = typeof(OutdatedCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        var plusIndex = infoVersion.IndexOf('+');
        return plusIndex == -1 ? infoVersion : infoVersion[..plusIndex];
    }
}