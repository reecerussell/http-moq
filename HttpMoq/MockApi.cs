using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpMoq
{
    public sealed class MockApi : IDisposable
    {
        private readonly IWebHost _host;
        private readonly List<Request> _requests = new();

        public MockApi(int port)
        {
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://+:{port}")
                .Configure(app =>
                {
                    app.Use(async (context, _) =>
                    {
                        var request = Find(context.Request.Path.Value, context.Request.QueryString.Value, context.Request.Method);
                        if (request == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "text/plain";

                            var bytes = Encoding.UTF8.GetBytes("No mock could be found to match this request.");
                            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

                            return;
                        }

                        request.Increment();
                        await request.Handle(context);
                    });
                })
                .Build();
        }

        public Request Get(string path)
        {
            var request = new Request(path, HttpMethod.Get);
            _requests.Add(request);

            return request;
        }

        public Request Post(string path)
        {
            var request = new Request(path, HttpMethod.Post);
            _requests.Add(request);
            
            return request;
        }

        public Request Put(string path)
        {
            var request = new Request(path, HttpMethod.Put);
            _requests.Add(request);

            return request;
        }

        internal Request Find(string path, string queryString, string method)
        {
            return _requests.FirstOrDefault(x => PathHelper.IsMatch(x.Path, path) && x.Method.ToString() == method &&
                                                 (x.Query == null || QueryStringHelper.IsMatch(x.Query, queryString)));
        }

        public void Remove(Request request)
        {
            if (!_requests.Remove(request))
            {
                throw new InvalidOperationException("This request does not exist in this MockApi.");
            }
        }

        public Task StartAsync()
        {
            return _host.StartAsync();
        }

        public Task StopAsync()
        {
            return _host.StopAsync();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
