using System.Threading.Tasks;
using DotNetToolsOutdated.JsonModels;

namespace DotNetToolsOutdated.Models
{
    internal struct OutdatedResponseProcessing
    {
        public Task ApiGetTaskOkContinued;
        public Task<string> OkResponseReadTask;

        public bool ProcessedOk;

        public bool ProcessedOkOutdated;
    }
}