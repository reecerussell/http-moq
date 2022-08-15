using HttpMoq.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HttpMoq;

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
    internal uint? Limit { get; set; }
    internal Func<string, bool> BodyValidator { get; private set; }

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

    /// <summary>
    /// Sets the limit on the number of times the request can be used.
    /// </summary>
    /// <param name="limit">A positive integer value for the limit.</param>
    /// <returns>The current <see cref="Request"/> instance.</returns>
    public Request SetLimit(uint limit)
    {
        Limit = limit;

        return this;
    }

    /// <summary>
    /// Used to set the body validator logic which is used when matching requests.
    /// This method allows a Request to be configured so that only if a requests body
    /// is valid, the Request is used.
    /// </summary>
    /// <param name="body">
    /// An action used to validate a request's body. The action is provide the body's content
    /// as a <see cref="string"/>, then the action should throw an exception if it's not valid.
    /// </param>
    /// <example>
    /// using FluentAssertions;
    /// 
    /// var request = new Request("/", "POST)
    ///     .EnsureBody(data =>
    ///     {
    ///         data.Should().Be("Hello World");
    ///     });
    /// </example>
    public Request EnsureBody(Action<string> body)
        => EnsureBody(s =>
        {
            try
            {
                body?.Invoke(s);
                return true;
            }
            catch
            {
                return false;
            }
        });

    /// <summary>
    /// Used to set the body validator logic which is used when matching requests.
    /// This method allows a Request to be configured so that only if a requests body
    /// is valid, the Request is used.
    ///
    /// A request's JSON body is parsed to a JObject which can be used to read the body.
    /// </summary>
    /// <param name="body">
    /// An action used to validate a request's body. The action is provide the body's content
    /// as a <see cref="JObject"/>, then the action should throw an exception if it's not valid.
    /// </param>
    /// <example>
    /// using FluentAssertions;
    /// 
    /// var request = new Request("/", "POST)
    ///     .EnsureBody(data =>
    ///     {
    ///         data.Value&lt;string&gt;("message").Should().Be("Hello World");
    ///     });
    /// </example>
    public Request EnsureBody(Action<JObject> body)
        => EnsureBody(content =>
        {
            try
            {
                var data = JObject.Parse(content);
                body?.Invoke(data);
                return true;
            }
            catch
            {
                return false;
            }
        });

    /// <summary>
    /// Used to set the body validator logic which is used when matching requests.
    /// This method allows a Request to be configured so that only if a requests body
    /// is valid, the Request is used.
    ///
    /// By specifying <typeparamref name="T"/>, a request's JSON body will be deserialized
    /// to the specified type, then the <paramref name="body"/> action can be used to validate it.
    /// </summary>
    /// <typeparam name="T">The type to deserialize a request's JSON body to.</typeparam>
    /// <param name="body">An action used to ensure the body is valid - this should throw if the body is not valid.</param>
    /// <example>
    /// using FluentAssertions;
    ///
    /// public class MyObject
    /// {
    ///     public string Message { get; set; }
    /// }
    /// 
    /// var request = new Request("/", "POST)
    ///     .EnsureBody&lt;MyObject&gt;(data =>
    ///     {
    ///         data.Message.Should().Be("Hello World");
    ///     });
    /// </example>
    public Request EnsureBody<T>(Action<T> body)
        => EnsureBody(content =>
        {
            try
            {
                var data = JsonConvert.DeserializeObject<T>(content);
                body?.Invoke(data);
                return true;
            }
            catch
            {
                return false;
            }
        });

    /// <summary>
    /// Used to set the body validator logic which is used when matching requests.
    /// This method allows a Request to be configured so that only if a requests body
    /// is valid, the Request is used.
    /// </summary>
    /// <param name="body">
    /// A function used to describe if a request's body is valid or not. Provided
    /// string data, a boolean should be returned if the body is valid, otherwise false.
    /// </param>
    /// <example>
    /// var request = new Request("/", "POST)
    ///     .EnsureBody(data => data == "Hello World");
    /// </example>
    public Request EnsureBody(Func<string, bool> body)
    {
        BodyValidator = body;

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
