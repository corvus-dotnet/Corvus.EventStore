// <copyright file="TableStorageCheckpointStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Core;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Implements a checkpoint store over table storage.
    /// </summary>
    public readonly struct TableStorageCheckpointStore : ICheckpointStore
    {
        private const string CheckpointProperty = "checkpoint";

        private readonly CloudTable table;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageCheckpointStore"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The cloud table factor for the store.</param>
        internal TableStorageCheckpointStore(ICheckpointStoreCloudTableFactory cloudTableFactory)
        {
            this.table = cloudTableFactory.GetTable();
        }

        /// <inheritdoc/>
        public async Task<ReadOnlyMemory<byte>?> ReadCheckpoint(Guid identity)
        {
            string pkrk = identity.ToString("D");
            TableResult result = await this.table.ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pkrk, pkrk)).ConfigureAwait(false);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode <= 299)
            {
                return ((DynamicTableEntity)result.Result).Properties[CheckpointProperty].BinaryValue;
            }

            if (result.HttpStatusCode == 404)
            {
                return null;
            }

            throw new Exception($"Unable to read checkpoint from table storage: Underlying error status code: {result.HttpStatusCode}");
        }

        /// <inheritdoc/>
        public async Task ResetCheckpoint(Guid identity)
        {
            string pkrk = identity.ToString("D");
            TableResult result = await this.table.ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pkrk, pkrk)).ConfigureAwait(false);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode <= 299)
            {
                TableResult deletionResult = await this.table.ExecuteAsync(TableOperation.Delete((DynamicTableEntity)result.Result)).ConfigureAwait(false);
                if (deletionResult.HttpStatusCode >= 200 && result.HttpStatusCode <= 299)
                {
                    return;
                }

                throw new Exception($"Unable to reset checkpoint from table storage: failed to execute delete operation. Underlying error status code: {result.HttpStatusCode}");
            }

            if (result.HttpStatusCode == 404)
            {
                // NOP, there was no corresponding row for the checkpoint.
                return;
            }

            throw new Exception($"Unable to reset checkpoint from table storage: failed to execute get operationoperation. Underlying error status code: {result.HttpStatusCode}");
        }

        /// <inheritdoc/>
        public async Task SaveCheckpoint(Guid identity, ReadOnlyMemory<byte> checkpoint)
        {
            string pkrk = identity.ToString("D");
            var entity = new DynamicTableEntity(pkrk, pkrk);
            entity.Properties.Add(CheckpointProperty, new EntityProperty(checkpoint.ToArray()));
            TableResult result = await this.table.ExecuteAsync(TableOperation.InsertOrReplace(entity));

            if (result.HttpStatusCode >= 200 && result.HttpStatusCode <= 299)
            {
                return;
            }

            throw new Exception($"Unable to set checkpoint in table storage. Underlying error status code: {result.HttpStatusCode}");
        }
    }
}
