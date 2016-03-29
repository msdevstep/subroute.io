using System.Net;

namespace Subroute.Core.Exceptions
{
    public interface IStatusCodeException
    {
        HttpStatusCode StatusCode { get; }
        bool IsPublic { get; }
    }
}