using System.Collections.Generic;
using Subroute.Common.RequestFormatters;
using Subroute.Common.ResponseFormatters;

namespace Subroute.Common
{
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

        public static readonly List<IRequestFormatter> RequestFormatters = new List<IRequestFormatter>();
        public static readonly List<IResponseFormatter> ResponseFormatters = new List<IResponseFormatter>();
    }
}