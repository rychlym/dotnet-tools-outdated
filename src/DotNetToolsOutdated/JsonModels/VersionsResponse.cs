using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotNetToolsOutdated.JsonModels
{
    public partial class VersionsResponse
    {
        [JsonProperty("versions")]
        public string[] Versions { get; set; }
    }
}

