using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
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
        public string Path { get; }
        public HttpMethod Method { get; }
        public string Content { get; internal set; }
        public string ContentType { get; internal set; }
        public HttpStatusCode StatusCode { get; internal set; } = HttpStatusCode.OK;
        
        private int _count;
        public int Count => _count;

        internal Request(string path, HttpMethod method)
        {
            Path = path;
            Method = method;
        }

        internal async Task Handle(HttpContext context)
        {
            context.Response.ContentType = ContentType;
            context.Response.StatusCode = (int)StatusCode;

            var bytes = Encoding.UTF8.GetBytes(Content);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
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
    }
}
