using System;
using System.IO;
using System.Text;

namespace DotNetToolsOutdated.Ut;

public static class Utils
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

    public static string GetAppHomeConfigSubDir(string appDirName) => Path.Combine(GetHomeDir() ?? string.Empty, ".config", appDirName);

    public static string GetHomeDir() =>
        Environment.GetEnvironmentVariable(OperatingSystem.IsWindows() ? "USERPROFILE" : "HOME");

    public static bool CopyIfNotExists(string fileName, string sourceDir, string destDir)
    {
        var destFilePath = Path.Combine(destDir, fileName);
        if (!File.Exists(destFilePath))
        {
            var sourceFilePath = Path.Combine(sourceDir, fileName);
            File.Copy(sourceFilePath, destFilePath);
            return true;
        }

        return false;
    }

    public static void SetReadOnly(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        fileInfo.IsReadOnly = true;
    }
}
