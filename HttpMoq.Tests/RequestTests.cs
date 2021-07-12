using FluentAssertions;
using System.Net.Http;
using Xunit;

namespace HttpMoq.Tests
{
    public class RequestTests
    {
        [Fact]
        public void Json_GivenObject_SetsContentAndType()
        {
            var request = new Request("/test", HttpMethod.Get)
                .Json(new { foo = "bar" });

            request.ContentType.Should().Be("application/json");
            request.Content.Should().Be("{\"foo\":\"bar\"}");
        }

        [Fact]
        public void Text_GivenContent_SetsTextContent()
        {
            var request = new Request("/test", HttpMethod.Get)
                .Text("foo bar");

            request.ContentType.Should().Be("text/plain");
            request.Content.Should().Be("foo bar");
        }
    }
}