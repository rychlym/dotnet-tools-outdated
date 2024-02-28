using Xunit;

namespace DotNetToolsOutdated.Ut.Tests
{
    public class StringUtTests
    {
        [Fact(DisplayName = "ToNerdCaps null input returns null")]
        public void ToNerdCaps_Null_Empty()
        {
            var result = StringUt.ToNerdCaps(null);

            Assert.Null(result);
        }

        [Fact(DisplayName = "ToNerdCaps empty input returns empty string")]
        public void ToNerdCaps_Empty_Empty()
        {
            var result = StringUt.ToNerdCaps(string.Empty);

            Assert.Empty(result);
        }

        [Theory(DisplayName = "ToNerdCaps one non capital character string input returns the same string")]
        [InlineData("a")]
        [InlineData("b")]
        [InlineData("c")]
        [InlineData("z")]
        public void ToNerdCaps_OneCharNonCap_Same(string s)
        {
            var result = StringUt.ToNerdCaps(s);

            Assert.Equal(s, result);
        }

        [Theory(DisplayName = "ToNerdCaps one non capital character string input returns the same string")]
        [InlineData("A")]
        [InlineData("B")]
        [InlineData("C")]
        [InlineData("Z")]
        public void ToNerdCaps_OneCharCap_Same(string s)
        {
            var result = StringUt.ToNerdCaps(s);

            var expected = char.ToLower(s[0]).ToString();

            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "ToNerdCaps null input returns null")]
        public void IsRootPathTest()
        {
            var example = "HelloPeople";

            var result = StringUt.ToNerdCaps(example);

            var expected = "helloPeople";
            Assert.Equal(expected, result);
        }
    }
}