using System.Collections.Generic;

namespace DotNetToolsOutdated.JsonModels;
public class LocalManifest
{
    public int version { get; set; }
    public bool isRoot { get; set; }
    public Dictionary<string, ToolDetail> tools { get; set; }
}

public class ToolDetail
{
    public string version { get; set; }
    public string[] commands { get; set; }
}
