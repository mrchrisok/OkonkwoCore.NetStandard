using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IEntityHttpService
    {
        Task<TEntity> PostAsync<TEntity>(string url, dynamic state) where TEntity : class, new();
    }
}
