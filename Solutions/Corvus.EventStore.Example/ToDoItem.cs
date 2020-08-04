namespace Corvus.EventStore.Example
{
    using System;

    public readonly struct ToDoItem
    {
        public ToDoItem(Guid id, string title)
        {
            this.Id = id;
            this.Title = title;
        }

        public Guid Id { get; }

        public string Title { get; }
    }
}
