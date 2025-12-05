using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetToolsOutdated.JsonModels;

public partial class OutJsonRoot
{
    [DataMember(Name = "ver")]
    public string Version { get; set; }

    public bool OutPkgRegardlessState { get; set; }

    [DataMember(Name = "dotnet-tools-outdated")]
    public List<OutdatedResponse> Packages { get; set; }
}


