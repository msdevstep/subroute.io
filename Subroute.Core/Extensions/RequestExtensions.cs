using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

namespace Subroute.Core.Extensions
{
    public static class RequestExtensions
    {
        public static string GetIpAddress(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;

            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }

            if (HttpContext.Current != null)
                return HttpContext.Current.Request.UserHostAddress;

            return null;
        }
    }
}