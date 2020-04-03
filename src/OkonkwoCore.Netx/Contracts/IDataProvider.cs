using System.Net.Http;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IDataProvider
    {
        Task<string> GetDataAsync(HttpRequestMessage req);
    }
}
