using HttpMoq.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpMoq
{
    /// <summary>
    /// Represents an API request, containing data to use in a response.
    /// </summary>
    public sealed class Request
    {
        internal string Path { get; }
        internal string Query { get; }
        internal string Method { get; }
        internal string Content { get; set; }
        internal string ContentType { get; set; }
        internal HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        
        private int _count;
        public int Count => _count;

        internal Request(string path, string method)
        {
            ThrowIfInvalidPath(path);
            ThrowIfInvalidMethod(method);

            var pathParts = path.Split('?');
            Path = pathParts[0];

            if (pathParts.Length > 1)
            {
                Query = pathParts[1];
            }

            Method = method;
        }

        internal async Task Handle(HttpContext context)
        {
            context.Response.ContentType = ContentType;
            context.Response.StatusCode = (int)StatusCode;

            if (Content != null)
            {
                var bytes = Encoding.UTF8.GetBytes(Content);
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        internal void Increment()
        {
            Interlocked.Increment(ref _count);
        }

        /// <summary>
        /// Sets the response body as application/json, with <paramref name="data"/> as the data.
        /// </summary>
        /// <param name="data">The value used as the JSON response.</param>
        /// <returns>The current <see cref="Request"/> instance.</returns>
        public Request ReturnJson(object data)
        {
            Content = JsonConvert.SerializeObject(data);
            ContentType = "application/json";

            return this;
        }

        /// <summary>
        /// Sets the response body as text/plain, with <paramref name="content"/> as the data.
        /// </summary>
        /// <param name="content">The value used as the response body.</param>
        /// <returns>The current <see cref="Request"/> instance.</returns>
        public Request ReturnText(string content)
        {
            Content = content;
            ContentType = "text/plain";

            return this;
        }

        /// <summary>
        /// Sets the response status code.
        /// </summary>
        /// <param name="statusCode">The status code to use in the response.</param>
        /// <returns>The current <see cref="Request"/> instance.</returns>
        public Request SetStatusCode(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;

            return this;
        }

        private static void ThrowIfInvalidPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
        }

        private static void ThrowIfInvalidMethod(string method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (!HttpMethods.IsConnect(method) &&
                !HttpMethods.IsDelete(method) &&
                !HttpMethods.IsGet(method) &&
                !HttpMethods.IsHead(method) &&
                !HttpMethods.IsOptions(method) &&
                !HttpMethods.IsPatch(method) &&
                !HttpMethods.IsPost(method) &&
                !HttpMethods.IsPut(method) &&
                !HttpMethods.IsTrace(method))
            {
                throw new InvalidMethodException(method);
            }
        }
    }
}
