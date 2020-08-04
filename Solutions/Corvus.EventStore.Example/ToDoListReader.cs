namespace Corvus.EventStore.Example
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;

    public readonly struct ToDoListReader<TEventReader, TSnapshotReader, TSnapshot>
        where TEventReader : IEventReader
        where TSnapshotReader : ISnapshotReader
        where TSnapshot : ISnapshot
    {
        private readonly AggregateReader<TEventReader, TSnapshotReader, TSnapshot, ToDoListAggregate.ToDoListMemento> reader;

        public ToDoListReader(
            AggregateReader<TEventReader, TSnapshotReader, TSnapshot, ToDoListAggregate.ToDoListMemento> reader)
        {
            this.reader = reader;
        }

        public async ValueTask<ToDoList> GetAsync(string toDoListId)
        {
            ToDoListAggregate aggregate = await this.reader.ReadAsync(
                snapshot => new ToDoListAggregate(toDoListId, snapshot.SequenceNumber, snapshot.GetPayload<ToDoListAggregate.ToDoListMemento>(), default),
                () => default,
                toDoListId).ConfigureAwait(false);

            return new ToDoList(aggregate);
        }
    }
}
