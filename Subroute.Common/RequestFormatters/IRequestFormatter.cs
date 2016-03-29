using System;

namespace Subroute.Common.RequestFormatters
{
    public interface IRequestFormatter
    {
        string Name { get; }
        object ReadRequestBody(Type bodyType, byte[] body);
    }
}