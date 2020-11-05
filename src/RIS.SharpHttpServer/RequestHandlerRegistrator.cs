#region

using System;
using System.Collections.Generic;
using System.Net;

#endregion

namespace RIS.Net.Http
{
    public class RequestHandlerRegistrator
    {
        public RequestHandlerRegistrator(HttpMethod httpMethod)
        {
            HttpMethod = httpMethod;
        }

        public HttpMethod HttpMethod { get; }

        public Dictionary<string, Func<HttpListenerRequest, string>> Handlers { get; } =
            new Dictionary<string, Func<HttpListenerRequest, string>>();

        public Func<HttpListenerRequest, string> this[string path]
        {
            get => Handlers[path];
            set => Handlers[path] = value;
        }
    }
}