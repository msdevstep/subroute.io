using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Subroute.Core.Execution;

namespace Subroute.Api.Models.HttpActionResults
{
    public class ExecutionResponseActionResult : IHttpActionResult
    {
        private readonly ExecutionResponse _response;

        public ExecutionResponseActionResult(ExecutionResponse response)
        {
            _response = response;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_response.StatusCode)
            {
                ReasonPhrase = _response.StatusMessage,
                Content = new ByteArrayContent(_response.Body ?? new byte[0])
            };

            foreach (var header in _response.Headers)
            {
                var headerType = GetHeaderType(header.Key);

                if (headerType == HeaderType.Response)
                    response.Headers.TryAddWithoutValidation(header.Key, header.Value);
                else
                    response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return Task.FromResult(response);
        }

        private HeaderType GetHeaderType(string headerName)
        {
            switch (headerName?.ToLower(CultureInfo.CurrentCulture))
            {
                case "content-type":
                case "content-disposition":
                case "content-encoding":
                case "content-language":
                case "content-length":
                case "content-md5":
                case "content-range":
                case "expires":
                case "last-modified":
                case "allow":
                    return HeaderType.Content;
                default:
                    return HeaderType.Response;
            }
        }

        private enum HeaderType
        {
            Response,
            Content
        }
    }
}
