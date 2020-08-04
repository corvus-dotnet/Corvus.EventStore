namespace Corvus.EventStore.Example
{
    using System;

    public readonly struct ToDoItemAddedEventPayload
    {
        public const string EventType = "corvus.event-store-example.to-do-item-added";

        public ToDoItemAddedEventPayload(Guid id, string title, string description)
        {
            this.ToDoItemId = id;
            this.Title = title;
            this.Description = description;
        }

        public Guid ToDoItemId { get; }

        public string Title { get; }

        public string Description { get; }
    }
}
