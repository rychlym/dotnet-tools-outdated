using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace DotNetToolsOutdated
{
    class Program
    {
        static int Main(string[] args)
        {
            return MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task<int> MainAsync(string[] args)
        {
            return await CommandLineApplication.ExecuteAsync<OutdatedCommand>(args);
        }
    }
}
