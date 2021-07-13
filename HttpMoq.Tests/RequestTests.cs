using FluentAssertions;
using System.Net.Http;
using Xunit;

namespace HttpMoq.Tests
{
    public class RequestTests
    {
        [Fact]
        public void ReturnJson_GivenObject_SetsContentAndType()
        {
            var request = new Request("/test", HttpMethod.Get)
                .ReturnJson(new { foo = "bar" });

            request.ContentType.Should().Be("application/json");
            request.Content.Should().Be("{\"foo\":\"bar\"}");
        }

        [Fact]
        public void ReturnText_GivenContent_SetsTextContent()
        {
            var request = new Request("/test", HttpMethod.Get)
                .ReturnText("foo bar");

            request.ContentType.Should().Be("text/plain");
            request.Content.Should().Be("foo bar");
        }
    }
}