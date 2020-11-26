//using Newtonsoft.Json;

using System.Runtime.Serialization;

namespace DotNetToolsOutdated.JsonModels
{
    public partial class VersionsResponse
    {
        [DataMember(Name = "data")]
        public string[] Versions { get; set; }
    }
}

