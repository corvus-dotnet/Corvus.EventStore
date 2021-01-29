// <copyright file="Utf8JsonStreamReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

#pragma warning disable SA1201, SA1202, SA1600, CS1591

namespace System.Text.Json
{
    using System.Buffers;
    using System.IO;

    /// <summary>
    /// Replacement for <see cref="Utf8JsonReader"/> that operates over a stream.
    /// </summary>
    /// <remarks>
    /// Based on example code from https://stackoverflow.com/questions/54983533/parsing-a-json-file-with-net-core-3-0-system-text-json
    /// which in itself is based on the code in Utf8JsonReader in the dotnet repository.
    /// </remarks>
    public ref struct Utf8JsonStreamReader
    {
        private readonly Stream stream;
        private readonly int bufferSize;

        private SequenceSegment? firstSegment;
        private int firstSegmentStartIndex;
        private SequenceSegment? lastSegment;
        private int lastSegmentEndIndex;

        private Utf8JsonReader jsonReader;
        private bool keepBuffers;
        private bool isFinalBlock;

        /// <summary>
        /// Constructs an instance of a <see cref="Utf8JsonStreamReader"/> from a stream, for a given size of buffer.
        /// </summary>
        /// <param name="stream">The stream from which to read the JSON elements.</param>
        /// <param name="bufferSize">The default size of the buffer to use when reading.</param>
        public Utf8JsonStreamReader(Stream stream, int bufferSize)
        {
            this.stream = stream;
            this.bufferSize = bufferSize;

            this.firstSegment = null;
            this.firstSegmentStartIndex = 0;
            this.lastSegment = null;
            this.lastSegmentEndIndex = -1;

            this.jsonReader = default;
            this.keepBuffers = false;
            this.isFinalBlock = false;
        }

        /// <summary>
        /// Reads the next token from the input stream.
        /// </summary>
        /// <returns><c>true</c> if the token was successfully read, otherwise <c>false</c>.</returns>
        public bool Read()
        {
            // read could be unsuccessful due to insufficient bufer size, retrying in loop with additional buffer segments
            while (!this.jsonReader.Read())
            {
                if (this.isFinalBlock)
                {
                    return false;
                }

                this.MoveNext();
            }

            return true;
        }

        private void MoveNext()
        {
            SequenceSegment? firstSegment = this.firstSegment;
            this.firstSegmentStartIndex += (int)this.jsonReader.BytesConsumed;

            // release previous segments if possible
            if (!this.keepBuffers)
            {
                while (firstSegment?.Memory.Length <= this.firstSegmentStartIndex)
                {
                    this.firstSegmentStartIndex -= firstSegment.Memory.Length;
                    firstSegment.Dispose();
                    firstSegment = (SequenceSegment?)firstSegment.Next;
                }
            }

            // create new segment
            var newSegment = new SequenceSegment(this.bufferSize, this.lastSegment);

            if (firstSegment != null)
            {
                this.firstSegment = firstSegment;
                newSegment.Previous = this.lastSegment;
                this.lastSegment?.SetNext(newSegment);
                this.lastSegment = newSegment;
            }
            else
            {
                this.firstSegment = this.lastSegment = newSegment;
                this.firstSegmentStartIndex = 0;
            }

            // read data from stream
            this.lastSegmentEndIndex = this.stream.Read(newSegment.Buffer.Memory.Span);
            this.isFinalBlock = this.lastSegmentEndIndex < newSegment.Buffer.Memory.Length;
            this.jsonReader = new Utf8JsonReader(new ReadOnlySequence<byte>(this.firstSegment, this.firstSegmentStartIndex, this.lastSegment, this.lastSegmentEndIndex), this.isFinalBlock, this.jsonReader.CurrentState);
        }

        /// <summary>
        /// Deserialize the entity at the current location.
        /// </summary>
        /// <typeparam name="T">The type of the entity to deserialize.</typeparam>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when deserializing.</param>
        /// <returns>An instance of the object deserialized.</returns>
        public T Deserialize<T>(JsonSerializerOptions? options = null)
        {
            // JsonSerializer.Deserialize can read only a single object. We have to extract
            // object to be deserialized into separate Utf8JsonReader. This incures one additional
            // pass through data (but data is only passed, not parsed).
            long tokenStartIndex = this.jsonReader.TokenStartIndex;
            SequenceSegment? firstSegment = this.firstSegment;
            int firstSegmentStartIndex = this.firstSegmentStartIndex;

            // loop through data until end of object is found
            this.keepBuffers = true;
            int depth = 0;

            if (this.TokenType == JsonTokenType.StartObject || this.TokenType == JsonTokenType.StartArray)
            {
                depth++;
            }

            while (depth > 0 && this.Read())
            {
                if (this.TokenType == JsonTokenType.StartObject || this.TokenType == JsonTokenType.StartArray)
                {
                    depth++;
                }
                else if (this.TokenType == JsonTokenType.EndObject || this.TokenType == JsonTokenType.EndArray)
                {
                    depth--;
                }
            }

            this.keepBuffers = false;

            // end of object found, extract json reader for deserializer
            var newJsonReader = new Utf8JsonReader(new ReadOnlySequence<byte>(firstSegment!, firstSegmentStartIndex, this.lastSegment!, this.lastSegmentEndIndex).Slice(tokenStartIndex, this.jsonReader.Position), true, default);

            // deserialize value
            T result = JsonSerializer.Deserialize<T>(ref newJsonReader, options);

            // release memory if possible
            firstSegmentStartIndex = this.firstSegmentStartIndex + (int)this.jsonReader.BytesConsumed;

            while (firstSegment?.Memory.Length < firstSegmentStartIndex)
            {
                firstSegmentStartIndex -= firstSegment.Memory.Length;
                firstSegment.Dispose();
                firstSegment = (SequenceSegment?)firstSegment.Next;
            }

            if (firstSegment != this.firstSegment)
            {
                this.firstSegment = firstSegment;
                this.firstSegmentStartIndex = firstSegmentStartIndex;
                this.jsonReader = new Utf8JsonReader(new ReadOnlySequence<byte>(this.firstSegment!, this.firstSegmentStartIndex, this.lastSegment!, this.lastSegmentEndIndex), this.isFinalBlock, this.jsonReader.CurrentState);
            }

            return result;
        }

        public void Dispose() => this.lastSegment?.Dispose();

        public int CurrentDepth => this.jsonReader.CurrentDepth;

        public bool HasValueSequence => this.jsonReader.HasValueSequence;

        public long TokenStartIndex => this.jsonReader.TokenStartIndex;

        public JsonTokenType TokenType => this.jsonReader.TokenType;

        public ReadOnlySequence<byte> ValueSequence => this.jsonReader.ValueSequence;

        public ReadOnlySpan<byte> ValueSpan => this.jsonReader.ValueSpan;

        public bool GetBoolean() => this.jsonReader.GetBoolean();

        public byte GetByte() => this.jsonReader.GetByte();

        public byte[] GetBytesFromBase64() => this.jsonReader.GetBytesFromBase64();

        public string GetComment() => this.jsonReader.GetComment();

        public DateTime GetDateTime() => this.jsonReader.GetDateTime();

        public DateTimeOffset GetDateTimeOffset() => this.jsonReader.GetDateTimeOffset();

        public decimal GetDecimal() => this.jsonReader.GetDecimal();

        public double GetDouble() => this.jsonReader.GetDouble();

        public Guid GetGuid() => this.jsonReader.GetGuid();

        public short GetInt16() => this.jsonReader.GetInt16();

        public int GetInt32() => this.jsonReader.GetInt32();

        public long GetInt64() => this.jsonReader.GetInt64();

        public sbyte GetSByte() => this.jsonReader.GetSByte();

        public float GetSingle() => this.jsonReader.GetSingle();

        public string GetString() => this.jsonReader.GetString();

        public uint GetUInt32() => this.jsonReader.GetUInt32();

        public ulong GetUInt64() => this.jsonReader.GetUInt64();

        public bool TryGetDecimal(out byte value) => this.jsonReader.TryGetByte(out value);

        public bool TryGetBytesFromBase64(out byte[] value) => this.jsonReader.TryGetBytesFromBase64(out value);

        public bool TryGetDateTime(out DateTime value) => this.jsonReader.TryGetDateTime(out value);

        public bool TryGetDateTimeOffset(out DateTimeOffset value) => this.jsonReader.TryGetDateTimeOffset(out value);

        public bool TryGetDecimal(out decimal value) => this.jsonReader.TryGetDecimal(out value);

        public bool TryGetDouble(out double value) => this.jsonReader.TryGetDouble(out value);

        public bool TryGetGuid(out Guid value) => this.jsonReader.TryGetGuid(out value);

        public bool TryGetInt16(out short value) => this.jsonReader.TryGetInt16(out value);

        public bool TryGetInt32(out int value) => this.jsonReader.TryGetInt32(out value);

        public bool TryGetInt64(out long value) => this.jsonReader.TryGetInt64(out value);

        public bool TryGetSByte(out sbyte value) => this.jsonReader.TryGetSByte(out value);

        public bool TryGetSingle(out float value) => this.jsonReader.TryGetSingle(out value);

        public bool TryGetUInt16(out ushort value) => this.jsonReader.TryGetUInt16(out value);

        public bool TryGetUInt32(out uint value) => this.jsonReader.TryGetUInt32(out value);

        public bool TryGetUInt64(out ulong value) => this.jsonReader.TryGetUInt64(out value);

        public bool Match(string text) => this.jsonReader.ValueTextEquals(text);

        public bool Match(ReadOnlySpan<byte> utf8Text) => this.jsonReader.ValueTextEquals(utf8Text);

        public bool Match(ReadOnlySpan<char> text) => this.jsonReader.ValueTextEquals(text);

        private sealed class SequenceSegment : ReadOnlySequenceSegment<byte>, IDisposable
        {
            internal IMemoryOwner<byte> Buffer { get; }

            internal SequenceSegment? Previous { get; set; }

            private bool disposed;

            public SequenceSegment(int size, SequenceSegment? previous)
            {
                this.Buffer = MemoryPool<byte>.Shared.Rent(size);
                this.Previous = previous;

                this.Memory = this.Buffer.Memory;
                this.RunningIndex = previous?.RunningIndex + previous?.Memory.Length ?? 0;
            }

            public void SetNext(SequenceSegment next) => this.Next = next;

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.Buffer.Dispose();
                    this.Previous?.Dispose();
                }
            }
        }
    }
}