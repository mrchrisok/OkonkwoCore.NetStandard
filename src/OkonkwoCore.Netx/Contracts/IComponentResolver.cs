using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IComponentResolver
    {
        void RegisterContainer(object container);
        T Resolve<T>();
        T Resolve<T>(string contractName);
        Task<T> ResolveAsync<T>();
        Task<T> ResolveAsync<T>(string contractName);
    }
}
