using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using OkonkwoCore.Netx.Contracts;
using OkonkwoCore.Netx.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Data
{
    public abstract class CloudTableRepository<TEntity> : ICloudTableRepository<TEntity>
        where TEntity : class, IIdentifiableEntity, new()
    {
        protected ICloudStorageFactory _cloudStorageFactory;
        protected CloudTable _cloudTable;
        protected ITableEntityProxyFactory _tableEntityProxyFactory;

        protected CloudTableRepository(string tableName,
            ICloudStorageFactory cloudStorageFactory, ITableEntityProxyFactory tableEntityProxyFactory)
        {
            _cloudStorageFactory = cloudStorageFactory;
            _cloudTable = _cloudStorageFactory.GetCloudTable(tableName);

            // is there still a synchronous version of this?
            _cloudTable.CreateIfNotExistsAsync().Wait();

            _tableEntityProxyFactory = tableEntityProxyFactory;
        }

        public virtual int TableServiceBatchMaximumOperations => 100;

        #region standard entity operations

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
            var tableResult = await AddAndReturnTableResultAsync(entity);
            return tableEntityProxy.GetEntityFromTableResult(tableResult);
        }

        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            var results = await AddRangeAndReturnTableResultsAsync(entities);

            var resultList = new List<TEntity>(results.Count());

            foreach (var result in results)
            {
                var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
                resultList.Add(tableEntityProxy.GetEntityFromTableResult(result));
            }

            return resultList;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync()
        {
            return await GetAsync(new TableQuery());
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(TableQuery query)
        {
            if (query == null)
                throw new ArgumentException("Query is null");
            //

            TableContinuationToken token = null;
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();

            var entities = new List<TEntity>();

            do
            {
                var queryResult = await _cloudTable.ExecuteQuerySegmentedAsync(query, tableEntityProxy.ResolveEntity, token);
                entities.AddRange(queryResult.Results);

                token = queryResult.ContinuationToken;

            } while (token != null);

            return entities;
        }

        public virtual async Task<TEntity> GetAsync(string id)
        {
            id = id ?? throw new ArgumentNullException("id");
            //

            var tableResult = await GetTableResultAsync(id);
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
            return tableEntityProxy.GetEntityFromTableResult(tableResult);
        }

        public virtual async Task<TEntity> GetAsync(string partitionKey, string rowKey)
        {
            if (partitionKey == null)
                throw new ArgumentNullException("PartitionKey");

            if (rowKey == null)
                throw new ArgumentNullException("RowKey");

            var tableResult = await GetTableResultAsync(partitionKey, rowKey);
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
            return tableEntityProxy.GetEntityFromTableResult(tableResult);
        }

        public async Task<IEnumerable<TEntity>> GetByPropertiesAsync(IDictionary<string, string> properties)
        {
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
            var partitionKey = tableEntityProxy.BuildPartitionKey(null);
            //

            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);

            foreach (var property in properties)
            {
                var propertyFilter = TableQuery.GenerateFilterCondition(property.Key, QueryComparisons.Equal, property.Value);
                filter = TableQuery.CombineFilters(filter, TableOperators.And, propertyFilter);
            }

            var entities = await GetByFilterAsync(filter);

            return entities;
        }

        public virtual async Task<IEnumerable<TEntity>> GetByFilterAsync(string filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");
            //

            var tableQuery = new TableQuery().Where(filter);

            return await GetAsync(tableQuery);
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var tableResult = await UpdateAndReturnTableResultAsync(entity);
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
            return tableEntityProxy.GetEntityFromTableResult(tableResult);
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            var tableResults = await UpdateRangeAndReturnTableResultsAsync(entities);

            var resultList = new List<TEntity>(tableResults.Count());

            foreach (var result in tableResults)
            {
                var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
                resultList.Add(tableEntityProxy.GetEntityFromTableResult(result));
            }

            return resultList;
        }

        public virtual async Task RemoveAsync(TEntity entity)
        {
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync(entity);
            tableEntityProxy.ETag = await GetETag(tableEntityProxy);
            //
            var operation = TableOperation.Delete(tableEntityProxy);
            _ = await _cloudTable.ExecuteAsync(operation);
        }

        public virtual async Task RemoveAsync(string id)
        {
            id = id ?? throw new ArgumentNullException("id");
            //

            _ = await RemoveAndReturnTableResultAsync(id);
        }

        public virtual async Task<TEntity> InsertOrMergeAsync(TEntity entity)
        {
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
            var tableResult = await InsertOrMergeAndReturnTableResultAsync(entity);

            return tableEntityProxy.GetEntityFromTableResult(tableResult);
        }

        public virtual async Task<IEnumerable<TEntity>> InsertOrReplaceRangeAsync(IEnumerable<TEntity> entities)
        {
            var tableResults = await InsertOrReplaceRangeAndReturnTableResultsAsync(entities);

            var resultList = new List<TEntity>(tableResults.Count());

            foreach (var result in tableResults)
            {
                var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>();
                resultList.Add(tableEntityProxy.GetEntityFromTableResult(result));
            }

            return resultList;
        }

        #endregion

        #region TableResult operations

        public virtual async Task<TableResult> AddAndReturnTableResultAsync(TEntity entity)
        {
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync(entity);
            var operation = TableOperation.Insert(tableEntityProxy);
            var tableResult = await _cloudTable.ExecuteAsync(operation);
            return tableResult;
        }

        public virtual async Task<IEnumerable<TableResult>> AddRangeAndReturnTableResultsAsync(IEnumerable<TEntity> entities)
        {
            var batchOperation = new TableBatchOperation();
            foreach (var entity in entities)
            {
                var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync(entity);
                batchOperation.Insert(tableEntityProxy);
            }

            var results = await ExecuteBatchAsLimitedBatchesAsync(batchOperation);

            return results;
        }

        public virtual async Task<TableResult> GetTableResultAsync(string id)
        {
            id = id ?? throw new ArgumentNullException("id");
            //

            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>(id);
            var operation = TableOperation.Retrieve(tableEntityProxy.PartitionKey, tableEntityProxy.RowKey);
            var tableResult = await _cloudTable.ExecuteAsync(operation);

            return tableResult;
        }

        public virtual async Task<TableResult> GetTableResultAsync(string partitionKey, string rowKey)
        {
            if (partitionKey == null)
                throw new ArgumentNullException("PartitionKey");

            if (rowKey == null)
                throw new ArgumentNullException("RowKey");

            var operation = TableOperation.Retrieve(partitionKey, rowKey);
            var tableResult = await _cloudTable.ExecuteAsync(operation);

            return tableResult;
        }

        public virtual async Task<TableResult> UpdateAndReturnTableResultAsync(TEntity entity)
        {
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync(entity);
            tableEntityProxy.ETag = await GetETag(tableEntityProxy);
            //
            var operation = TableOperation.Replace(tableEntityProxy);
            var tableResult = await _cloudTable.ExecuteAsync(operation);
            return tableResult;
        }

        public virtual async Task<IEnumerable<TableResult>> UpdateRangeAndReturnTableResultsAsync(IEnumerable<TEntity> entities)
        {
            var batchOperation = new TableBatchOperation();
            foreach (var entity in entities)
            {
                var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync(entity);
                batchOperation.Replace(tableEntityProxy);
            }

            var results = await ExecuteBatchAsLimitedBatchesAsync(batchOperation);

            return results;
        }

        public virtual Task<IEnumerable<TableResult>> GetTableResultsAsync()
        {
            // need logic to return all rows in the table

            throw new NotImplementedException();
        }

        public virtual async Task<TableResult> RemoveAndReturnTableResultAsync(string id)
        {
            id = id ?? throw new ArgumentNullException("id");
            //

            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync<TEntity>(id);
            tableEntityProxy.ETag = await GetETag(tableEntityProxy);
            //
            var operation = TableOperation.Delete(tableEntityProxy);
            var tableResult = await _cloudTable.ExecuteAsync(operation);
            return tableResult;
        }

        public virtual async Task<TableResult> InsertOrMergeAndReturnTableResultAsync(TEntity entity)
        {
            var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync(entity);
            var operation = TableOperation.InsertOrMerge(tableEntityProxy);
            var tableResult = await _cloudTable.ExecuteAsync(operation);

            return tableResult;
        }

        public virtual async Task<IEnumerable<TableResult>> InsertOrReplaceRangeAndReturnTableResultsAsync(IEnumerable<TEntity> entities)
        {
            var batchOperation = new TableBatchOperation();
            foreach (var entity in entities)
            {
                var tableEntityProxy = await _tableEntityProxyFactory.BuildAsync(entity);
                batchOperation.InsertOrReplace(tableEntityProxy);
            }

            var results = await ExecuteBatchAsLimitedBatchesAsync(batchOperation);

            return results;
        }
        #endregion

        protected virtual async Task<string> GetETag(ITableEntity tableEntity)
        {
            if (!string.IsNullOrEmpty(tableEntity.ETag))
                return tableEntity.ETag;
            else
            {
                var tableResult = await GetTableResultAsync(tableEntity.PartitionKey, tableEntity.RowKey);
                return string.IsNullOrEmpty(tableResult.Etag) ? "*" : tableResult.Etag;
            }
        }

        #region helper methods - batch operations

        protected virtual async Task<IEnumerable<TableResult>> ExecuteBatchAsLimitedBatchesAsync(
            TableBatchOperation batch, TableRequestOptions requestOptions = null,
            OperationContext operationContext = null, CancellationToken cancellationToken = default)
        {
            if (IsBatchCountUnderSupportedOperationsLimit(batch))
            {
                return await _cloudTable.ExecuteBatchAsync(batch, requestOptions, operationContext, cancellationToken);
            }

            var result = new List<TableResult>();
            var limitedBatchOperationLists = GetLimitedBatchOperationLists(batch);

            foreach (var limitedBatchOperationList in limitedBatchOperationLists)
            {
                var limitedBatch = CreateLimitedTableBatchOperation(limitedBatchOperationList);
                var limitedBatchResult = await _cloudTable.ExecuteBatchAsync(limitedBatch, requestOptions, operationContext, cancellationToken);
                result.AddRange(limitedBatchResult);
            }

            return result;
        }

        protected virtual bool IsBatchCountUnderSupportedOperationsLimit(TableBatchOperation batch)
        {
            // this is no longer public 

            return batch.Count <= TableServiceBatchMaximumOperations;
        }

        protected virtual IEnumerable<List<TableOperation>> GetLimitedBatchOperationLists(TableBatchOperation batch)
        {
            // this is no longer public 

            return batch.ChunkBy(TableServiceBatchMaximumOperations);
        }

        protected virtual TableBatchOperation
            CreateLimitedTableBatchOperation(IEnumerable<TableOperation> limitedBatchOperationList)
        {
            var limitedBatch = new TableBatchOperation();
            foreach (var limitedBatchOperation in limitedBatchOperationList)
            {
                limitedBatch.Add(limitedBatchOperation);
            }

            return limitedBatch;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            _cloudStorageFactory = null;
            _cloudTable = null;
            _tableEntityProxyFactory = null;
        }

        #endregion
    }
}
