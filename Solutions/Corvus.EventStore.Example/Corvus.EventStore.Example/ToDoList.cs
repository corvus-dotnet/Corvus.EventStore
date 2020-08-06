// <copyright file="ToDoList.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Example.Internal;

    /// <summary>
    /// A to do list backed by an aggregate root.
    /// </summary>
    public readonly struct ToDoList
    {
        private readonly AggregateWithMemento<ToDoListAggregateImplementation, ToDoListMemento> aggregate;

        private ToDoList(AggregateWithMemento<ToDoListAggregateImplementation, ToDoListMemento> aggregate)
        {
            this.aggregate = aggregate;
        }

        /// <summary>
        /// Reads an instance of a to-do list, optionally to the specified commit sequence number.
        /// </summary>
        /// <typeparam name="TReader">The type of the <see cref="IAggregateReader"/>.</typeparam>
        /// <param name="reader">The reader from which to read the aggregate.</param>
        /// <param name="aggregateId">The id of the aggregate to read.</param>
        /// <param name="commitSequenceNumber">The (optional) commit sequence number at which to read the aggregate.</param>
        /// <returns>A <see cref="ValueTask"/> which completes with the to do list.</returns>
        public static async ValueTask<ToDoList> Read<TReader>(TReader reader, Guid aggregateId, long commitSequenceNumber = long.MaxValue)
            where TReader : IAggregateReader
        {
            AggregateWithMemento<ToDoListAggregateImplementation, ToDoListMemento> aggregate = await AggregateWithMemento<ToDoListAggregateImplementation, ToDoListMemento>.Read(reader, aggregateId, commitSequenceNumber).ConfigureAwait(false);
            return new ToDoList(aggregate);
        }

        /// <summary>
        /// Initialize the ToDo list with a start date and an owner.
        /// </summary>
        /// <param name="startDate">The starting date for items in the ToDo list.</param>
        /// <param name="owner">The owner of the ToDo list.</param>
        /// <returns>The updated <see cref="ToDoList"/>.</returns>
        public ToDoList Initialize(DateTimeOffset startDate, string owner)
        {
            // Apply an event to set the start date
            // Then apply an event to set the owner
                return this.SetStartDate(startDate)
                           .SetOwner(owner);
        }

        /// <summary>
        /// Sets the owner of the to do list.
        /// </summary>
        /// <param name="owner">The name of the owner.</param>
        /// <returns>A <see cref="ToDoList"/> with the name updated.</returns>
        public ToDoList SetOwner(string owner)
        {
            // Then apply an event to set the owner
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the start date of the todolist.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <returns>A <see cref="ToDoList"/> with the start date updated.</returns>
        public ToDoList SetStartDate(DateTimeOffset startDate)
        {
            // Apply an event to set the start date
            throw new NotImplementedException();
        }

        /// <summary>
        /// Commit the ToDo list.
        /// </summary>
        /// <typeparam name="TWriter">The type of the writer to which to commit the todo list.</typeparam>
        /// <param name="writer">The writer to which to commit the todo list.</param>
        /// <returns>The committed <see cref="ToDoList"/>.</returns>
        public async ValueTask<ToDoList> CommitAsync<TWriter>(TWriter writer)
            where TWriter : IAggregateWriter
        {
            return new ToDoList(
                await writer.WriteAsync(
                    this.aggregate,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    NeverSnapshotPolicy<AggregateWithMemento<ToDoListAggregateImplementation, ToDoListMemento>>.Instance).ConfigureAwait(false));
        }
    }
}
