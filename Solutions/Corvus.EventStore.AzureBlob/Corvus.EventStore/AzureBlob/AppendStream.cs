// <copyright file="AppendStream.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureBlob
{
    using System;
    using System.IO;

    /// <summary>
    /// A stream which appends content at the end of a stream.
    /// </summary>
    internal class AppendStream : Stream
    {
        private readonly Stream underlyingStream;
        private readonly ReadOnlyMemory<byte> contentToPrepend;
        private readonly ReadOnlyMemory<byte> contentToAppend;
        private long position;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppendStream"/> class.
        /// </summary>
        /// <param name="underlyingStream">The stream to which content is to be appended.</param>
        /// <param name="contentToPrepend">The content to prepend to the stream.</param>
        /// <param name="contentToAppend">The content to append to the stream.</param>
        public AppendStream(Stream underlyingStream, ReadOnlyMemory<byte> contentToPrepend, ReadOnlyMemory<byte> contentToAppend)
        {
            this.underlyingStream = underlyingStream;
            this.contentToPrepend = contentToPrepend;
            this.contentToAppend = contentToAppend;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => this.underlyingStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => this.contentToPrepend.Length + this.underlyingStream.Length + this.contentToAppend.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.position < this.contentToPrepend.Length)
            {
                int start = (int)this.position;
                int bytesToRead = Math.Min(count, this.contentToPrepend.Length - start);
                this.contentToPrepend.Slice(start, bytesToRead).CopyTo(buffer);
                this.position += bytesToRead;
                return bytesToRead;
            }

            if (this.position < this.contentToPrepend.Length + this.underlyingStream.Length)
            {
                int bytesRead = this.underlyingStream.Read(buffer, offset, count);
                this.position += bytesRead;
                return bytesRead;
            }

            if (this.position < this.Length)
            {
                int start = (int)(this.position - (this.underlyingStream.Length + this.contentToPrepend.Length));
                int bytesToRead = Math.Min(count, this.contentToAppend.Length - start);
                this.contentToAppend.Slice(start, bytesToRead).CopyTo(buffer);
                this.position += bytesToRead;
                return bytesToRead;
            }

            return 0;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long positionToSeek = 0;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    positionToSeek = offset;
                    break;
                case SeekOrigin.Current:
                    positionToSeek = this.position + offset;
                    break;
                case SeekOrigin.End:
                    positionToSeek = this.Length - offset;
                    break;
                default:
                    break;
            }

            if (positionToSeek >= this.underlyingStream.Length)
            {
                this.underlyingStream.Seek(0, SeekOrigin.End);
                this.position = Math.Min(positionToSeek, this.Length);
            }
            else
            {
                this.underlyingStream.Seek(positionToSeek, SeekOrigin.Begin);
                this.position = positionToSeek;
            }

            return this.position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
