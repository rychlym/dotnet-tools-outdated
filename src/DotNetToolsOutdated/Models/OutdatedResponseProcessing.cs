using System.Net.Http;
using System.Threading.Tasks;
using DotNetToolsOutdated.JsonModels;

namespace DotNetToolsOutdated.Models
{
    internal struct OutdatedResponseProcessing
    {
        public Task<HttpResponseMessage> NugetApiGetTask;
        public Task<PackageResponse> ResponseReadTask;

        public bool ProcessedOk;

        public bool ProcessedOkOutdated;
    }
}