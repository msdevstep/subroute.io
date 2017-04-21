using System.Collections.Generic;
using Subroute.Common.RequestFormatters;
using Subroute.Common.ResponseFormatters;

namespace Subroute.Common
{
    /// <summary>
    /// Class containing the specific configuration for the current route.
    /// This class can be used to register request and response formatters.
    /// </summary>
    public static class GlobalConfiguration
    {
        static GlobalConfiguration()
        {
            RequestFormatters.Add(new StringRequestFormatter());
            RequestFormatters.Add(new JsonRequestFormatter());
            RequestFormatters.Add(new XmlRequestFormatter());
            RequestFormatters.Add(new StreamRequestFormatter());
            RequestFormatters.Add(new PostRequestFormatter());

            ResponseFormatters.Add(new StringResponseFormatter());
            ResponseFormatters.Add(new JsonResponseFormatter());
            ResponseFormatters.Add(new XmlResponseFormatter());
            ResponseFormatters.Add(new StreamResponseFormatter());
        }

        /// <summary>
        /// Request formatters used to read data from the request stream.
        /// </summary>
        public static readonly List<IRequestFormatter> RequestFormatters = new List<IRequestFormatter>();

        /// <summary>
        /// Response formatters used to format data for the response stream.
        /// </summary>
        public static readonly List<IResponseFormatter> ResponseFormatters = new List<IResponseFormatter>();
    }
}