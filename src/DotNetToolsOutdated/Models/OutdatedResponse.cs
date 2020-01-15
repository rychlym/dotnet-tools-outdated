using System.Diagnostics;

namespace DotNetToolsOutdated.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]

    public class OutdatedResponse
    {
        public string Directory;

        public string PackageName;

        public string CurrentVer;

        public string AvailableVer;

        internal OutdatedResponseProcessing processing;

        public bool IsOutdated => CompareVersions(CurrentVer, AvailableVer) == -1;


        public static int CompareVersions(string ver1, string ver2)
        {
            if (ver1 == null && ver2 == null) return 0;
            if (ver1 == null && ver2 != null) return -1;
            if (ver1 != null && ver2 == null) return 1;

            var len = ver1.Length;
            if (ver2.Length < len) len = ver2.Length;

            var i = 0;
            while (i < len)
            {
                if (ver1[i] < ver2[i]) return -1;
                if (ver1[i] > ver2[i]) return 1;
                i++;
            }
            return 0;

        }

        internal string DebuggerDisplay
        {
            get
            {
                var res = $"OutResp: \"{PackageName}\", current: {CurrentVer}, avail: {AvailableVer}";
                if (processing.ProcessedOkOutdated)
                {
                    res = res + ", ok OUTDATED";
                }
                else if (processing.ProcessedOk)
                {
                    res = res + ", ok";
                }
                return res;
            }
        }
    }
}