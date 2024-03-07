using Xunit;

namespace DotNetToolsOutdated.Ut.Tests
{
    public class PathUtTests
    {
        [Fact(DisplayName = "IsRootPath null input returns true")]
        public void IsRootPath_Null_True()
        {
            var result = Utils.IsRootPath(null);

            Assert.True(result);
        }

        [Fact(DisplayName = "IsRootPath empty input returns false")]
        public void IsRootPath_Empty_False()
        {
            var result = Utils.IsRootPath(string.Empty);

            Assert.False(result);
        }

        [Fact(DisplayName = "IsRootPath starting with windows drive and slash returns true")]
        public void IsRootPath_StartsWithWinDriveSlash_True()
        {
            var result = Utils.IsRootPath(@"c:\");

            Assert.True(result);
        }

    }
}