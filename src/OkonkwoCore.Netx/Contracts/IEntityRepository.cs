using System.Collections.Generic;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IEntityRepository<TEntity> : IRepository
        where TEntity : IIdentifiableEntity, new()
    {
        Task<TEntity> AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<IEnumerable<TEntity>> GetAsync();
        Task<TEntity> GetAsync(string id);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task RemoveAsync(TEntity entity);
        Task RemoveAsync(string id);
    }
}
