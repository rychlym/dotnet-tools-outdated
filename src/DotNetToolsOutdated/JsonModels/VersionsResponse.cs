using Newtonsoft.Json;

namespace DotNetToolsOutdated.JsonModels
{
    public partial class VersionsResponse
    {
        [JsonProperty("versions")]
        public string[] Versions { get; set; }
    }
}