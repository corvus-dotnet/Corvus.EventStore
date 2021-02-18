// <copyright file="JsonCommit.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System.Text.Json;

#pragma warning disable SA1600 // Elements should be documented

    /// <summary>
    /// Provides constants for the structure of a commit.
    /// </summary>
    internal static class JsonCommit
    {
        public const string IdPropertyNameString = "id";
        public const string AggregateIdPropertyNameString = "aggregateId";
        public const string PartitionKeyPropertyNameString = "partitionKey";
        public const string CommitSequenceNumberPropertyNameString = "sequenceNumber";
        public const string EventsPropertyNameString = "events";

        public static readonly JsonEncodedText EventSequenceNumberPropertyName = JsonEncodedText.Encode(JsonEventHandler.EventSequenceNumberPropertyNameString);
        public static readonly JsonEncodedText EventTypePropertyName = JsonEncodedText.Encode(JsonEventHandler.EventTypePropertyNameString);
        public static readonly JsonEncodedText EventPayloadPropertyName = JsonEncodedText.Encode(JsonEventHandler.EventPayloadPropertyNameString);
        public static readonly JsonEncodedText IdPropertyName = JsonEncodedText.Encode(IdPropertyNameString);
        public static readonly JsonEncodedText AggregateIdPropertyName = JsonEncodedText.Encode(AggregateIdPropertyNameString);
        public static readonly JsonEncodedText PartitionKeyPropertyName = JsonEncodedText.Encode(PartitionKeyPropertyNameString);
        public static readonly JsonEncodedText CommitSequenceNumberPropertyName = JsonEncodedText.Encode(CommitSequenceNumberPropertyNameString);
        public static readonly JsonEncodedText EventsPropertyName = JsonEncodedText.Encode(EventsPropertyNameString);
    }
}
