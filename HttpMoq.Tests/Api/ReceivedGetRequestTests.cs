using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HttpMoq.Tests.Api
{
    public class ReceivedGetRequestTests : IAsyncLifetime
    {
        private MockApi _api;
        private HttpResponseMessage _response;

        public async Task InitializeAsync()
        {
            _api = new MockApi(23496);
            _api.Get("/test")
                .Json(new { foo = "bar" })
                .SetStatusCode(HttpStatusCode.BadRequest);

            await _api.StartAsync();

            using var client = new HttpClient();
            _response = await client.GetAsync("http://localhost:23496/test");
        }

        public async Task DisposeAsync()
        {
            await _api.StopAsync();
            _api.Dispose();
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
}
