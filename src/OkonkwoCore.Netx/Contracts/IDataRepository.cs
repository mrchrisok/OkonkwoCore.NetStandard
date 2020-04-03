using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IRepository : IDisposable
    {

    }

    public interface IDataRepository : IRepository
    {
        Task<TEntity> AddAsync<TEntity>(TEntity entity) where TEntity : class, IIdentifiableEntity, new();
        Task<IEnumerable<TEntity>> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IIdentifiableEntity, new();
        Task<IEnumerable<TEntity>> GetAsync<TEntity>() where TEntity : class, IIdentifiableEntity, new();
        Task<TEntity> GetAsync<TEntity>(string id) where TEntity : class, IIdentifiableEntity, new();
        Task<TEntity> UpdateAsync<TEntity>(TEntity entity) where TEntity : class, IIdentifiableEntity, new();
        Task RemoveAsync<TEntity>(TEntity entity) where TEntity : class, IIdentifiableEntity, new();
        Task RemoveAsync<TEntity>(string id) where TEntity : class, IIdentifiableEntity, new();
    }
}
