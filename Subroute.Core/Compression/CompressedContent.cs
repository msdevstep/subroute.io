using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Subroute.Common;
using Subroute.Common.Exceptions;

namespace Subroute.Core.Compression
{
    public class CompressedContent : HttpContent
    {
        private readonly HttpContent _originalContent;
        private readonly string _encodingType;

        public CompressedContent(HttpContent content, string encodingType)
        {
            // Ensure we have content and an encoding type.
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (encodingType == null)
                throw new ArgumentNullException(nameof(encodingType));

            // Set the content and encoding to private fields to use when building the content later.
            _originalContent = content;
            _encodingType = encodingType.ToLowerInvariant();

            // We can only accept gzip or deflate encoding types. Throw exception for everything else.
            if (_encodingType != "gzip" && _encodingType != "deflate")
                throw new BadRequestException($"Encoding '{_encodingType}' is not supported. Only supports gzip or deflate encoding.");

            // Copy the headers from the original content
            foreach (var header in _originalContent.Headers)
                Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Add the encoding to the response headers.
            Headers.ContentEncoding.Add(encodingType);
        }

        protected override bool TryComputeLength(out long length)
        {
            // Length is set to -1 when using compressed content.
            length = -1;

            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Stream compressedStream = null;

            // Use the encoding type to determine what type of stream to use to return the content.
            switch (_encodingType)
            {
                case "gzip":
                    compressedStream = new GZipStream(stream, CompressionMode.Compress, true);
                    break;
                case "deflate":
                    compressedStream = new DeflateStream(stream, CompressionMode.Compress, true);
                    break;
            }

            // Return a copied stream and dispose of the stream when finished.
            return _originalContent.CopyToAsync(compressedStream).ContinueWith(tsk => compressedStream?.Dispose());
        }
    }
}