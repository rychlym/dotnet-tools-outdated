using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace DotNetToolsOutdated
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await CommandLineApplication.ExecuteAsync<OutdatedCommand>(args);
        }
    }
}
