using FluentAssertions;
using Xunit;

namespace HttpMoq.Tests;

public class QueryStringHelperTests
{
    [Theory]
    [InlineData("?&hello=world&foo=bar")]
    [InlineData("?hello=world&&foo=bar")]
    [InlineData("?hello=world&&foo=bar&")]
    public void Parse_GivenValidQueryString_ReturnsSeparatedParams(string queryString)
    {
        var result = QueryStringHelper.Parse(queryString);

        result["hello"].Should().BeEquivalentTo("world");
        result["foo"].Should().BeEquivalentTo("bar");
    }

    [Fact]
    public void Parse_GivenEmptyQueryString_ReturnsEmptyResults()
    {
        var result = QueryStringHelper.Parse(string.Empty);
        result.Count.Should().Be(0);
    }
    
    [Fact]
    public void Parse_GivenNullQueryString_ReturnsEmptyResults()
    {
        var result = QueryStringHelper.Parse(null);
        result.Count.Should().Be(0);
    }

    [Fact]
    public void IsMatch_GivenValidPatternAndQueryString_ReturnsTrue()
    {
        const string pattern = "hello=world&foo=*";
        const string queryString = "hello=world&foo=bar";

        QueryStringHelper.IsMatch(pattern, queryString).Should().BeTrue();
    }

    [Fact]
    public void IsMatch_GivenQueryStringWithLessParams_ReturnsFalse()
    {
        const string pattern = "hello=world&foo=*";
        const string queryString = "hello=world";

        QueryStringHelper.IsMatch(pattern, queryString).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_GivenPatternWithLessParams_ReturnsFalse()
    {
        const string pattern = "hello=world";
        const string queryString = "hello=world&foo=bar";

        QueryStringHelper.IsMatch(pattern, queryString).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_GivenMultipleValuesMatchingWildcard_ReturnsTrue()
    {
        const string pattern = "hello=world&foo=*";
        const string queryString = "hello=world&foo=1&foo=1";

        QueryStringHelper.IsMatch(pattern, queryString).Should().BeTrue();
    }

    [Fact]
    public void IsMatch_WhereQueryStringDoesNotContainParam_ReturnsFalse()
    {
        const string pattern = "hello=world&foo=bar";
        const string queryString = "hello=world&bar=foo";

        QueryStringHelper.IsMatch(pattern, queryString).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_WhereValuesDoNotMatch_ReturnsFalse()
    {
        const string pattern = "hello=world&foo=bar";
        const string queryString = "hello=world&foo=foo";

        QueryStringHelper.IsMatch(pattern, queryString).Should().BeFalse();
    }

    [Fact]
    public void IsMatch_GivenMultipleValuesForSameKey_ReturnsTrue()
    {
        const string pattern = "foo=1&foo=2";
        const string queryString = "foo=2&foo=1";

        QueryStringHelper.IsMatch(pattern, queryString).Should().BeTrue();
    }
}
