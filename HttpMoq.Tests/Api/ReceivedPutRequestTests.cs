using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HttpMoq.Tests.Api
{
    public class ReceivedPutRequestTests : IAsyncLifetime
    {
        private MockApi _api;
        private Request _request;
        private HttpResponseMessage _response;

        public async Task InitializeAsync()
        {
            _api = new MockApi(5467);
            _request = _api.Put("/test")
                .ReturnJson(new { foo = "bar" });

            await _api.StartAsync();

            using var client = new HttpClient();
            _response = await client.PutAsync("http://localhost:5467/test", null);
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
            _response.StatusCode.Should().Be(200);
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
