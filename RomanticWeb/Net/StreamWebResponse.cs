using System;
using System.IO;
using System.Net;

namespace RomanticWeb.Net
{
    /// <summary>Mimics a <see cref="System.Net.WebResponse"/> like access to assembly embedded resources.</summary>
    public class StreamWebResponse : WebResponse
    {
        private readonly Stream _fileStream;
        private readonly Uri _responseUri;

        internal StreamWebResponse(Stream fileStream)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException("fileStream");
            }

            _fileStream = fileStream;
            _responseUri = new Uri("file:///");
        }

#if NETSTANDARD16
        /// <inheritdoc />
        public override long ContentLength { get { return _fileStream.Length; } }

        /// <inheritdoc />
        public override string ContentType { get { return "application/octet-stream"; } }
#endif

        /// <inheritdoc />
        public override Uri ResponseUri { get { return _responseUri; } }

        /// <summary>Gets a response stream with an embedded resource stream.</summary>
        /// <returns>An embedded resource stream.</returns>
        public override Stream GetResponseStream()
        {
            return _fileStream;
        }
    }
}