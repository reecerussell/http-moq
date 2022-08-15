using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("HttpMoq.Tests")]

namespace HttpMoq;

internal static class PathHelper
{
    public static IDictionary<string, string> ParseArgs(string template, string path)
    {
        var argMatches = Regex.Matches(template, "({[a-zA-Z]+})");
        var argNames = new string[argMatches.Count];

        for (var i = 0; i < argMatches.Count; i++)
        {
            var value = argMatches[i].Value;
            argNames[i] = value.Substring(1, value.Length - 2);
        }

        var pattern = Regex.Replace(template, "({[a-zA-Z]+})", "([a-zA-Z0-9-_]+)");
        var valueMatches = Regex.Matches(path, pattern);

        var values = new Dictionary<string, string>(valueMatches.Count);

        for (var i = 0; i < valueMatches.Count; i++)
        {
            values[argNames[i]] = valueMatches[i].Groups[1].Value;
        }

        return values;
    }

    public static bool IsMatch(string pattern, string path)
    {
        pattern = pattern.ToLower();
        path = path.ToLower();

        if (pattern.Length < 1 || pattern[0] != '/')
        {
            pattern = "/" + pattern;
        }

        if (path.Length < 1 || path[0] != '/')
        {
            path = "/" + path;
        }

        pattern = pattern.Replace("*", "(.*[^/])");

        return Regex.IsMatch(path, $"^{pattern}$");
    }
}