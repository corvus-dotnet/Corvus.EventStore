// <copyright file="Utf8JsonCommitListSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.CommitStore.Azure.TableStorage.Core.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text.Json;
    using Corvus.EventStore.Azure.TableStorage.Core.Internal;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Serialization helpers for an event list.
    /// </summary>
    internal static class Utf8JsonCommitListSerializer
    {
        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerOptions"/>.
        /// </summary>
        public static JsonSerializerOptions Options { get; set; } = GetDefaultOptions();

        /// <summary>
        /// Deserialize an event list.
        /// </summary>
        /// <param name="commitList">The Utf8 encoded binary JSON representation of the list.</param>
        /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="DynamicTableEntity"/>.</returns>
        public static List<BatchedCommit> DeserializeCommitList(ReadOnlySpan<byte> commitList)
        {
            return JsonSerializer.Deserialize<List<BatchedCommit>>(commitList, Options);
        }

        /// <summary>
        /// Deserialize an event list.
        /// </summary>
        /// <param name="commitList">The Utf8 encoded binary JSON representation of the list.</param>
        /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="DynamicTableEntity"/>.</returns>
        public static List<BatchedCommit> DeserializeCommitList(string commitList)
        {
            return JsonSerializer.Deserialize<List<BatchedCommit>>(commitList, Options);
        }

        /// <summary>
        /// Serializes the event list.
        /// </summary>
        /// <param name="commitList">The commitList to serialize.</param>
        /// <returns>The Utf8 binary encoded Json representation of the list.</returns>
        public static byte[] SerializeCommitList(List<BatchedCommit> commitList)
        {
            return JsonSerializer.SerializeToUtf8Bytes(commitList, Options);
        }

        /// <summary>
        /// Serializes the event list.
        /// </summary>
        /// <param name="commitList">The commitList to serialize.</param>
        /// <returns>The Utf8 binary encoded Json representation of the list.</returns>
        public static string SerializeCommitListToString(List<BatchedCommit> commitList)
        {
            return JsonSerializer.Serialize(commitList, Options);
        }

        private static JsonSerializerOptions GetDefaultOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ReadOnlyMemoryByteConverter());
            return options;
        }
    }
}
