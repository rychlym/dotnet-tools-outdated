using System.Diagnostics;
using System.Runtime.Serialization;
using DotNetToolsOutdated.Models;

namespace DotNetToolsOutdated.JsonModels;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class OutdatedResponse
{
    [DataMember(Order = 1)]
    public string PackageName { get; set; }

    [DataMember(Order = 2)]
    public bool IsOutdated => CompareVersions(CurrentVer, AvailableVer) == -1 || BecomeUnlisted;

    [DataMember(Order = 3)]
    public string CurrentVer { get; set; }

    [DataMember(Order = 4)]
    public string AvailableVer { get; set; }

    [DataMember(Order = 5)]
    public bool BecomeUnlisted => AvailableVer == "";

    [DataMember(Order = 6)]
    public string Directory { get; set; }

    [DataMember(Order = 7)]
    public LocalManifestRefInfo LocalManifestRefInfo;

    [IgnoreDataMember]
    internal OutdatedResponseProcessingResult processingResult;

    [IgnoreDataMember]
    internal string DebuggerDisplay
    {
        get
        {
            var typeStr = LocalManifestRefInfo == null ? "global" : "local";
            var res = $"OutResp: {typeStr} \"{PackageName}\", current: {CurrentVer}, avail: {AvailableVer}";
            if (processingResult.ProcessedOkOutdated)
            {
                res = res + ", ok OUTDATED";
            }
            else if (processingResult.ProcessedOk)
            {
                res = res + ", ok";
            }
            return res;
        }
    }

    public static int CompareVersions(string ver1, string ver2)
    {
        if (ver1 == null && ver2 == null) return 0;
        if (ver1 == null && ver2 != null) return -1;
        if (ver1 != null && ver2 == null) return 1;

        var ver1Parsed = Semver.SemVersion.TryParse(ver1, out var version1);
        var ver2Parsed = Semver.SemVersion.TryParse(ver2, out var version2);
        if (ver1Parsed && ver2Parsed)
        {
            if (version1 < version2) return -1;
            if (version1 > version2) return 1;
            return 0;
        }

        // a fall-back if parsing fails
        var ver1Arr = ver1.Split('.');
        var ver2Arr = ver2.Split('.');

        var len = ver1Arr.Length;
        if (ver2Arr.Length < len) len = ver2.Length;
        var i = 0;
        while (i < len)
        {
            // number comparison
            var num1Parsed = int.TryParse(ver1Arr[i], out var num1);
            var num2Parsed = int.TryParse(ver2Arr[i], out var num2);
            if (num1Parsed && num2Parsed)
            {
                if (num1 < num2) return -1;
                if (num1 > num2) return 1;
            }
            else
            {
                // string comparison
                var stringCmp = ver1Arr[i].CompareTo(ver2Arr[i]);
                if (stringCmp != 0) return stringCmp;
            }
            i++;
        }
        // array length comparison
        if (ver1Arr.Length < ver2Arr.Length) return -1;
        if (ver1Arr.Length > ver2Arr.Length) return 1;
        return 0;
    }
}