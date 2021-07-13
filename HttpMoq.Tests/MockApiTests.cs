using FluentAssertions;
using System;
using System.Net.Http;
using Xunit;

namespace HttpMoq.Tests
{
    public class MockApiTests
    {
        [Fact]
        public void Remove_WhereRequestExists_RemovesFromApi()
        {
            var api = new MockApi(3454);
            var request = api.Get("/test");

            api.Remove(request);

            api.Find("/test", "GET").Should().BeNull();
        }

        [Fact]
        public void Remove_WhereRequestDoesNotExist_ThrowsInvalidOperationException()
        {
            var api = new MockApi(3454);
            var request = new Request("/test", HttpMethod.Get);

            Assert.Throws<InvalidOperationException>(() => api.Remove(request));
        }
    }
}
