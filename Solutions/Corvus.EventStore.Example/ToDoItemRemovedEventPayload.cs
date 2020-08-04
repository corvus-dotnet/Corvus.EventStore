namespace Corvus.EventStore.Example
{
    using System;

    public readonly struct ToDoItemRemovedEventPayload
    {
        public const string EventType = "corvus.event-store-example.to-do-item-removed";

        public ToDoItemRemovedEventPayload(Guid id)
        {
            this.ToDoItemId = id;
        }

        public Guid ToDoItemId { get; }
    }
}
