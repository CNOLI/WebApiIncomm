using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using wa_api_incomm.Models;

namespace wa_api_incomm.ApiRest
{
    public class SentinelApi
    {
        private const string ApiURL = Config.vc_url_sentinel;
        private HttpClient api = new HttpClient();

        public SentinelApi()
        {
            //api.BaseAddress = new Uri(ApiURL);
        }

        public async Task<EncriptaRest> Encriptacion(Encripta modelo)
        {
            EncriptaRest result = new EncriptaRest();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");

                response = await api.PostAsync(ApiURL + "rest/rws_senenc", httpContent).ConfigureAwait(false);

                result = JsonConvert.DeserializeObject<EncriptaRest>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". " + response.Content.ReadAsStringAsync().Result);
            }
            return result;
        }

        public async Task<ConsultaPersonaRest> ConsultaPersona(ConsultaPersona modelo)
        {
            ConsultaPersonaRest result = new ConsultaPersonaRest();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");

                response = await api.PostAsync(ApiURL + "rest/RWS_Tda_SolDat", httpContent).ConfigureAwait(false);

                result = JsonConvert.DeserializeObject<ConsultaPersonaRest>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". " + response.Content.ReadAsStringAsync().Result);
            }
            return result;
        }
        
        public async Task<ConsultaTitularRest> ConsultaTitularFacturacion(ConsultaTitularFac modelo, Serilog.ILogger logger, string id_trx_hub = "")
        {
            ConsultaTitularRest result = new ConsultaTitularRest();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");

                string url = ApiURL + "rest/RWS_Tda_SolConTitMasFacV2";

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(SentinelApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                    " - Modelo enviado (ConsultaTitularFacturacion): " + JsonConvert.SerializeObject(modelo);
                logger.Information(msg_request);

                response = await api.PostAsync(url, httpContent).ConfigureAwait(false);

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(SentinelApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (ConsultaTitularFacturacion): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);

                result = JsonConvert.DeserializeObject<ConsultaTitularRest>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". " + response.Content.ReadAsStringAsync().Result);
            }
            return result;
        }
    }
}
