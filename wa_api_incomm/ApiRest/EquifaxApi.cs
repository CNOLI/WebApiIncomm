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
using wa_api_incomm.Models.BanBif;

namespace wa_api_incomm.ApiRest
{
    public class EquifaxApi
    {
        private Token token = null;
        private const string ApiURL = "https://accepwse.equifax.com.pe/salespartner/"; //QA
        private HttpClient api = new HttpClient();
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        public EquifaxApi()
        {

            String username = config.GetSection("EquifaxInfo:username").Value;
            String password = config.GetSection("EquifaxInfo:password").Value;

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            api = new HttpClient(clientHandler);

            api.BaseAddress = new Uri(ApiURL);

            token = GetTokenAsync(username, password).Result;


            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            
        }


        public async Task<Token> GetTokenAsync(string username, string password)
        {
            Token accessToken = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                api.DefaultRequestHeaders.Add("username", username);
                api.DefaultRequestHeaders.Add("password", password);

                response = await api.PostAsync("api/v1/auth", null);

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
        public async Task<Response.E_Response_Trx> Generar_Reporte(PagoModel model,string idTransaccionOrigen)
        {
            Response.E_Response_Trx Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                String codigoCanal = config.GetSection("BanBifInfo:codigoCanal").Value;
                
                api.DefaultRequestHeaders.Add("codigoCanal", codigoCanal);

                string parametros = "";
                parametros += "?idTransaccionOrigen=" + idTransaccionOrigen;

                var json = JsonConvert.SerializeObject(model);
                var httpContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                response = await api.PostAsync("/api-recaudaciones/v1/pagos" + parametros, httpContent).ConfigureAwait(false);

                var jsonrpta = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<Response.E_Response_Trx>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception("get_convenio: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Result = JsonConvert.DeserializeObject<Response.E_Response_Trx>(await response.Content.ReadAsStringAsync());
            }
            return Result;
        }
    }
}
