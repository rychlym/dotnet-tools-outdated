namespace DotNetToolsOutdated.JsonModels;

public class LocalManifestRefInfo
{
    public string FilePath { get; } 
    public int Version { get; } 
    public bool IsRoot { get; } 

    public LocalManifestRefInfo(string filePath, int version, bool isRoot)
    {
        FilePath = filePath;
        Version = version;
        IsRoot = isRoot;
    }
}
