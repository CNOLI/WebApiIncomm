using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Izipay;
using wa_api_incomm.Models.ServiPagos;
using System.Security.Cryptography;
using System.Threading;

namespace wa_api_incomm.ApiRest
{
    public class MegaPuntoApi
    {
        private Token token = null;
        private string ApiURL = "";

        private HttpClient api = new HttpClient();
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        string ApiURLRespaldo2;
        string usuario;
        string clave;
        public MegaPuntoApi(Models.Hub.ConvenioModel hub_convenio)
        {
            ApiURL = hub_convenio.vc_url_api_1;
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            api = new HttpClient(clientHandler);
            api.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(hub_convenio.nu_seg_timeout));

            usuario = config.GetSection("MegaPuntoInfo:Usuario").Value;
            clave = config.GetSection("MegaPuntoInfo:Clave").Value;

            api.BaseAddress = new Uri(ApiURL);
            api.DefaultRequestHeaders.Accept.Clear();

            var token = GetTokenAsync(usuario, clave).Result;
            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);


        }
        public async Task<AccessToken> GetTokenAsync(string UserName, string Password)
        {
            AccessToken accessToken = new AccessToken();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                UserToken usrToken = new UserToken();
                usrToken.UserName = UserName;
                usrToken.Password = Password;

                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpContent = new StringContent(JsonConvert.SerializeObject(usrToken), Encoding.UTF8, "application/json");

                response = await api.PostAsync("Auth/token", httpContent).ConfigureAwait(false);

                accessToken = JsonConvert.DeserializeObject<AccessToken>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return accessToken;

        }

        public async Task<Megapunto_ResponseModel> Recargar_Dispositivo(Megapunto_InputModel modelo, decimal? idTransaccion, Serilog.ILogger logger, string id_trx_hub = "")
        {
            Megapunto_ResponseModel Result = new Megapunto_ResponseModel();
            HttpResponseMessage response = new HttpResponseMessage();
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;
            try
            {
                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");

                string url = ApiURL + "Comercio/recargar_dispositivo";

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(MegaPuntoApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (Recargar_Dispositivo): " + JsonConvert.SerializeObject(modelo);
                logger.Information(msg_request);

                dt_inicio = DateTime.Now;
                response = await api.PostAsync(url, httpContent).ConfigureAwait(false);
                dt_fin = DateTime.Now;

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(MegaPuntoApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (Recargar_Dispositivo): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);


                var jsonrpta = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<Megapunto_ResponseModel>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    Result = JsonConvert.DeserializeObject<Megapunto_ResponseModel>(await response.Content.ReadAsStringAsync());
                }
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = dt_fin;
            }
            catch (OperationCanceledException e)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                Result.timeout = true;
                Result.codigo = "-1";
                Result.mensaje = "No hubo respuesta a la solicitud. (Timeout)";
            }
            catch (Exception ex)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". Recarga_Dispositivo_Megapunto " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". Recarga_Dispositivo_Megapunto " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
    }
}
