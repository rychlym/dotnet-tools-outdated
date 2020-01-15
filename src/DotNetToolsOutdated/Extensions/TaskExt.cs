using System.Threading.Tasks;

namespace DotNetToolsOutdated.Extensions
{
    public static class TaskExt
    {
#pragma warning disable CC0057 // Unused parameters
        /// <summary>
        /// Runs the task asynchronously and forget it. This method doesn't do anything,
        /// it's just there to "hide" otherwise unused task objects when intended to forget them.
        /// </summary>
        /// <param name="task">The task.</param>
        public static void RunAsyncAndForget(this Task task)
#pragma warning restore CC0057 // Unused parameters
        {
        }
    }
}
