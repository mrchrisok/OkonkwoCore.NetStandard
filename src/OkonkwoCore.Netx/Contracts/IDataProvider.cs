using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IDataProvider
    {
        Task<string> GetDataAsync(object state);
    }
}
