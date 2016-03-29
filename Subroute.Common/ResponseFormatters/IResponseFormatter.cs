using System;

namespace Subroute.Common.ResponseFormatters
{
    public interface IResponseFormatter
    {
        string Name { get; }

        byte[] WriteResponseBody(object payload);
    }
}