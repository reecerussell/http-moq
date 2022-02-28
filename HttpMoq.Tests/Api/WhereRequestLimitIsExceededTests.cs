using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HttpMoq.Tests.Api
{
    public class WhereRequestLimitIsExceededTests : IAsyncLifetime
    {
        private MockApi _api;
        private Request _request;
        private HttpResponseMessage _response1;
        private HttpResponseMessage _response2;

        public async Task InitializeAsync()
        {
            _api = new MockApi(23437);
            _request = _api.Get("/test")
                .ReturnJson(new { foo = "bar" })
                .SetStatusCode(HttpStatusCode.OK)
                .SetLimit(1);

            await _api.StartAsync();
            
            _response1 = await _api.HttpClient.GetAsync("/test");
            _response2 = await _api.HttpClient.GetAsync("/test");
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
        public void TheResponseStatusCodesAreCorrect()
        {
            _response1.StatusCode.Should().Be(200);
            _response2.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task TheResponseContentIsCorrect()
        {
            _response1.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            var json = await _response1.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);

            data.Value<string>("foo").Should().Be("bar");
        }
    }
}
