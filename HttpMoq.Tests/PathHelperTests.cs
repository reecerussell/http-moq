using FluentAssertions;
using Xunit;

namespace HttpMoq.Tests
{
    public class PathHelperTests
    {
        [Theory]
        [InlineData("/todo/{id}", "/todo/123", "id", "123")]
        [InlineData("/todo/{id}/complete", "/todo/123/complete", "id", "123")]
        public void ParseArgs_GivenOneArg_ReturnsValue(string template, string path, string argName, string expectedValue)
        {
            var args = PathHelper.ParseArgs(template, path);
            args[argName].Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("", "/")]
        [InlineData("/", "")]
        [InlineData("", "")]
        [InlineData("/", "/")]
        [InlineData("/hello/world", "Hello/World")]
        [InlineData("todo/*/details", "/todo/123/details")]
        public void IsMatch_GivenPatternAndMatchingPath_ReturnsTrue(string pattern, string path)
        {
            PathHelper.IsMatch(pattern, path).Should().BeTrue();
        }
        
        [Theory]
        [InlineData("/", "/hello")]
        [InlineData("/hello", "/hello/world")]
        [InlineData("/hello/*", "/hello")]
        [InlineData("/hello/world", "/hello/world/test")]
        [InlineData("/hello/*/world", "/hello/*/world/test")]
        public void IsMatch_GivenPatternAndDifferentPath_ReturnsFalse(string pattern, string path)
        {
            PathHelper.IsMatch(pattern, path).Should().BeFalse();
        }
    }
}
