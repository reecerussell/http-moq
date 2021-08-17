using System.Collections.Generic;
using System.Linq;

namespace HttpMoq
{
    internal static class QueryStringHelper
    {
        public static IDictionary<string, string[]> Parse(string queryString)
        {
            var args = new Dictionary<string, string[]>();

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
                if (string.IsNullOrEmpty(set))
                {
                    continue;
                }

                var parts = set.Split('=');
                var name = parts[0];
                var value = parts[1];

                if (args.TryGetValue(name, out var values))
                {
                    args[name] = values.Append(value).ToArray();
                }
                else
                {
                    args[name] = new [] {value};
                }
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
                if (!test.TryGetValue(t.Key, out var values))
                {
                    return false;
                }

                if (t.Value.Length == 1 && t.Value[0] == "*")
                {
                    continue;
                }

                if (!IsEquivalent(t.Value, values))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsEquivalent(string[] patternArr, string[] testArr)
        {
            for (var i = 0; i < testArr.Length; i++)
            {
                var value = testArr[i];
                var found = false;

                for (var j = 0; j < patternArr.Length; j++)
                {
                    if (value == patternArr[j])
                    {
                        found = true;

                        continue;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
