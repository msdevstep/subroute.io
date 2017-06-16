using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Subroute.Core.Compression
{
    public class CompressionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await base.SendAsync(request, cancellationToken).ContinueWith(responseToCompleteTask =>
                {
                    // Get the response result so we can use to to create a new response with compressed content.
                    var response = responseToCompleteTask.Result;

                    // In rare cases the request property is not set on the response. So we can't check
                    // for an accept encoding header. Just return the response we have since we can't compress.
                    if (response.RequestMessage == null)
                        return response;

                    // In the event we have no acceptable encodings, we'll just return the original request.
                    if (response.RequestMessage.Headers.AcceptEncoding == null || !response.RequestMessage.Headers.AcceptEncoding.Any())
                        return response;

                    // Get the first acceptable encoding value to determine what type of compression to use.
                    var encodingType = response.RequestMessage.Headers.AcceptEncoding.First().Value;

                    // In some cases the content has not been set and will be null. In these cases, there
                    // is nothing to compress, so we'll just return the original response.
                    if (response.Content == null)
                        return response;

                    // Replace the original responses content with the compressed version of the original content.
                    response.Content = new CompressedContent(response.Content, encodingType);

                    return response;
                },
                TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}