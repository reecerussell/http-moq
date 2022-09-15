using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace HttpMoq.Tests;

public class MockApiTests
{
    [Fact]
    public void Remove_WhereRequestExists_RemovesFromApi()
    {
        var api = new MockApi(3454);
        var request = api.Get("/test");

        api.Remove(request);

        api.Find("/test", null, "GET").Should().BeNull();
    }

    [Fact]
    public void Remove_WhereRequestDoesNotExist_ThrowsInvalidOperationException()
    {
        var api = new MockApi(3454);
        var request = new Request("/test", HttpMethods.Get);

        Assert.Throws<InvalidOperationException>(() => api.Remove(request));
    }

    [Fact]
    public void Find_GivenMatchingPathAndQuery_ReturnsRequest()
    {
        var api = new MockApi(3454);
        var request = api.Get("/test?hello=world");
        
        api.Find("/test", "?hello=world", "GET").Should().Be(request);
    }

    [Fact]
    public void Find_GivenMatchingPathButNotQuery_ReturnsNull()
    {
        var api = new MockApi(3454);
        var request = api.Get("/test");

        api.Find("/test", null, "GET").Should().Be(request);
    }
    
    [Fact]
    public void Find_GivenMultipleRequestsWithSamePathButDifferentQueryParams_ReturnsTheExpectedRequest()
    {
        var api = new MockApi(9000);
        api.Get("/test");
        api.Get("/test?hello=hello");
        api.Get("/test?world=hello");
        api.Get("/test/world?hello=world");
        var request = api.Get("/test?hello=world");

        api.Find("/test", "?hello=world", "GET").Should().Be(request);
    }
    
    [Fact]
    public void Find_GivenMultipleRequestsWithTheSameFirstUriSegment_ReturnsTheExpectedRequest()
    {
        var api = new MockApi(9010);
        api.Get("/test");
        api.Get("/test/hello");
        api.Get("/test/world/hello");
        api.Get("/test/world?test=world");
        api.Get("/hello/test/world");
        var request = api.Get("/test/world");

        api.Find("/test/world", null, "GET").Should().Be(request);
    }

    [Fact]
    public void Expect_GivenValidMethodAndString_AddsRequestToApi()
    {
        const string method = "GET";
        const string path = "/hello";

        var api = new MockApi(32432);
        var request = api.Expect(method, path)
            .SetStatusCode(HttpStatusCode.Created);

        api.Find(path, null, method).Should().Be(request);
    }

    [Fact]
    public void Ctor_GivenPort_SetsPortAndUrl()
    {
        var api = new MockApi(8080);
        api.Port.Should().Be(8080);
        api.Url.Should().Be("http://localhost:8080");
    }
}
