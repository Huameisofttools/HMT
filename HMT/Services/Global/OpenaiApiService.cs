using Newtonsoft.Json;
using RestSharp;
using suiren.Models;
using System.Threading.Tasks;

namespace suiren.Services
{
    public class OpenaiApiService : IThirdPartyApiService
    {
        private readonly RestClient _restClient;

        public OpenaiApiService(RestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<DskApiResponse> GetDskDataAsync(string parameters)
        {
            RestRequest request = new RestRequest("https://api.deepseek.com/chat/completions", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", "Bearer sk-613975d20507407399cb67b54a313504");
            request.AddStringBody(parameters, RestSharp.DataFormat.Json);
            RestResponse response = await _restClient.ExecuteAsync(request).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DskApiResponse>(response.Content);            
        }
    }
}
