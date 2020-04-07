using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using OkonkwoCore.Netx.Contracts;
using System;
using System.Collections.Generic;

namespace OkonkwoCore.Netx.Data
{
    public abstract class TableEntityProxy<TEntity> : ITableEntity
        where TEntity : class, IIdentifiableEntity, new()
    {
        protected string _partitionKey;
        protected string _rowKey;
        protected string _eTag;
        protected TEntity _entity;

        /// <summary>
        /// 
        /// </summary>
        protected TableEntityProxy()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected TableEntityProxy(TEntity value)
        {
            _entity = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        protected TableEntityProxy(string id)
        {
            PartitionKey = BuildPartitionKey(id);
            RowKey = BuildRowKey(id);
            _entity = new TEntity() { Id = id };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        protected TableEntityProxy(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey ?? throw new ArgumentNullException(nameof(partitionKey));
            RowKey = rowKey ?? throw new ArgumentNullException(nameof(rowKey));
            _entity = new TEntity();
        }

        /// <summary>
        /// 
        /// </summary>
        public string PartitionKey
        {
            get
            {
                _partitionKey = _partitionKey ?? BuildPartitionKey(_entity.Id);
                return _partitionKey;
            }

            set
            {
                _partitionKey = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string RowKey
        {
            get
            {
                _rowKey = _rowKey ?? BuildRowKey(_entity.Id);
                return _rowKey;
            }

            set
            {
                _rowKey = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ETag
        {
            get
            {
                _eTag = _eTag ?? BuildETag();
                return _eTag;
            }

            set { _eTag = value; }
        }

        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TEntity Entity { get => _entity; set => _entity = value; }

        /// <summary>
        /// Constructs and hydrates the entity from the Azure Table entry
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="operationContext"></param>
        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            _entity = new TEntity();

            TableEntity.ReadUserObject(_entity, properties, operationContext);

            ReadValues(properties, operationContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="operationContext"></param>
        protected virtual void ReadValues(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <param name="timeStamp"></param>
        /// <param name="properties"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public virtual TEntity ResolveEntity(string partitionKey, string rowKey, DateTimeOffset timeStamp, IDictionary<string, EntityProperty> properties, string etag)
        {
            // resolution logic that can be passed as a EntityResolver<T>

            // useful when different entities are stored in the same Azure Table
            // if a different strategy is used to determine how entities are distributed within a table
            // this method should be overriden in the child class to reflect the strategy

            if (partitionKey == BuildPartitionKey(null))
            {
                _partitionKey = partitionKey;
                _rowKey = rowKey;

                ReadEntity(properties, null);
            }

            Timestamp = timeStamp;

            return _entity;
        }

        /// <summary>
        /// Writes the entity's properties into an IDictionary
        /// </summary>
        /// <param name="operationContext"></param>
        /// <returns></returns>
        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = TableEntity.WriteUserObject(_entity, operationContext);

            WriteValues(properties, operationContext);

            return properties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="operationContext"></param>
        protected virtual void WriteValues(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableResult"></param>
        /// <returns></returns>
        public virtual TEntity GetEntityFromTableResult(TableResult tableResult)
        {
            if (tableResult.Result == null)
                return null;

            if (tableResult.Result is DynamicTableEntity dynamicEntity)
            {
                _partitionKey = dynamicEntity.PartitionKey;
                _rowKey = dynamicEntity.RowKey;

                ReadEntity(dynamicEntity.Properties, null);
            }

            return _entity;
        }

        /// <summary>
        /// Builds the PartitionKey
        /// </summary>
        /// <param name="id">Identifier to use to build the PartitionKey</param>
        /// <returns></returns>
        public abstract string BuildPartitionKey(string id);

        /// <summary>
        /// Builds the RowKey
        /// </summary>
        /// <param name="id">Identifier to use to build the RowKey</param>
        /// <returns></returns>
        public abstract string BuildRowKey(string id);

        /// <summary>
        /// Builds the eTag
        /// </summary>
        /// <returns></returns>
        public virtual string BuildETag()
        {
            // Return "*" implements last-write-wins concurrency

            return "*";

            // to implement optimistic concurrency, override this method to return null
            // doing so will coerce the CloudTableRepository to set the Etag by reading the table
        }
    }
}
