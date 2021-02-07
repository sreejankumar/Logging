using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Logging.Models
{
    public class OutputFilterStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly MemoryStream _copyStream;

        public OutputFilterStream(HttpContext httpContext)
        {
            //Copy a pointer to the original response body stream
            _innerStream = httpContext.Response.Body;

            //Create a new memory stream...
            _copyStream = new MemoryStream();

            //...and use that for the temporary response body
            httpContext.Response.Body = _copyStream;
        }

        public async Task<string> ReadStreamAsync(HttpResponse response)
        {
            if (_copyStream.Length <= 0L || !_copyStream.CanRead || !_copyStream.CanSeek)
            {
                return string.Empty;
            }

            _copyStream.Position = 0L;
            try
            {
                //We need to read the response stream from the beginning...
                response.Body.Seek(0, SeekOrigin.Begin);

                //...and copy it into a string
                var text = await new StreamReader(response.Body).ReadToEndAsync();

                //We need to reset the reader for the response so that the client can read it.
                response.Body.Seek(0, SeekOrigin.Begin);

                //Copy the contents of the new memory stream (which contains the response) to the original stream,
                //which is then returned to the client.
                await _copyStream.CopyToAsync(_innerStream);
                return text;
            }
            catch (Exception)
            {
                // ignored 
            }

            return string.Empty;
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;

        public override void Flush()
        {
            _innerStream.Flush();
        }
        public override long Length => _innerStream.Length;
        public override long Position
        {
            get => _innerStream.Position;
            set => _copyStream.Position = _innerStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _copyStream.Seek(offset, origin);
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _copyStream.SetLength(value);
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _copyStream.Write(buffer, offset, count);
            _innerStream.Write(buffer, offset, count);
        }
    }
}
