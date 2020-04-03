using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OkonkwoCore.Netx.Contracts
{
    public interface IHttpService : IDisposable
    {
        Task<HttpResponseMessage> GetAsync(string url, dynamic state);
        Task<HttpResponseMessage> PostAsync(string url, dynamic state);
    }
}
