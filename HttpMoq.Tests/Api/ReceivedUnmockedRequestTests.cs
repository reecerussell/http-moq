using FluentAssertions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HttpMoq.Tests.Api
{
    public class ReceivedUnmockedRequestTests : IAsyncLifetime
    {
        private MockApi _api;
        private HttpResponseMessage _response;

        public async Task InitializeAsync()
        {
            _api = new MockApi(46345);

            await _api.StartAsync();
            
            _response = await _api.HttpClient.GetAsync("/test");
        }

        public async Task DisposeAsync()
        {
            await _api.StopAsync();
            _api.Dispose();
        }

        [Fact]
        public void TheResponseStatusCodeIsCorrect()
        {
            _response.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task TheResponseContentIsCorrect()
        {
            _response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");

            var text = await _response.Content.ReadAsStringAsync();
            text.Should().Be("No mock could be found to match this request.");
        }
    }
}
