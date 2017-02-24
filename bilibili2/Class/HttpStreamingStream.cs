using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace bilibili2.Class
{
    public sealed class HttpStreamingStream : IRandomAccessStreamWithContentType
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public ulong Position { get; private set; }

        private readonly ulong _size;
        public ulong Size
        {
            get { return _size; }
            set { throw new NotSupportedException(); }
        }

        public string ContentType { get; }

        readonly HttpClient _client;
        readonly Uri _uri;

        private HttpStreamingStream(HttpClient client, Uri uri, ulong size, string contentType)
        {
            _client = client;
            _uri = uri;
            _size = size;
            ContentType = contentType;
        }

        public IRandomAccessStream CloneStream() { throw new NotImplementedException(); }

        public void Dispose() { }

        public IAsyncOperation<bool> FlushAsync() { throw new NotSupportedException(); }

        public IInputStream GetInputStreamAt(ulong position) { throw new NotImplementedException(); }

        public IOutputStream GetOutputStreamAt(ulong position) { throw new NotSupportedException(); }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return AsyncInfo.Run<IBuffer, uint>(async (cancelToken, progress) =>
            {
                progress.Report(0);

                using (var request = new HttpRequestMessage(HttpMethod.Get, _uri))
                {
                    request.Headers.TryAppendWithoutValidation("Range", $"bytes={Position}-{Position + count}");
                    using (var response = await _client.SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (var content = response.Content)
                        using (var stream = await content.ReadAsInputStreamAsync())
                            return await stream.ReadAsync(buffer, count, options).AsTask(cancelToken, progress);
                    }
                }
            });
        }

        public void Seek(ulong position)
        {
            Position = position;
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer) { throw new NotSupportedException(); }

        static async Task<HttpStreamingStream> CreateAsyncInternal(HttpClient client, Uri uri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Head, uri))
            using (var response = await client.SendRequestAsync(request))
            {
                response.EnsureSuccessStatusCode();

                using (var content = response.Content)
                {
                    var size = content.Headers.ContentLength;
                    if (size == null)
                        throw new NotSupportedException("The requested Uri doesn't support Content-Length");

                    if (!response.Headers.TryGetValue("Accept-Ranges", out string acceptRanges) || acceptRanges != "bytes")
                        throw new NotSupportedException("The requested Uri may not support ranged request.");

                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    return new HttpStreamingStream(client, uri, size.Value, contentType);
                }
            }
        }

        public static IAsyncOperation<HttpStreamingStream> CreateAsync(HttpClient client, Uri uri)
        {
            return CreateAsyncInternal(client, uri).AsAsyncOperation();
        }
    }
}
