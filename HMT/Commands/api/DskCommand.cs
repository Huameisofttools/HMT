using suiren.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suiren.Commands.Api
{
    [Export(typeof(DskCommand))]
    public class DskCommand
    {
        private readonly IThirdPartyApiService _apiService;

        [ImportingConstructor]
        public DskCommand(IThirdPartyApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task ExecuteAsync()
        {
            var data = await _apiService.GetDskDataAsync("parameter=value");
        }
    }
}
