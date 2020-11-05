#region

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

#endregion

namespace RIS.Net.Http
{
    public class HttpServer : IDisposable
    {
        private readonly HttpListener listener = new HttpListener();

        private readonly int port;
        private readonly Router router;

        public HttpServer(int port)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            router = new Router();

            this.port = port;
        }

        public string Hostname { get; set; } = "localhost";

        public string Scheme { get; set; } = "http";

        public string BaseUrl => BuildUri();

        public RequestHandlerRegistrator Get => router.GetRegistrator(HttpMethod.Get);

        public RequestHandlerRegistrator Post => router.GetRegistrator(HttpMethod.Post);

        public RequestHandlerRegistrator Put => router.GetRegistrator(HttpMethod.Put);

        public RequestHandlerRegistrator Delete => router.GetRegistrator(HttpMethod.Delete);

        public RequestHandlerRegistrator Options => router.GetRegistrator(HttpMethod.Options);

        public RequestHandlerRegistrator Patch => router.GetRegistrator(HttpMethod.Patch);

        public RequestHandlerRegistrator Head => router.GetRegistrator(HttpMethod.Head);

        public RequestHandlerRegistrator Connect => router.GetRegistrator(HttpMethod.Connect);

        public RequestHandlerRegistrator Trace => router.GetRegistrator(HttpMethod.Trace);

        public void Run()
        {
            router.GetAllRoutes()
                .ToList()
                .ForEach(path =>
                {
                    var query = "";
                    if (path.Contains("?"))
                    {
                        query = path.Substring(path.IndexOf("?") + 1);
                        path = path.Substring(0, path.IndexOf("?"));
                    }

                    if (!path.EndsWith("/"))
                        path += "/";
                    listener.Prefixes.Add(BuildUri(path, query));
                });

            listener.Start();

            ThreadPool.QueueUserWorkItem(o =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (listener.IsListening)
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                var handler = router.FindHandler(ctx.Request);
                                if (handler == null)
                                    Respond404(ctx);
                                else
                                    ProcessRequest(ctx, handler);
                            }
                            catch
                            {
                            } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, listener.GetContext());
                }
                catch
                {
                } // suppress any exceptions
            });
        }

        public void Stop()
        {
            listener.Stop();
            listener.Close();
        }

        public void ServeStatic(DirectoryInfo directory, string path = "")
        {
            router.ServeStatic(directory, path);
        }

        private string BuildUri(string path = "", string query = "")
        {
            return new UriBuilder(Scheme, Hostname, port, path, query).ToString();
        }

        private void ProcessRequest(HttpListenerContext ctx, Func<HttpListenerRequest, string> handler)
        {
            try
            {
                var response = handler(ctx.Request);
                Respond200(ctx, response);
            }
            catch (Exception)
            {
                Respond500(ctx);
            }
        }

        public void Respond200(HttpListenerContext ctx, string content)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.StatusDescription = "The request was fulfilled.";
            var buf = Encoding.UTF8.GetBytes(content);
            ctx.Response.ContentLength64 = buf.Length;
            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
        }

        public void Respond404(HttpListenerContext ctx)
        {
            ctx.Response.StatusCode = 404;
            ctx.Response.StatusDescription = "The server has not found anything matching the URI given.";
        }

        public void Respond500(HttpListenerContext ctx)
        {
            ctx.Response.StatusCode = 500;
            ctx.Response.StatusDescription =
                "The server encountered an unexpected condition which prevented it from fulfilling the request.";
        }

        #region IDisposable Support

        private bool disposed; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    if (listener.IsListening)
                        Stop();

                disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}