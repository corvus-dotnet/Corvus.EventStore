namespace Corvus.EventStore.Example
{
    public class Program
    {
        static void Main(string[] args)
        {
            // New up all the things (concrete readers, writers, etc). 
            var todoListReader = new ToDoListReader();
            

            // Example 1: Retrieve a new instance of an aggregate from the store. Do things to it and save it.
            // Note: We never create an instance of an aggregate with 'new AggregateType()'. We always request them
            // from the store.

            // Example 2: Retrieve an instance of an aggregate from the store. Do more things to it and save it again.


        }
    }
}
