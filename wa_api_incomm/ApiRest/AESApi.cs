using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using wa_api_incomm.Models;

namespace wa_api_incomm.ApiRest
{
    public class AESApi
    {
        private HttpClient api = new HttpClient();

        public AESApi(string url)
        {
            api.BaseAddress = new Uri(url);
            api.DefaultRequestHeaders.Accept.Clear();
            api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public async Task<PinModel> GetPin(DatosPinModel model)
        {
            PinModel _pin = new PinModel();
            HttpResponseMessage response = await api.PostAsJsonAsync("api/Pin/get", model);
            if (response.IsSuccessStatusCode)
            {
                _pin = await response.Content.ReadAsAsync<PinModel>();
            }

            return _pin;
        }
    }
}
