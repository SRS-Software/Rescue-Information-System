#region

using System.Net;
using System.Reflection;
using RestSharp;
using SRS.Utilities;

#endregion

namespace RIS.Core.Helper
{
    public static class RestSharpExtensions
    {
        public static bool Check(this IRestResponse _response)
        {
            if (_response.ErrorException != null)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response -> " + _response.ErrorMessage);
                return false;
            }

            if (_response.StatusCode != HttpStatusCode.OK)
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(),
                    "Error retrieving response -> " + _response.StatusDescription);
                return false;
            }

            if (string.IsNullOrEmpty(_response.Content))
            {
                Logger.WriteError(MethodBase.GetCurrentMethod(), "Error retrieving response -> No Data");
                return false;
            }

            return true;
        }
    }
}