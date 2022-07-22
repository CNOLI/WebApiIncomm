﻿using Microsoft.Extensions.Configuration;
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
    public class ServiPagosApi
    {
        private Token token = null;
        private string ApiURL = ""; //Config.vc_url_servipagos;

        private HttpClient api = new HttpClient();
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        string ApiURLRespaldo2;
        string usuario;
        string clave;
        public ServiPagosApi(Models.Hub.ConvenioModel hub_convenio)
        {
            ApiURL = hub_convenio.vc_url_api_1;
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            api = new HttpClient(clientHandler);
            api.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(hub_convenio.nu_seg_timeout));

            ApiURLRespaldo2 = config.GetSection("ServiPagosInfo:IPRespaldo").Value;
            usuario = config.GetSection("ServiPagosInfo:Usuario").Value;
            clave = config.GetSection("ServiPagosInfo:Clave").Value;

            api.BaseAddress = new Uri(ApiURL);
            api.DefaultRequestHeaders.Accept.Clear();
            api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


        }
        public string GetSHA1(string str)
        {
            SHA1 sha1 = SHA1Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha1.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        public async Task<ServiPagos_ResponseModel> Recargar(ServiPagos_InputModel modelo, decimal? idTransaccion, Serilog.ILogger logger, string id_trx_hub = "")
        {
            ServiPagos_ResponseModel Result = new ServiPagos_ResponseModel();
            HttpResponseMessage response = new HttpResponseMessage();
            var dt_inicio = DateTime.Now;
            var dt_fin = DateTime.Now;
            try
            {
                string hash = modelo.vc_cod_producto + usuario + clave + idTransaccion + modelo.vc_numero_servicio + Convert.ToInt32(modelo.nu_precio_vta) + modelo.ubigeo;
                hash = GetSHA1(hash);

                string parametros = "";
                parametros += "?producto=" + modelo.vc_cod_producto;
                parametros += "&usuario=" + usuario;
                parametros += "&msgid=" + idTransaccion;
                parametros += "&numero=" + modelo.vc_numero_servicio;
                parametros += "&monto=" + Convert.ToInt32(modelo.nu_precio_vta);
                parametros += "&ubigeo=" + modelo.ubigeo;
                parametros += "&firma=" + hash;

                string url = ApiURL + "venta/" + parametros;

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(ServiPagosApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (Recargar): ''";
                logger.Information(msg_request);

                dt_inicio = DateTime.Now;
                //Thread.Sleep(TimeSpan.FromSeconds(180));
                response = await api.GetAsync(url).ConfigureAwait(false);
                dt_fin = DateTime.Now;

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(ServiPagosApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (Recargar): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);


                var jsonrpta = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<ServiPagos_ResponseModel>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    Result = JsonConvert.DeserializeObject<ServiPagos_ResponseModel>(await response.Content.ReadAsStringAsync());
                }
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = dt_fin;

            }
            catch (OperationCanceledException e)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                Result.timeout = true;
                Result.respuesta.resultado = "-1";
                Result.respuesta.obs = "No hubo respuesta a la solicitud. (Timeout)";
            }
            catch (Exception ex)
            {
                Result.dt_inicio = dt_inicio;
                Result.dt_fin = DateTime.Now;
                logger.Error(ex.Message + ". Recarga_ServiPagos " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". Recarga_ServiPagos " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
        public async Task<ServiPagos_ResponseConsultaModel> Consultar(ServiPagos_InputModel modelo, decimal? idTransaccion, Serilog.ILogger logger, string id_trx_hub = "")
        {
            ServiPagos_ResponseConsultaModel Result = new ServiPagos_ResponseConsultaModel();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                string hash = "1" + usuario + clave + "2" + idTransaccion;
                hash = GetSHA1(hash);

                string parametros = "";
                parametros += "?tipo=1";
                parametros += "&usuario=" + usuario;
                parametros += "&parametro=2";
                parametros += "&valor=" + idTransaccion;
                parametros += "&firma=" + hash;

                string url = ApiURL + "consulta/" + parametros;

                string msg_request = "idtrx: " + id_trx_hub + " / " + typeof(ServiPagosApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                     " - Modelo enviado (Consultar): ''";
                logger.Information(msg_request);

                response = await api.GetAsync(url).ConfigureAwait(false);

                string msg_response = "idtrx: " + id_trx_hub + " / " + typeof(ServiPagosApi).ToString().Split(".")[2] + " - " + "URL: " + url +
                                      " - Modelo recibido (Consultar): " + response.Content.ReadAsStringAsync().Result;
                logger.Information(msg_response);


                var jsonrpta = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    Result = JsonConvert.DeserializeObject<ServiPagos_ResponseConsultaModel>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    Result = JsonConvert.DeserializeObject<ServiPagos_ResponseConsultaModel>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (OperationCanceledException e)
            {
                Result.timeout = true;
                Result.respuesta.resultado = "-1";
                Result.respuesta.obs = "No hubo respuesta a la solicitud. (Timeout)";
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + ". Consulta_ServiPagos " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
                throw new Exception(ex.Message + ". Consulta_ServiPagos " + (response.Content == null ? "" : response.Content.ReadAsStringAsync().Result));
            }
            return Result;
        }
    }
}
