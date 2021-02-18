// <copyright file="JsonEventHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System.Text.Json;

    /// <summary>
    /// Methods which assist in the creation of a <see cref="IJsonEventHandler{TMemento}"/>.
    /// </summary>
    public static class JsonEventHandler
    {
        /// <summary>
        /// The sequence number property name.
        /// </summary>
        internal const string EventSequenceNumberPropertyNameString = "sequenceNumber";

        /// <summary>
        /// The event type property name.
        /// </summary>
        internal const string EventTypePropertyNameString = "type";

        /// <summary>
        /// The event payload property name.
        /// </summary>
        internal const string EventPayloadPropertyNameString = "payload";

        /// <summary>
        /// Read to the end of the event, having read to the end of the payload.
        /// </summary>
        /// <param name="streamReader">The <see cref="Utf8JsonStreamReader"/> from which to read the payload.</param>
        public static void ReadToEndOfEvent(ref Utf8JsonStreamReader streamReader)
        {
            // Skip to the end of the event
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException($"Expected to see the end of the Event object.");
            }
        }

        /// <summary>
        /// Find the event type value in the stream reader.
        /// </summary>
        /// <param name="streamReader">The stream reader in which to find the event type.</param>
        public static void FindEventType(ref Utf8JsonStreamReader streamReader)
        {
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(EventTypePropertyNameString))
            {
                throw new JsonException($"Expected to find the {EventTypePropertyNameString} property.");
            }

            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected the {EventTypePropertyNameString} property to be a string.");
            }
        }

        /// <summary>
        /// Find the value of the payload property in the event.
        /// </summary>
        /// <param name="streamReader">The <see cref="Utf8JsonStreamReader"/> from which we are reading the event.</param>
        public static void FindPayload(ref Utf8JsonStreamReader streamReader)
        {
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(EventPayloadPropertyNameString))
            {
                throw new JsonException($"Expected to find the {EventPayloadPropertyNameString} property.");
            }

            // position at the start of the payload
            streamReader.Read();
        }

        /// <summary>
        /// Read the event sequence number from the <see cref="Utf8JsonStreamReader"/>.
        /// </summary>
        /// <param name="streamReader">The stream reader from which to read the event sequence number.</param>
        /// <returns>The event sequence number.</returns>
        public static long ReadEventSequenceNumber(ref Utf8JsonStreamReader streamReader)
        {
            long sequenceNumber;
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(EventSequenceNumberPropertyNameString))
            {
                throw new JsonException($"Expected to find the {EventSequenceNumberPropertyNameString} property.");
            }

            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected the {EventSequenceNumberPropertyNameString} property to be a number.");
            }

            sequenceNumber = streamReader.GetInt64();
            return sequenceNumber;
        }
    }
}
