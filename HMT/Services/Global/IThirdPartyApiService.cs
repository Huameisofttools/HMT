using RestSharp;
using suiren.Models;
using System.Threading.Tasks;

namespace suiren.Services
{
    public interface IThirdPartyApiService
    {
        Task<RestResponse> GetDskDataAsync(string parameters);
    }
}
