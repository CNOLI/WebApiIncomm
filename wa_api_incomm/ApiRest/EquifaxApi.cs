using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Equifax;
using static wa_api_incomm.Models.Equifax_InputModel;
using static wa_api_incomm.Models.Equifax_ResponseModel;

namespace wa_api_incomm.ApiRest
{
    public class EquifaxApi
    {
        private Token token = null;
        private const string ApiURL = Config.vc_url_equifax;

        private HttpClient api = new HttpClient();
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        public EquifaxApi()
        {

            String username = config.GetSection("EquifaxInfo:username").Value;
            String password = config.GetSection("EquifaxInfo:password").Value;

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            api = new HttpClient(clientHandler);

            //api.BaseAddress = new Uri(ApiURL);

            token = GetTokenAsync(username, password).Result;

        }


        public async Task<Token> GetTokenAsync(string username, string password)
        {
            Token accessToken = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                api.DefaultRequestHeaders.Add("username", username);
                api.DefaultRequestHeaders.Add("password", password);

                response = await api.PostAsync(ApiURL + "api/v1/auth", null);

                if (response.IsSuccessStatusCode)
                {
                    accessToken = JsonConvert.DeserializeObject<Token>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception("GetTokenAsync: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". GetTokenAsync " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return accessToken;
        }
        public async Task<Generar_Reporte_Response> Generar_Reporte(Generar_Reporte_Input model, Serilog.ILogger logger, string id_trx_hub = "")
        {
            Generar_Reporte_Response Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {

                api.DefaultRequestHeaders.Add("Authorization", "bearer " + token.token);

                var json = JsonConvert.SerializeObject(model);
                var httpContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                string url = ApiURL + "api/v1/sales";

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(EquifaxApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (Generar_Reporte): " + JsonConvert.SerializeObject(model);
                logger.Information(msg_request);

                response = await api.PostAsync(url, httpContent).ConfigureAwait(false);

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(EquifaxApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (Generar_Reporte): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);

                var jsonrpta = response.Content.ReadAsStringAsync().Result;

                Result = JsonConvert.DeserializeObject<Generar_Reporte_Response>(await response.Content.ReadAsStringAsync());

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + ". Generar_Reporte " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));

                if (response.Content == null)
                {
                    throw new Exception("Generar_Reporte: " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                }
                else
                {
                    Result = JsonConvert.DeserializeObject<Generar_Reporte_Response>(await response.Content.ReadAsStringAsync());
                }
            }
            return Result;
        }
    }
}
