#region

using System;
using System.Net;

#endregion

namespace RIS.Net.Http
{
    public static class HttpListenerRequestExtensions
    {
        public static HttpMethod GetHttpMethod(this HttpListenerRequest request)
        {
            return (HttpMethod) Enum.Parse(typeof(HttpMethod), request.HttpMethod, true);
        }
    }
}