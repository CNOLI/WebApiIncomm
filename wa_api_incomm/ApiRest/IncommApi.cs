using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;

namespace wa_api_incomm.ApiRest
{
    public class IncommApi
    {
        private string ApiURL = "";// Config.vc_url_incomm;

        private HttpClient api = new HttpClient();
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        public IncommApi(Models.Hub.ConvenioModel hub_convenio, string merchantId, string auth, string posid, string source)
        {
            ApiURL = hub_convenio.vc_url_api_1;

            api.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(hub_convenio.nu_seg_timeout));
            api.DefaultRequestHeaders.Accept.Clear();
            api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
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
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;
            try
            {
                string url = ApiURL + "moviired-api/digitalContent/v1/pines";

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(IncommApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (Transaccion): " + JsonConvert.SerializeObject(model);
                logger.Information(msg_request);

                dt_inicio = DateTime.Now;
                response = await api.PostAsJsonAsync(url, model);
                dt_fin = DateTime.Now;

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(IncommApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (Transaccion): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);

                result = await response.Content.ReadAsAsync<ResultTransaccionIncomm>();
                result.dt_inicio = dt_inicio;
                result.dt_fin = dt_fin;

            }
            catch (OperationCanceledException e)
            {
                result.dt_inicio = dt_inicio;
                result.dt_fin = DateTime.Now;
                result.timeout = true;
                result.errorCode = "-1";
                result.errorMessage = "No hubo respuesta a la solicitud. (Timeout)";
            }
            catch (Exception ex)
            {
                result.dt_inicio = dt_inicio;
                result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". Transaccion_Incomm " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". Transaccion_Incomm " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return result;
        }


    }
}
