using RestSharp;
using System;

namespace suiren.Utilities
{
    public static class ClientFactory
    {
        public static RestClient CreateDskHttpClient()
        {
            var options = new RestClientOptions("https://api.deepseek.com/chat/completions");

            return new RestClient(options);
        }
    }
}
