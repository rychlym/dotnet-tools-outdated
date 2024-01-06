using System.Threading.Tasks;

namespace DotNetToolsOutdated.Models;

internal class OutdatedResponseProcessing
{
    public Task ApiGetTaskOkContinued;
    public Task<string> OkResponseReadTask;

    public string[] ResultVersionsResponse;
    public string ResultAvailableVer => (ResultVersionsResponse?.Length > 0) ? ResultVersionsResponse[^1] : "";
}

internal struct OutdatedResponseProcessingResult
{
    public bool ProcessedOk;
    public bool ProcessedOkOutdated;
}