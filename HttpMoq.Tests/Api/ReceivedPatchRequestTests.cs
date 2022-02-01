using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HttpMoq.Tests.Api
{
    public class ReceivedPatchRequestTests : IAsyncLifetime
    {
        private MockApi _api;
        private Request _request;
        private HttpResponseMessage _response;

        public async Task InitializeAsync()
        {
            _api = new MockApi();
            _request = _api.Patch("/test")
                .ReturnJson(new { foo = "bar" });

            await _api.StartAsync();
            
            _response = await _api.HttpClient.PatchAsync("test", null);
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
