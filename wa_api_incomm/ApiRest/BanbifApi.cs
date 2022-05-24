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
using wa_api_incomm.Models.BanBif;

namespace wa_api_incomm.ApiRest
{
    public class BanBifApi
    {
        private Token token = null;
        private const string ApiURLToken = Config.vc_url_banbif_token;
        private const string ApiURL = Config.vc_url_banbif;

        private HttpClient api = new HttpClient();
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        public BanBifApi(bool bi_pago = false)
        {

            String client_id = config.GetSection("BanBifInfo:client_id").Value;
            String client_secret = config.GetSection("BanBifInfo:client_secret").Value;

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            api = new HttpClient(clientHandler);
            if (bi_pago)
            {
                api.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(config.GetSection("BanBifInfo:TimeOut").Value));
                //api.Timeout = TimeSpan.FromMilliseconds(1200);
                //api.Timeout = TimeSpan.FromMilliseconds(1000);
            }

            token = GetTokenAsync(client_id, client_secret).Result;

            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
        }


        public async Task<Token> GetTokenAsync(string client_id, string client_secret)
        {
            Token accessToken = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                var values = new Dictionary<string, string>();
                values.Add("grant_type", "client_credentials");
                values.Add("client_id", client_id);
                values.Add("client_secret", client_secret);
                var content = new FormUrlEncodedContent(values);

                response = await api.PostAsync(ApiURLToken, content);

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
        public async Task<Response> get_rubros_recaudador()
        {
            Response Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                String codigoCanal = config.GetSection("BanBifInfo:codigoCanal").Value;
                api.DefaultRequestHeaders.Add("codigoCanal", codigoCanal);
                api.DefaultRequestHeaders.Add("numeroPagina", "1");
                api.DefaultRequestHeaders.Add("cantidadRegistros", "100");

                string parametros = "";
                parametros += "?activo=true";

                response = await api.GetAsync(ApiURL + "api-recaudaciones/v1/recaudadores/BANBIF/rubros" + parametros);

                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<Response>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception("get_rubros_recaudador: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". get_rubros_recaudador " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }

        public async Task<Response> get_empresa_rubros(RubroModel.Rubro_Input model)
        {
            Response Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                String codigoCanal = config.GetSection("BanBifInfo:codigoCanal").Value;
                String codigoRecaudador = config.GetSection("BanBifInfo:codigoRecaudador").Value;

                if (model.vc_cod_rubro is null)
                {
                    throw new Exception("Debe ingresar un codigo de rubro.");
                }

                api.DefaultRequestHeaders.Add("codigoCanal", codigoCanal);
                api.DefaultRequestHeaders.Add("numeroPagina", "1");
                api.DefaultRequestHeaders.Add("cantidadRegistros", "100");

                string parametros = "";
                parametros += "?codigoRecaudador=" + codigoRecaudador;

                response = await api.GetAsync(ApiURL + "api-recaudaciones/v1/rubros/" + model.vc_cod_rubro + "/empresas" + parametros);

                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<Response>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception("get_empresa_rubros: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". get_empresa_rubros " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }

        public async Task<Response> get_lista_convenio(EmpresaModel.Empresa_Input model)
        {
            Response Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                String codigoCanal = config.GetSection("BanBifInfo:codigoCanal").Value;
                String codigoRecaudador = config.GetSection("BanBifInfo:codigoRecaudador").Value;

                if (model.vc_cod_empresa is null)
                {
                    throw new Exception("Debe ingresar un codigo de empresa.");
                }

                api.DefaultRequestHeaders.Add("codigoCanal", codigoCanal);
                api.DefaultRequestHeaders.Add("numeroPagina", "1");
                api.DefaultRequestHeaders.Add("cantidadRegistros", "100");

                string parametros = "";
                parametros += "?codigoEmpresa=" + model.vc_cod_empresa;
                parametros += "&codigoRecaudador=" + codigoRecaudador;
                parametros += "&codigoProducto=" + "1"; // Preguntar a que se refiere

                response = await api.GetAsync(ApiURL + "api-recaudaciones/v1/convenios" + parametros);

                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<Response>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception("get_lista_convenio: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". get_lista_convenio " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }

        public async Task<Response.E_Response> get_convenio(ConvenioModel.Convenio_Input model)
        {
            Response.E_Response Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                String codigoCanal = config.GetSection("BanBifInfo:codigoCanal").Value;
                String codigoRecaudador = config.GetSection("BanBifInfo:codigoRecaudador").Value;

                if (model.vc_cod_convenio is null)
                {
                    throw new Exception("Debe ingresar un codigo de convenio.");
                }

                api.DefaultRequestHeaders.Add("codigoCanal", codigoCanal);

                string parametros = "";
                parametros += "?codigoRecaudador=" + codigoRecaudador;

                response = await api.GetAsync(ApiURL + "api-recaudaciones/v1/convenios/" + model.vc_cod_convenio + parametros);

                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<Response.E_Response>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception("get_convenio: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ". get_convenio " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }

        public async Task<Response.Ls_Response_Trx> Consultar_Deuda(DeudaModel.Deuda_Input model, Serilog.ILogger logger, string id_trx_hub = "")
        {
            Response.Ls_Response_Trx Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                api.DefaultRequestHeaders.Accept.Remove(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                String codigoCanal = config.GetSection("BanBifInfo:codigoCanal").Value;
                String codigoRecaudador = config.GetSection("BanBifInfo:codigoRecaudador").Value;

                if (model.vc_cod_convenio is null)
                {
                    throw new Exception("Debe ingresar un codigo de convenio.");
                }

                api.DefaultRequestHeaders.Add("codigoCanal", codigoCanal);

                string parametros = "";
                parametros += "?codigoRecaudador=" + codigoRecaudador;
                parametros += "&fechaHora=" + DateTime.Now.ToString("yyyyMMddHHmmss");
                parametros += "&idServicio=" + model.numero_servicio;
                parametros += "&idTransaccionOrigen=" + string.Concat(DateTime.Now.ToString("yyyyMMddHHmmss"), new Random().Next(1, 9).ToString("D1"));

                string url = ApiURL + "api-recaudaciones/v1/convenios/" + model.vc_cod_convenio + "/deudas" + parametros;

                if (logger != null)
                {
                    string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(BanBifApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                         " - Modelo enviado (Consultar_Deuda): ''";
                    logger.Information(msg_request);
                }

                response = await api.GetAsync(url).ConfigureAwait(false);

                if (logger != null)
                {
                    string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(BanBifApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                          " - Modelo recibido (Consultar_Deuda): " + response.Content.ReadAsStringAsync().Result;
                    logger.Information(msg_response);
                }

                //var jsonrpta = response.Content.ReadAsStringAsync().Result;

                Result = JsonConvert.DeserializeObject<Response.Ls_Response_Trx>(await response.Content.ReadAsStringAsync());

            }
            catch (OperationCanceledException e)
            {
                if (logger != null)
                {
                    logger.Error(e.Message + ". Consultar_Deuda TIMEOUT" + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                }
                throw new Exception(e.Message + ". Consultar_Deuda TIMEOUT " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.Error(ex.Message + ". Consultar_Deuda " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                }
                throw new Exception(ex.Message + ". Consultar_Deuda " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
        public async Task<Response.E_Response_Trx> Procesar_Pago(PagoModel model, decimal? idTransaccionOrigen, Serilog.ILogger logger, string id_trx_hub = "")
        {
            Response.E_Response_Trx Result = new Response.E_Response_Trx();
            HttpResponseMessage response = new HttpResponseMessage();
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;
            try
            {
                String codigoCanal = config.GetSection("BanBifInfo:codigoCanal").Value;

                api.DefaultRequestHeaders.Add("codigoCanal", codigoCanal);

                string parametros = "";
                parametros += "?idTransaccionOrigen=" + idTransaccionOrigen.ToString();

                var json = JsonConvert.SerializeObject(model);
                var httpContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.Default, "application/json");

                string url = ApiURL + "api-recaudaciones/v1/pagos" + parametros;

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(BanBifApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (Procesar_Pago): " + JsonConvert.SerializeObject(model);
                logger.Information(msg_request);

                dt_inicio = DateTime.Now;
                response = await api.PostAsync(url, httpContent).ConfigureAwait(false);
                dt_fin = DateTime.Now;

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(BanBifApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (Procesar_Pago): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);

                //var jsonrpta = response.Content.ReadAsStringAsync().Result;

                Result = JsonConvert.DeserializeObject<Response.E_Response_Trx>(await response.Content.ReadAsStringAsync());
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = dt_fin;

                //Temporal
                //Result.timeout = true;
            }
            catch (OperationCanceledException e)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                Result.timeout = true;
            }
            catch (Exception ex)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". Procesar_Pago " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". Procesar_Pago " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
        public async Task<Response.E_Response_Trx> Reversar_Pago(ReversarPagoParamModel param_model, ReversarPagoModel model, decimal? idTransaccionOrigen, Serilog.ILogger logger, string id_trx_hub = "")
        {
            Response.E_Response_Trx Result = null;
            HttpResponseMessage response = new HttpResponseMessage();
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;
            try
            {
                String codigoCanal = config.GetSection("BanBifInfo:codigoCanal").Value;
                String codigoRecaudador = config.GetSection("BanBifInfo:codigoRecaudador").Value;

                api.DefaultRequestHeaders.Add("codigoCanal", codigoCanal);

                string parametros = "";
                parametros += "?codigoRecaudador=" + codigoRecaudador;
                parametros += "&codigoConvenio=" + param_model.codigoConvenio.ToString();
                parametros += "&fechaHora=" + DateTime.Now.ToString("yyyyMMddHHmmss");
                parametros += "&idTransaccionOrigen=" + idTransaccionOrigen.ToString();
                parametros += "&codigoMoneda=" + param_model.codigoMoneda.ToString();
                parametros += "&cantidadPagos=" + param_model.cantidadPagos.ToString();
                parametros += "&agrupacion=" + (param_model.agrupacion == true ? 1 : 0);
                parametros += "&montoTotalDeuda=" + param_model.montoTotalDeuda.ToString();
                parametros += "&montoTotalSaldo=" + param_model.montoTotalSaldo.ToString();

                var json = JsonConvert.SerializeObject(model);
                var httpContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.Default, "application/json");


                string url = ApiURL + "api-recaudaciones/v1/extornosPagos" + parametros;

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(BanBifApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (Reversar_Pago): " + JsonConvert.SerializeObject(model);
                logger.Information(msg_request);

                dt_inicio = DateTime.Now;
                response = await api.PutAsync(url, httpContent).ConfigureAwait(false);
                dt_fin = DateTime.Now;

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(BanBifApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (Reversar_Pago): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);


                Result = JsonConvert.DeserializeObject<Response.E_Response_Trx>(await response.Content.ReadAsStringAsync());
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
                    logger.Error(e.Message + ". Reversar_Pago " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                    throw new Exception(e.Message + ". Reversar_Pago " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                }
            }
            catch (Exception ex)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". Reversar_Pago " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". Reversar_Pago " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
    }
}
