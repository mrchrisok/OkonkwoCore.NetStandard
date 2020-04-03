using OkonkwoCore.Netx.Data;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface ITableEntityProxyFactory
    {
        Task<TableEntityProxy<TEntity>> BuildAsync<TEntity>() where TEntity : class, IIdentifiableEntity, new();
        Task<TableEntityProxy<TEntity>> BuildAsync<TEntity>(TEntity entity) where TEntity : class, IIdentifiableEntity, new();
        Task<TableEntityProxy<TEntity>> BuildAsync<TEntity>(string id) where TEntity : class, IIdentifiableEntity, new();
        Task<TableEntityProxy<TEntity>> BuildAsync<TEntity>(string partitionKey, string rowKey) where TEntity : class, IIdentifiableEntity, new();
    }
}