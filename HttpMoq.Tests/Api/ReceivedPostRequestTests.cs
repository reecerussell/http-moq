using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HttpMoq.Tests.Api;

public class ReceivedPostRequestTests
{
    public class WhereBodyMatches : IAsyncLifetime
    {
        private MockApi _api;
        private Request _request;
        private HttpResponseMessage _response;

        public async Task InitializeAsync()
        {
            const string testBody = "{\"message\":\"hi\"}";

            _api = new MockApi(34946);
            _request = _api.Post("/test")
                .EnsureBody(b => b.Value<string>("message").Should().Be("hi"))
                .ReturnJson(new { foo = "bar" })
                .SetStatusCode(HttpStatusCode.BadRequest);

            await _api.StartAsync();

            using var client = new HttpClient();
            _response = await client.PostAsync($"{_api.Url}/test", new StringContent(testBody));
        }

        public async Task DisposeAsync()
        {
            await _api.StopAsync();
            _api.Dispose();
        }

        [Fact]
        public void TheRequestCountIs1()
        {
            _request.Count.Should().Be(1);
        }

        [Fact]
        public void TheResponseStatusCodeIsCorrect()
        {
            _response.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task TheResponseContentIsCorrect()
        {
            _response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            var json = await _response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            data.Value<string>("foo").Should().Be("bar");
        }
    }

    public class WhereBodyDoesNotMatch : IAsyncLifetime
    {
        private MockApi _api;
        private Request _request;
        private HttpResponseMessage _response;

        public async Task InitializeAsync()
        {
            _api = new MockApi(34944);
            _request = _api.Post("/test")
                .EnsureBody(b => b.Should().Be("bar"))
                .ReturnJson(new { foo = "bar" })
                .SetStatusCode(HttpStatusCode.BadRequest);

            await _api.StartAsync();

            using var client = new HttpClient();
            _response = await client.PostAsync($"{_api.Url}/test", null);
        }

        public async Task DisposeAsync()
        {
            await _api.StopAsync();
            _api.Dispose();
        }

        [Fact]
        public void TheRequestCountIs1()
        {
            _request.Count.Should().Be(0);
        }

        [Fact]
        public void TheResponseStatusCodeIsCorrect()
        {
            _response.StatusCode.Should().Be(404);
        }
    }
}
