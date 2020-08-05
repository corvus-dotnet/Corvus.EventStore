// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        public static void Main()
        {
            // Example 1: Retrieve a new instance of an aggregate from the store. Do things to it and save it.
            // Note: We never create an instance of an aggregate with 'new AggregateType()'. We always request them
            // from the store.

            // Example 2: Retrieve an instance of an aggregate from the store. Do more things to it and save it again.
        }
    }
}
