using System;

namespace HttpMoq.Exceptions
{
    public class InvalidMethodException : Exception
    {
        public string Method { get; }

        public InvalidMethodException(string method)
            : base($"'{method}' is not a valid HTTP method.")
        {
            Method = method;
        }
    }
}
