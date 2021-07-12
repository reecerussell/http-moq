using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

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
                        var request = _requests.FirstOrDefault(r => PathHelper.IsMatch(r.Path, context.Request.Path)
                                                                    && r.Method.ToString() == context.Request.Method);
                        if (request == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "text/plain";

                            var bytes = Encoding.UTF8.GetBytes("No mock could be found to match this request.");
                            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

                            return;
                        }

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
