using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.IzipayFinazas;
using static wa_api_incomm.Models.Izipay_InputModel;
using static wa_api_incomm.Models.Izipay_ResponseModel;

namespace wa_api_incomm.ApiRest
{
    public class IziPayFinanzasApi
    {
        private Token token = null;
        private const string ApiURL = "https://psrdes.izipay.pe:8088/"; //QA
        //private const string ApiURL = "https://psr.izipay.pe:8090/"; //PROD
        private HttpClient api;
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        String izikey;
        public IziPayFinanzasApi(IHttpClientFactory client_factory, Token model)
        {
            api = client_factory.CreateClient("HttpClientWithSSLUntrusted");

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            api = new HttpClient(clientHandler);

            izikey = config.GetSection("IzipayFinanzasInfo:izikey").Value;
            model.bin_acq = config.GetSection("IzipayFinanzasInfo:bin_acq").Value;
           

            api.BaseAddress = new Uri(ApiURL);
            api.DefaultRequestHeaders.Accept.Clear();
            api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            TokenResult token = GetTokenAsync(model).Result;
            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

        }

        public void GetInfoBase(IzipayModel.InfoBase info)
        {
            info.secuencia = new Random().Next(100000, 999999).ToString();
            info.fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss");
        }
        public string GetSHA256(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }


        public async Task<TokenResult> GetTokenAsync(Token model)
        {
            TokenResult accessToken = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                GetInfoBase(model);

                model.mac = izikey + model.bin_acq + model.id_estab + model.id_term + model.secreto;
                model.mac = GetSHA256(model.mac).ToUpper();

                //ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };                
                var httpContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                response = await api.PostAsync("psr-fin-fe/t-auth", httpContent).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    accessToken = JsonConvert.DeserializeObject<TokenResult>(await response.Content.ReadAsStringAsync());
                    return accessToken;
                }

                throw new Exception("Token IzipayApi: " + (response.Content != null ? response.Content.ReadAsStringAsync().Result : ""));
            }
            catch (Exception ex)
            {
                throw new Exception("Token IzipayApi " + ex.Message + ex.InnerException);
            }
        }

        public async Task<ResultPagoDirecto> PagoDirecto(object modelo)
        {
            ResultPagoDirecto result = new ResultPagoDirecto();
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await api.PostAsync("psr-fin-fe/pagoDirecto", httpContent).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResultPagoDirecto>(await response.Content.ReadAsStringAsync());
            }
            return result;
        }
    }
}
