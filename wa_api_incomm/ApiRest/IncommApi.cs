using Newtonsoft.Json;
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
        private const string ApiURL = Config.vc_url_incomm;

        private HttpClient api = new HttpClient();

        public IncommApi(string merchantId, string auth, string posid, string source)
        {
            //api.BaseAddress = new Uri(ApiURL);
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
            HttpResponseMessage response = await api.GetAsync(ApiURL + "moviired-api/digitalContent/v1/pines/categories");
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<Categories>();
            }
            return result;
        }

        public async Task<ResultTransaccionIncomm> Transaccion(TransaccionIncommModel model, Serilog.ILogger logger, string id_trx_hub = "")
        {
            ResultTransaccionIncomm result = new ResultTransaccionIncomm();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                string url = ApiURL + "moviired-api/digitalContent/v1/pines";

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(IncommApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (Transaccion): " + JsonConvert.SerializeObject(model);
                logger.Information(msg_request);

                response = await api.PostAsJsonAsync(url, model);

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(IncommApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (Transaccion): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);

                result = await response.Content.ReadAsAsync<ResultTransaccionIncomm>();

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + ". Transaccion_Incomm " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". Transaccion_Incomm " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return result;
        }


    }
}
