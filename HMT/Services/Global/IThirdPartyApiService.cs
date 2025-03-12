using suiren.Models;
using System.Threading.Tasks;

namespace suiren.Services
{
    public interface IThirdPartyApiService
    {
        Task<DskApiResponse> GetDskDataAsync(string parameters);
    }
}
