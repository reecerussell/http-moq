using System.Collections.Generic;

namespace HttpMoq
{
    internal static class QueryStringHelper
    {
        public static IDictionary<string, string> Parse(string queryString)
        {
            var args = new Dictionary<string, string>();

            if (queryString.Length < 1)
            {
                return args;
            }

            if (queryString[0] == '?')
            {
                queryString = queryString.Substring(1);
            }

            var sets = queryString.Split('&');

            foreach (var set in sets)
            {
                var parts = set.Split('=');

                args[parts[0]] = parts[1];
            }

            return args;
        }

        public static bool IsMatch(string pattern, string queryString)
        {
            var target = Parse(pattern);
            var test = Parse(queryString);

            if (target.Count != test.Count)
            {
                return false;
            }

            foreach (var t in target)
            {
                if (!test.TryGetValue(t.Key, out var testValue))
                {
                    return false;
                }

                if (t.Value != testValue && t.Value != "*")
                {
                    return false;
                }
            }

            return true;
        }
    }
}
