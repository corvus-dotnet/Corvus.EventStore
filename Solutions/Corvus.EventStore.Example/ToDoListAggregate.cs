namespace Corvus.EventStore.Example
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;

    public readonly struct ToDoListAggregate : IAggregateRoot<ToDoListAggregate>
    {
        private readonly ImmutableList<IEvent> uncommittedEvents;

        private readonly ToDoListMemento taskListMemento;

        public ToDoListAggregate(
            string aggregateId,
            long sequenceNumber,
            ToDoListMemento memento,
            ImmutableList<IEvent> uncommittedEvents)
        {
#if DEBUG
            Debug.Assert(uncommittedEvents.Count == 0 || uncommittedEvents[^1].SequenceNumber == sequenceNumber);
#endif
            this.AggregateId = aggregateId;
            this.SequenceNumber = sequenceNumber;
            this.uncommittedEvents = uncommittedEvents;
            this.taskListMemento = memento;
        }

        public string AggregateId { get; }

        public long SequenceNumber { get; }

        public ToDoListAggregate AddToDoItem(Guid id, string title, string description)
        {
            var payload = new ToDoItemAddedEventPayload(
                id,
                title,
                description);

            var newEvent = new Event<ToDoItemAddedEventPayload>(
                this.AggregateId,
                ToDoItemAddedEventPayload.EventType,
                this.SequenceNumber + 1,
                0,
                this.AggregateId,
                payload);

            return this.ApplyEvent(newEvent);
        }

        public ToDoListAggregate RemoveToDoItem(Guid id)
        {
            if (!this.taskListMemento.Tasks.Any(x => x.Id == id))
            {
                throw new ArgumentException($"Item with id '{id}' not found.", nameof(id));
            }

            var payload = new ToDoItemRemovedEventPayload(id);

            var newEvent = new Event<ToDoItemRemovedEventPayload>(
                this.AggregateId,
                ToDoItemRemovedEventPayload.EventType,
                this.SequenceNumber + 1,
                0,
                this.AggregateId,
                payload);

            return this.ApplyEvent(newEvent);
        }

        public ToDoListAggregate ApplyEvents(IEventEnumerator events)
        {
            ToDoListAggregate currentAggregate = this;

            while (events.MoveNext())
            {
                switch (events.CurrentEventType)
                {
                    case ToDoItemAddedEventPayload.EventType:
                        currentAggregate = this.WithToDoItemAddedEvent(
                            new Event<ToDoItemAddedEventPayload>(
                                events.CurrentAggregateId,
                                events.CurrentEventType,
                                events.CurrentSequenceNumber,
                                events.CurrentTimestamp,
                                events.CurrentPartitionKey,
                                events.GetCurrentPayload<ToDoItemAddedEventPayload>()));
                        break;

                    case ToDoItemRemovedEventPayload.EventType:
                        currentAggregate = this.WithToDoItemAddedEvent(
                            new Event<ToDoItemRemovedEventPayload>(
                                events.CurrentAggregateId,
                                events.CurrentEventType,
                                events.CurrentSequenceNumber,
                                events.CurrentTimestamp,
                                events.CurrentPartitionKey,
                                events.GetCurrentPayload<ToDoItemRemovedEventPayload>()));
                        break;

                    default:
                        throw new ArgumentException($"Unrecognised event type '{events.CurrentEventType}'.");
                }
            }

            return currentAggregate;
        }

        public ToDoListAggregate ApplyEvent<TEvent>(in TEvent @event) where TEvent : IEvent
        {
            if (@event.AggregateId != this.AggregateId)
            {
                throw new ArgumentException(nameof(@event));
            }

            if (@event.SequenceNumber != (this.SequenceNumber + 1))
            {
                throw new InvalidOperationException($"Unable to apply event out of sequence. Expected sequence number {this.SequenceNumber + 1}; actual sequence number {@event.SequenceNumber}");
            }

            switch (@event.EventType)
            {
                case ToDoItemAddedEventPayload.EventType:
                    return this.WithToDoItemAddedEvent(@event);

                case ToDoItemRemovedEventPayload.EventType:
                    return this.WithToDoItemRemovedEvent(@event);

                default:
                    throw new ArgumentException($"Unrecognised event type '{@event.EventType}'.");
            }
        }

        public ValueTask<ToDoListAggregate> StoreAsync<TEventWriter>(in TEventWriter writer) where TEventWriter : IEventWriter
        {
            throw new NotImplementedException();
        }

        public ValueTask<ToDoListAggregate> StoreSnapshotAsync<TSnapshotWriter>(in TSnapshotWriter writer) where TSnapshotWriter : ISnapshotWriter
        {
            throw new NotImplementedException();
        }

        public readonly struct ToDoListMemento
        {
            public ToDoListMemento(ImmutableList<ToDoItem> tasks)
            {
                this.Tasks = tasks;
            }

            public ImmutableList<ToDoItem> Tasks { get; }
        }

        private ToDoListAggregate WithToDoItemAddedEvent<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            ToDoItemAddedEventPayload payload = @event.GetPayload<ToDoItemAddedEventPayload>();

            return new ToDoListAggregate(
                this.AggregateId,
                @event.SequenceNumber,
                new ToDoListMemento(this.taskListMemento.Tasks.Add(new ToDoItem(payload.ToDoItemId, payload.Title))),
                this.uncommittedEvents.Add(@event));
        }

        private ToDoListAggregate WithToDoItemRemovedEvent<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            ToDoItemRemovedEventPayload payload = @event.GetPayload<ToDoItemRemovedEventPayload>();

            // We don't have to cater for the item not being present in the list, because this validation was done
            // when the event was created.
            ToDoItem itemToRemove = this.taskListMemento.Tasks.Find(item => item.Id == payload.ToDoItemId);

            return new ToDoListAggregate(
                this.AggregateId,
                @event.SequenceNumber,
                new ToDoListMemento(this.taskListMemento.Tasks.Remove(itemToRemove)),
                this.uncommittedEvents.Add(@event));
        }
    }
}
