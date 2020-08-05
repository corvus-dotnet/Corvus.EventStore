namespace Corvus.EventStore.Example
{
    using System;

    public readonly struct ToDoList
    {
        public ToDoList(in ToDoListAggregate toDoListAggregate)
        {
            this.ToDoListAggregate = toDoListAggregate;
        }

        internal ToDoListAggregate ToDoListAggregate { get; }

        public ToDoList AddToDoItem(Guid id, string title, string description)
        {
            return new ToDoList(this.ToDoListAggregate.AddToDoItem(id, title, description));
        }

        public ToDoList RemoveToDoItem(Guid id)
        {
            return new ToDoList(this.ToDoListAggregate.RemoveToDoItem(id));
        }
    }
}
