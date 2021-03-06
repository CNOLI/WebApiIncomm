using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;

namespace wa_api_incomm.ApiRest
{
    public class IncommApi
    {
        private const string ApiURL = "http://192.168.29.20:2555";//qa
        //private const string ApiURL = "http://192.168.31.19:20966";//produccion        
        private HttpClient api = new HttpClient();

        public IncommApi(string merchantId,string auth,string posid, string source)
        {
            api.BaseAddress = new Uri(ApiURL);
            api.DefaultRequestHeaders.Accept.Clear();
            api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //var authenticationHeaderValue = new AuthenticationHeaderValue("0MzrKwCsAlFWowGqWt/Q2Q==", "");
            api.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", auth);

            api.DefaultRequestHeaders.Add("merchantId", merchantId);
            api.DefaultRequestHeaders.Add("posId", posid);
            api.DefaultRequestHeaders.Add("source", source);

        }

        public async Task<Categories> SelCategories()
        {
            Categories result = new Categories();
            HttpResponseMessage response = await api.GetAsync("moviired-api/digitalContent/v1/pines/categories");
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<Categories>();
            }
            return result;
        }

        public async Task<ResultTransaccionIncomm> Transaccion(TransaccionIncommModel model)
        {
            ResultTransaccionIncomm result = new ResultTransaccionIncomm();
            HttpResponseMessage response = await api.PostAsJsonAsync("moviired-api/digitalContent/v1/pines", model);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<ResultTransaccionIncomm>();
            }
            return result;
        }


    }
}
