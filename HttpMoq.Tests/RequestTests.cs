using System;
using System.Collections.Generic;
using FluentAssertions;
using System.Net.Http;
using System.Threading.Tasks;
using HttpMoq.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Xunit;

namespace HttpMoq.Tests;

public class RequestTests
{
    [Fact]
    public void ReturnJson_GivenObject_SetsContentAndType()
    {
        var request = new Request("/test", HttpMethods.Get)
            .ReturnJson(new { foo = "bar" });

        request.ContentType.Should().Be("application/json");
        request.Content.Should().Be("{\"foo\":\"bar\"}");
    }

    [Fact]
    public void ReturnText_GivenContent_SetsTextContent()
    {
        var request = new Request("/test", HttpMethods.Get)
            .ReturnText("foo bar");

        request.ContentType.Should().Be("text/plain");
        request.Content.Should().Be("foo bar");
    }

    [Fact]
    public void Increment_IncreasesCountBy1()
    {
        var request = new Request("/test", HttpMethods.Get);
        var count = request.Count;

        request.Increment();

        request.Count.Should().Be(count + 1);
    }

    [Fact]
    public void Increment_WhenCalledConcurrently_IncrementsSuccessfully()
    {
        var request = new Request("/test", HttpMethods.Get);
        var count = request.Count;

        Action increment = request.Increment;

        Parallel.Invoke(increment, increment, increment);

        request.Count.Should().Be(count + 3);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("not a method")]
    public void Constructor_GivenInvalidMethod_Throws(string method)
    {
        var ex = Assert.Throws<InvalidMethodException>(() => new Request("/", method));
        ex.Method.Should().Be(method);
    }

    [Fact]
    public void Constructor_GivenNullMethod_Throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Request("/", null));
        ex.ParamName.Should().Be("method");
    }

    [Fact]
    public void Constructor_GivenNullPath_Throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Request(null, HttpMethods.Get));
        ex.ParamName.Should().Be("path");
    }

    [Fact]
    public void EnsureBody_GivenStringActionThatDoesNotThrow_SetsAndReturnsTrue()
    {
        var actionHit = false;

        void BodyValidator(string _)
        {
            actionHit = true;
        }

        var request = new Request("/", "GET")
            .EnsureBody(BodyValidator);

        var result = request.BodyValidator("foo");
        result.Should().BeTrue();
        actionHit.Should().BeTrue();
    }

    [Fact]
    public void EnsureBody_GivenStringActionThatThrows_SetsAndReturnsFalse()
    {
        var actionHit = false;

        void BodyValidator(string _)
        {
            actionHit = true;

            throw new ArgumentException("invalid body");
        }

        var request = new Request("/", "GET")
            .EnsureBody(BodyValidator);

        var result = request.BodyValidator("foo");
        result.Should().BeFalse();
        actionHit.Should().BeTrue();
    }

    [Fact]
    public void EnsureBody_GivenJObjectActionThatDoesNotThrow_SetsAndReturnsTrue()
    {
        const string testBody = "{\"message\":\"hello\"}";

        var actionHit = false;

        void BodyValidator(JObject data)
        {
            actionHit = true;
            data.Value<string>("message").Should().Be("hello");
        }

        var request = new Request("/", "GET")
            .EnsureBody(BodyValidator);

        var result = request.BodyValidator(testBody);
        result.Should().BeTrue();
        actionHit.Should().BeTrue();
    }

    [Fact]
    public void EnsureBody_GivenJObjectActionThatThrows_SetsAndReturnsFalse()
    {
        const string testBody = "{\"message\":\"hello\"}";

        var actionHit = false;

        void BodyValidator(JObject data)
        {
            actionHit = true;
            data.Value<string>("message").Should().Be("goodbye");
        }

        var request = new Request("/", "GET")
            .EnsureBody(BodyValidator);

        var result = request.BodyValidator(testBody);
        result.Should().BeFalse();
        actionHit.Should().BeTrue();
    }

    [Fact]
    public void EnsureBody_GivenTActionThatDoesNotThrow_SetsAndReturnsTrue()
    {
        const string testBody = "{\"message\":\"hello\"}";

        var actionHit = false;

        void BodyValidator(Dictionary<string, string> data)
        {
            actionHit = true;
            data["message"].Should().Be("hello");
        }

        var request = new Request("/", "GET")
            .EnsureBody<Dictionary<string, string>>(BodyValidator);

        var result = request.BodyValidator(testBody);
        result.Should().BeTrue();
        actionHit.Should().BeTrue();
    }

    [Fact]
    public void EnsureBody_GivenTActionThatThrows_SetsAndReturnsFalse()
    {
        const string testBody = "{\"message\":\"hello\"}";

        var actionHit = false;

        void BodyValidator(Dictionary<string, string> data)
        {
            actionHit = true;
            data["message"].Should().Be("goodbye");
        }

        var request = new Request("/", "GET")
            .EnsureBody<Dictionary<string, string>>(BodyValidator);

        var result = request.BodyValidator(testBody);
        result.Should().BeFalse();
        actionHit.Should().BeTrue();
    }
}