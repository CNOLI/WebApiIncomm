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
using static wa_api_incomm.Models.Izipay_InputModel;
using static wa_api_incomm.Models.Izipay_ResponseModel;

namespace wa_api_incomm.ApiRest
{
    public class IzipayApi
    {
        private Token token = null;
        private string ApiURL = "";// Config.vc_url_izipay;

        private HttpClient api = new HttpClient();
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        String bin_acq;
        String id_distrib;
        String secreto;

        public IzipayApi(Models.Hub.ConvenioModel hub_convenio)
        {
            ApiURL = hub_convenio.vc_url_api_2;

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            api = new HttpClient(clientHandler);

            bin_acq = config.GetSection("IzipayInfo:bin_acq").Value;
            id_distrib = config.GetSection("IzipayInfo:id_distrib").Value;
            secreto = config.GetSection("IzipayInfo:secreto").Value;

            api.BaseAddress = new Uri(ApiURL);
            api.DefaultRequestHeaders.Accept.Clear();
            api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Token token = GetTokenAsync(bin_acq, secreto).Result;
            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

        }


        public async Task<Token> GetTokenAsync(string username, string password)
        {
            Token accessToken = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                object json_req = new
                {
                    secuencia = new Random().Next(100000, 999999),
                    fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                    bin_acq = bin_acq,
                    secreto = secreto
                };
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                var httpContent = new StringContent(JsonConvert.SerializeObject(json_req), Encoding.UTF8, "application/json");
                response = await api.PostAsync("psr-adm-ws/a-auth", httpContent).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    accessToken = JsonConvert.DeserializeObject<Token>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception("Token IzipayApi: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Token IzipayApi " + ex.Message + ex.InnerException);
            }
            return accessToken;
        }

        public async Task<ResultApi> Obtener_Comercio(object modelo)
        {
            ResultApi result = new ResultApi(); ;

            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/leerEstablecimiento", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultApi>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public async Task<ResultApi> Actualizar_Comercio(object modelo)
        {
            ResultApi result = new ResultApi();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/actualizarEstablecimiento", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultApi>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public async Task<ResultApi> Crear_Comercio(object modelo)
        {
            ResultApi result = new ResultApi();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/nuevoEstablecimiento", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultApi>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public async Task<ResultApi> Crear_Terminal(object modelo)
        {
            ResultApi result = new ResultApi();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/nuevoTerminal", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultApi>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public async Task<ResultApi> Eliminar_Terminal(object modelo)
        {
            ResultApi result = new ResultApi();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/eliminarTerminal", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultApi>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public async Task<ResultApi> Actualizar_Contraseña_Terminal(object modelo)
        {
            ResultApi result = new ResultApi();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/actualizarPasswordTerminal", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultApi>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public async Task<ResultRegla> Crear_Regla(object modelo)
        {
            ResultRegla result = new ResultRegla();
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/nuevaRegla", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultRegla>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public async Task<ResultApi> Actualizar_Regla(object modelo)
        {
            ResultApi result = new ResultApi();
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/actualizarRegla", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultApi>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }

        public async Task<ResultApi> Eliminar_Regla(object modelo)
        {
            ResultApi result = new ResultApi();
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-adm-ws/eliminarRegla", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultApi>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }
    }
}
