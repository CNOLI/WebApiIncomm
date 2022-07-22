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
using System.Threading;
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
        private string ApiURL = ""; // Config.vc_url_izipay_finanzas;

        private HttpClient api;
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        String izikey;
        public IziPayFinanzasApi(Models.Hub.ConvenioModel hub_convenio, IHttpClientFactory client_factory, Token model)
        {
            ApiURL = hub_convenio.vc_url_api_1;

            api = client_factory.CreateClient("HttpClientWithSSLUntrusted");

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            api = new HttpClient(clientHandler);
            api.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(hub_convenio.nu_seg_timeout));

            izikey = config.GetSection("IzipayFinanzasInfo:izikey").Value;
            model.bin_acq = config.GetSection("IzipayFinanzasInfo:bin_acq").Value;
            
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
          
                var httpContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                response = await api.PostAsync(ApiURL + "psr-fin-fe/t-auth", httpContent).ConfigureAwait(false);

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

        public async Task<ResultPago> PagoDirecto(object modelo, Serilog.ILogger logger, string id_trx_hub = "")
        {
            ResultPago result = new ResultPago();
            HttpResponseMessage response = new HttpResponseMessage();
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;

            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");

                string url = ApiURL + "psr-fin-fe/pagoDirecto";

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(IziPayFinanzasApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (PagoDirecto): " + JsonConvert.SerializeObject(modelo);
                logger.Information(msg_request);

                dt_inicio = DateTime.Now;
                response = await api.PostAsync(url, httpContent).ConfigureAwait(false);
                dt_fin = DateTime.Now;


                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(IziPayFinanzasApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (PagoDirecto): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);

                result = JsonConvert.DeserializeObject<ResultPago>(await response.Content.ReadAsStringAsync());
                result.dt_inicio = dt_inicio;
                result.dt_fin = dt_fin;

            }
            catch (OperationCanceledException e)
            {
                result.dt_inicio = dt_inicio;
                result.dt_fin = DateTime.Now;
                result.timeout = true;
                result.rc = "-1";
                result.descripcion = "No hubo respuesta a la solicitud. (Timeout)";
            }
            catch (Exception ex)
            {
                result.dt_inicio = dt_inicio;
                result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". PagoDirecto " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". PagoDirecto " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return result;
        }
        public async Task<ResultConsultarRecibos> ConsultarRecibos(object modelo, Serilog.ILogger logger, string id_trx_hub = "")
        {
            ResultConsultarRecibos result = new ResultConsultarRecibos();
            HttpResponseMessage response = new HttpResponseMessage();
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;

            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");

                string url = ApiURL + "psr-fin-fe/consultarRecibos";

                if (logger != null)
                {
                    string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(IziPayFinanzasApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (ConsultarRecibos): " + JsonConvert.SerializeObject(modelo);
                    logger.Information(msg_request);
                }

                dt_inicio = DateTime.Now;
                response = await api.PostAsync(url, httpContent).ConfigureAwait(false);
                dt_fin = DateTime.Now;


                if (logger != null)
                {
                    string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(IziPayFinanzasApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (ConsultarRecibos): " + response.Content.ReadAsStringAsync().Result;
                    logger.Information(msg_response);
                }

                result = JsonConvert.DeserializeObject<ResultConsultarRecibos>(await response.Content.ReadAsStringAsync());
                result.dt_inicio = dt_inicio;
                result.dt_fin = dt_fin;

            }
            catch (OperationCanceledException e)
            {
                result.dt_inicio = dt_inicio;
                result.dt_fin = DateTime.Now;
                result.timeout = true;
                result.rc = "-1";
                result.descripcion = "No hubo respuesta a la solicitud. (Timeout)";
            }
            catch (Exception ex)
            {
                result.dt_inicio = dt_inicio;
                result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". ConsultarRecibos " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". ConsultarRecibos " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return result;
        }
        public async Task<ResultPago> PagoRecibo(object modelo, Serilog.ILogger logger, string id_trx_hub = "")
        {
            ResultPago result = new ResultPago();
            HttpResponseMessage response = new HttpResponseMessage();
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;

            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.UTF8, "application/json");

                string url = ApiURL + "psr-fin-fe/pagoRecibo";

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(IziPayFinanzasApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (PagoRecibo): " + JsonConvert.SerializeObject(modelo);
                logger.Information(msg_request);

                dt_inicio = DateTime.Now;
                response = await api.PostAsync(url, httpContent).ConfigureAwait(false);
                dt_fin = DateTime.Now;


                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(IziPayFinanzasApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (PagoRecibo): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);

                result = JsonConvert.DeserializeObject<ResultPago>(await response.Content.ReadAsStringAsync());
                result.dt_inicio = dt_inicio;
                result.dt_fin = dt_fin;

            }
            catch (OperationCanceledException e)
            {
                result.dt_inicio = dt_inicio;
                result.dt_fin = DateTime.Now;
                result.timeout = true;
                result.rc = "-1";
                result.descripcion = "No hubo respuesta a la solicitud. (Timeout)";
            }
            catch (Exception ex)
            {
                result.dt_inicio = dt_inicio;
                result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". PagoRecibo " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". PagoRecibo " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return result;
        }

        public async Task<ResultPago> AnulaPagoRecibo(object modelo, Serilog.ILogger logger, string id_trx_hub = "")
        {
            ResultPago Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;
            try
            {

                var httpContent = new StringContent(JsonConvert.SerializeObject(modelo), Encoding.Default, "application/json");

                string url = ApiURL + "psr-fin-fe/anularPagoRecibo";

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(IziPayFinanzasApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (AnulaPagoRecibo): " + JsonConvert.SerializeObject(modelo);
                logger.Information(msg_request);

                dt_inicio = DateTime.Now;
                response = await api.PostAsync(url, httpContent).ConfigureAwait(false);
                dt_fin = DateTime.Now;

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(IziPayFinanzasApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (AnulaPagoRecibo): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);


                Result = JsonConvert.DeserializeObject<ResultPago>(await response.Content.ReadAsStringAsync());
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = dt_fin;

            }
            catch (WebException e)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    Result.timeout = true;
                }
                else
                {
                    logger.Error(e.Message + ". AnulaPagoRecibo " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                    throw new Exception(e.Message + ". AnulaPagoRecibo " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                }
            }
            catch (Exception ex)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". AnulaPagoRecibo " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". AnulaPagoRecibo " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
    }
}
