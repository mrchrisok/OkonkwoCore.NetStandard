using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface ICloudTableRepository<TEntity> : IEntityRepository<TEntity>
        where TEntity : class, IIdentifiableEntity, new()
    {
        Task<TEntity> GetAsync(string partitionKey, string rowKey);
        Task<IEnumerable<TEntity>> GetByPropertiesAsync(IDictionary<string, string> properties);
        Task<IEnumerable<TEntity>> GetByFilterAsync(string filter);
        Task<IEnumerable<TEntity>> GetAsync(TableQuery query);
        Task<TEntity> InsertOrMergeAsync(TEntity entity);
        Task<IEnumerable<TEntity>> InsertOrReplaceRangeAsync(IEnumerable<TEntity> entities);
        Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);
        //
        Task<TableResult> AddAndReturnTableResultAsync(TEntity entity);
        Task<IEnumerable<TableResult>> AddRangeAndReturnTableResultsAsync(IEnumerable<TEntity> entities);
        Task<IEnumerable<TableResult>> GetTableResultsAsync();
        Task<TableResult> GetTableResultAsync(string id);
        Task<TableResult> GetTableResultAsync(string partitionKey, string rowKey);
        Task<TableResult> InsertOrMergeAndReturnTableResultAsync(TEntity entity);
        Task<IEnumerable<TableResult>> InsertOrReplaceRangeAndReturnTableResultsAsync(IEnumerable<TEntity> entities);
        Task<TableResult> UpdateAndReturnTableResultAsync(TEntity entity);
        Task<IEnumerable<TableResult>> UpdateRangeAndReturnTableResultsAsync(IEnumerable<TEntity> entities);
        Task<TableResult> RemoveAndReturnTableResultAsync(string id);
    }
}
