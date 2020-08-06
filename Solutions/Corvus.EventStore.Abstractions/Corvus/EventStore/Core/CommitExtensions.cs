// <copyright file="CommitExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions for the <see cref="Commit"/>.
    /// </summary>
    public static class CommitExtensions
    {
        /// <summary>
        /// Validate that an ordered list of commits can be applied to a given aggregate.
        /// </summary>
        /// <param name="commits">The commits to validate.</param>
        /// <param name="aggregateId">The ID of the aggregate.</param>
        /// <param name="currentCommitSequenceNumber">The current commit sequence number of the aggregate.</param>
        /// <param name="currentEventSequenceNumber">The current event sequence number of the aggregate.</param>
        /// <exception cref="InvalidOperationException">The enumerable was not valid. The reason is in the exception message.</exception>
        /// <remarks>This will pick up out-of-sequence commits and events. This also has the side effect of preventing you from applying a commit if you have uncommited events in the instance.</remarks>
        public static void ValidateCommits(this IEnumerable<Commit> commits, Guid aggregateId, long currentCommitSequenceNumber, long currentEventSequenceNumber)
        {
            long previousCommitSequenceNumber = currentCommitSequenceNumber;
            long previousEventSequenceNumber = currentEventSequenceNumber;

            foreach (Commit commit in commits)
            {
                if (commit.AggregateId != aggregateId)
                {
                    // TODO: consider a custom exception
                    throw new InvalidOperationException($"Incorrect aggregate Id for commit with sequence number {commit.SequenceNumber}. Expected {aggregateId}, actual {commit.AggregateId}");
                }

                if (commit.SequenceNumber != previousCommitSequenceNumber + 1)
                {
                    // TODO: consider a custom exception
                    throw new InvalidOperationException($"Incorrect commit sequence number. Expected {previousCommitSequenceNumber + 1}, actual {commit.SequenceNumber}");
                }

                foreach (SerializedEvent @event in commit.Events)
                {
                    if (@event.SequenceNumber != previousEventSequenceNumber + 1)
                    {
                        // TODO: consider a custom exception
                        throw new InvalidOperationException($"Incorrect event sequence number. Expected {previousEventSequenceNumber + 1}, actual {@event.SequenceNumber}");
                    }

                    ++previousEventSequenceNumber;
                }

                ++previousCommitSequenceNumber;
            }
        }
    }
}
