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

                    // In the event we have no acceptable encodings, we'll just return the original request.
                    if (response.RequestMessage.Headers.AcceptEncoding == null || !response.RequestMessage.Headers.AcceptEncoding.Any())
                        return response;

                    // Get the first acceptable encoding value to determine what type of compression to use.
                    var encodingType = response.RequestMessage.Headers.AcceptEncoding.First().Value;

                    // Replace the original responses content with the compressed version of the original content.
                    response.Content = new CompressedContent(response.Content, encodingType);

                    return response;
                },
                TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}