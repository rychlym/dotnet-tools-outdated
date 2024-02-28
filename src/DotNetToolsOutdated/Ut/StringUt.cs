using System.IO;
using System.Text;

namespace DotNetToolsOutdated.Ut
{
    public static class StringUt
    {
        public static string ToNerdCaps(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var sb = new StringBuilder(input);
            sb[0] = char.ToLower(input[0]);
            return sb.ToString();
        }

        public static bool IsRootPath(string path)
        {
            var rootPath = Path.GetPathRoot(path);
            return rootPath == path;
        }
    }
}
