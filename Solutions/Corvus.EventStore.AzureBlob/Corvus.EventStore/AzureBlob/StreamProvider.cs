// <copyright file="StreamProvider.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureBlob
{
    using System;
    using System.Buffers;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Provides a set of streams over an underlying stream,
    /// broken at a given byte sequence as a separator.
    /// </summary>
    internal struct StreamProvider : IDisposable
    {
        private readonly Stream underlyingStream;
        private readonly ReadOnlyMemory<byte> separator;
        private readonly int bufferSize;
        private readonly long underlyingStreamLength;
        private readonly IMemoryOwner<byte> bufferOwner;
        private int lastPosition;
        private int bytesRead;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamProvider"/> struct.
        /// </summary>
        /// <param name="underlyingStream">The underlying stream to break up at the blocks.</param>
        /// <param name="underlyingStreamLength">The length of the underlying stream.</param>
        /// <param name="separator">The block separator.</param>
        /// <param name="bufferSize">The size of the buffer to use.</param>
        public StreamProvider(Stream underlyingStream, long underlyingStreamLength, ReadOnlyMemory<byte> separator, int bufferSize)
        {
            this.underlyingStream = underlyingStream;
            this.separator = separator;
            this.bufferSize = bufferSize;
            this.underlyingStreamLength = underlyingStreamLength;
            this.bufferOwner = MemoryPool<byte>.Shared.Rent(this.bufferSize);
            this.lastPosition = 0;
            this.bytesRead = 0;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.bufferOwner.Dispose();
        }

        /// <summary>
        /// Gets the next stream up to the separator, or end of file.
        /// </summary>
        /// <param name="stream">The next stream.</param>
        /// <returns>True if there was a stream to read.</returns>
        public bool NextStream([NotNullWhen(true)]out Stream? stream)
        {
            Span<byte> buffer = this.bufferOwner.Memory.Span;
            int matchedSeparatorBytes = 0;
            ReadOnlySpan<byte> separatorSpan = this.separator.Span;
            var writer = new ArrayBufferWriter<byte>();
            while (this.lastPosition > 0 || this.underlyingStream.Position < this.underlyingStreamLength)
            {
                int offsetToSeparator = this.lastPosition;

                if (this.lastPosition == 0)
                {
                    // Alow ourselves a little bit of extra space in the buffer
                    this.bytesRead = this.underlyingStream.Read(buffer.Slice(0, buffer.Length - this.separator.Length));
                }

                for (int i = this.lastPosition; i < this.bytesRead; ++i)
                {
                    if (buffer[i] != separatorSpan[matchedSeparatorBytes])
                    {
                        // Just carry on, we're fine
                        matchedSeparatorBytes = 0;

                        // And we are at least i characters to the separator
                        offsetToSeparator = i + 1;
                    }
                    else
                    {
                        // We have matched the next separator character.
                        // If this is the first separator, extend the bytes read to ensure that
                        // we have the complete separator in this block
                        if (matchedSeparatorBytes == 0)
                        {
                            this.bytesRead += this.ExtendBuffer(this.bytesRead, buffer, i);
                        }

                        matchedSeparatorBytes += 1;

                        if (matchedSeparatorBytes == this.separator.Length)
                        {
                            break;
                        }
                    }
                }

                // If we had any bytes, write them to the output stream
                if (offsetToSeparator > this.lastPosition)
                {
                    writer.Write(buffer.Slice(this.lastPosition, offsetToSeparator - this.lastPosition));
                }

                if (offsetToSeparator + this.separator.Length < this.bytesRead)
                {
                    this.lastPosition = offsetToSeparator + this.separator.Length;
                }
                else
                {
                    this.lastPosition = 0;
                }

                // If we matched the whole separator, break
                if (matchedSeparatorBytes == this.separator.Length)
                {
                    break;
                }
            }

            if (writer.WrittenCount > 0)
            {
                stream = new ReadOnlyMemoryStream(writer.WrittenMemory);
                return true;
            }

            stream = null;
            return false;
        }

        private int ExtendBuffer(int bytesRead, Span<byte> buffer, int position)
        {
            int totalRead = 0;

            // We only need to extend if we have insufficient bytes read left to
            // capture
            if (position > bytesRead - this.separator.Length)
            {
                while (totalRead < (this.separator.Length - 1) && this.underlyingStream.Position < this.underlyingStreamLength)
                {
                    totalRead += this.underlyingStream.Read(buffer.Slice(bytesRead, this.separator.Length - (1 + totalRead)));
                }
            }

            return totalRead;
        }
    }
}
