// <copyright file="ReadOnlyMemoryStream.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.IO
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Provides a <see cref="Stream"/> for the contents of a <see cref="ReadOnlyMemory{T}"/>.</summary>
    internal sealed class ReadOnlyMemoryStream : Stream
    {
        private readonly ReadOnlyMemory<byte> content;
        private int position;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemoryStream"/> class.
        /// </summary>
        /// <param name="content">The readonly memory over which to create this stream.</param>
        public ReadOnlyMemoryStream(ReadOnlyMemory<byte> content)
        {
            this.content = content;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => this.content.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => this.position;
            set
            {
                if (value < 0 || value > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.position = (int)value;
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos =
                origin == SeekOrigin.Begin ? offset :
                origin == SeekOrigin.Current ? this.position + offset :
                origin == SeekOrigin.End ? this.content.Length + offset :
                throw new ArgumentOutOfRangeException(nameof(origin));

            if (pos > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            else if (pos < 0)
            {
                throw new IOException("You cannot seek before the beginning of the stream");
            }

            this.position = (int)pos;
            return this.position;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            ReadOnlySpan<byte> s = this.content.Span;
            return this.position < s.Length ? s[this.position++] : -1;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateReadArrayArguments(buffer, offset, count);
            return this.Read(new Span<byte>(buffer, offset, count));
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            int remaining = this.content.Length - this.position;

            if (remaining <= 0 || buffer.Length == 0)
            {
                return 0;
            }
            else if (remaining <= buffer.Length)
            {
                this.content.Span.Slice(this.position).CopyTo(buffer);
                this.position = this.content.Length;
                return remaining;
            }
            else
            {
                this.content.Span.Slice(this.position, buffer.Length).CopyTo(buffer);
                this.position += buffer.Length;
                return buffer.Length;
            }
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateReadArrayArguments(buffer, offset, count);
            return cancellationToken.IsCancellationRequested ?
                Task.FromCanceled<int>(cancellationToken) :
                Task.FromResult(this.Read(new Span<byte>(buffer, offset, count)));
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            cancellationToken.IsCancellationRequested ?
                new ValueTask<int>(Task.FromCanceled<int>(cancellationToken)) :
                new ValueTask<int>(this.Read(buffer.Span));

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
            TaskToApm.Begin(this.ReadAsync(buffer, offset, count), callback, state);

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult) =>
            TaskToApm.End<int>(asyncResult);

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);
            if (this.content.Length > this.position)
            {
                destination.Write(this.content.Span.Slice(this.position));
            }
        }

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            StreamHelpers.ValidateCopyToArgs(this, destination, bufferSize);
            return this.content.Length > this.position ?
                destination.WriteAsync(this.content.Slice(this.position), cancellationToken).AsTask() :
                Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        private static void ValidateReadArrayArguments(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0 || buffer.Length - offset < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }
    }
}