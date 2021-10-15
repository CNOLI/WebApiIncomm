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
    public class BanBifApi
    {
        private Token token = null;
        private const string ApiURLToken = "https://rh-sso-rhsso.uatapps.banbifapimarket.com.pe/auth/realms/Banbif-API-External/protocol/openid-connect/token";
        private const string ApiURL = "https://api-recaudaciones.uatapps.banbifapimarket.com.pe/"; //QA
        private HttpClient api = new HttpClient();
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        public BanBifApi()
        {

            String client_id = config.GetSection("BanBifInfo:client_id").Value;
            String client_secret = config.GetSection("BanBifInfo:client_secret").Value;

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            api = new HttpClient(clientHandler);

            api.BaseAddress = new Uri(ApiURL);
            //client.Timeout = TimeSpan.FromSeconds(60);

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

                response = await api.GetAsync("api-recaudaciones/v1/recaudadores/BANBIF/rubros" + parametros);

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
                parametros += "?codigoRecaudador="+ codigoRecaudador;

                response = await api.GetAsync("api-recaudaciones/v1/rubros/" + model.vc_cod_rubro + "/empresas" + parametros);

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

                response = await api.GetAsync("api-recaudaciones/v1/convenios" + parametros);

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

                response = await api.GetAsync("api-recaudaciones/v1/convenios/"+ model.vc_cod_convenio + parametros);

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

        public async Task<Response.Ls_Response_Trx> get_consultar_deudas(DeudaModel.Deuda_Input model)
        {
            Response.Ls_Response_Trx Result = null;
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
                parametros += "&fechaHora=" + DateTime.Now.ToString("yyyyMMddHHmmss");
                parametros += "&idServicio=" + model.nu_id_servicio;
                parametros += "&idTransaccionOrigen=" + string.Concat(DateTime.Now.ToString("yyyyMMddHHmmss"), new Random().Next(1, 9).ToString("D1"));

                response = await api.GetAsync("api-recaudaciones/v1/convenios/"+ model.vc_cod_convenio + "/deudas" + parametros);

                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<Response.Ls_Response_Trx>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    throw new Exception("get_convenio: " + response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Result = JsonConvert.DeserializeObject<Response.Ls_Response_Trx>(await response.Content.ReadAsStringAsync());
                //throw new Exception(ex.Message + ". get_convenio " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
        public async Task<Response.E_Response_Trx> post_procesar_pago(PagoModel model,string idTransaccionOrigen)
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
                //throw new Exception(ex.Message + ". get_convenio " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
    }
}
