using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Izipay_InputModel;
using System.Net.Http;
using static wa_api_incomm.Models.RecargaModel;
using static wa_api_incomm.Models.ServiPagos.ServiPagos_InputModel;
using static wa_api_incomm.Models.Megapunto_InputModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using static wa_api_incomm.Models.ReporteCrediticioModel;
using static wa_api_incomm.Models.EquifaxModel;
using System.ComponentModel.DataAnnotations;

namespace wa_api_incomm.Services
{
    public class ReporteCrediticioService : IReporteCrediticioService
    {

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();


        private readonly Serilog.ILogger _logger;

        public ReporteCrediticioService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object validar_persona(string conexion, SentinelInfo info, Sentinel_InputModel.Consultado model)
        {
            try
            {
                GlobalService global_service = new GlobalService();
                Encripta encripta = null;
                EncriptaRest encripta_rest = null;
                ConsultaPersona modelo = new ConsultaPersona();
                ConsultaPersonaRest rest = new ConsultaPersonaRest();
                Sentinel_ResponseModel response = new Sentinel_ResponseModel();
                string mensaje_error = "";


                SqlConnection con_sql = null;
                con_sql = new SqlConnection(conexion);

                ConvenioModel hub_convenio;
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, 2);
                con_sql.Close();

                ApiRest.SentinelApi api = new ApiRest.SentinelApi(hub_convenio);

                con_sql.Open();
                TipoDocIdentidadModel tipodocidentidad_consultado = new TipoDocIdentidadModel();
                try
                {
                    tipodocidentidad_consultado = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultado), 2);
                }
                catch (Exception ex)
                {
                    tipodocidentidad_consultado = global_service.get_tipo_documento_codigo(con_sql, model.tipo_documento_consultado, 2);
                }

                if (tipodocidentidad_consultado.nu_id_tipo_doc_identidad <= 0)
                {
                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento de consultado no existe.");
                }
                con_sql.Close();

                //encriptar el usuario
                encripta = new Encripta();
                encripta.keysentinel = info.keysentinel;
                encripta.parametro = info.Usuario;
                encripta_rest = api.Encriptacion(encripta).Result;
                if (encripta_rest.coderror != 0)
                {
                    switch (encripta_rest.coderror)
                    {
                        case 1:
                            mensaje_error = "Key no encontrado o asociado a un webservice o no activo.";
                            break;
                        case 2:
                            mensaje_error = "Error en web service.";
                            break;
                    }

                    response.codigo = encripta_rest.coderror.ToString();
                    response.mensaje = mensaje_error;

                    return response;
                }
                modelo.Gx_UsuEnc = encripta_rest.encriptado;

                //encriptar la contraseña
                encripta = new Encripta();
                encripta.keysentinel = info.keysentinel;
                encripta.parametro = info.Contrasena;
                encripta_rest = api.Encriptacion(encripta).Result;
                if (encripta_rest.coderror != 0)
                {
                    switch (encripta_rest.coderror)
                    {
                        case 1:
                            mensaje_error = "Key no encontrado o asociado a un webservice o no activo.";
                            break;
                        case 2:
                            mensaje_error = "Error en web service.";
                            break;
                    }
                    response.codigo = encripta_rest.coderror.ToString();
                    response.mensaje = mensaje_error;

                    return response;
                }
                modelo.Gx_PasEnc = encripta_rest.encriptado;

                modelo.Gx_Key = info.Gx_Key;
                modelo.TipoDocSol = tipodocidentidad_consultado.vc_cod_tipo_doc_identidad;
                modelo.NroDocSol = model.numero_documento_consultado;

                rest = api.ConsultaPersona(modelo).Result;

                rest.MensajeWS = "Consulta exitosa.";

                if (rest.CodigoWS != "0")
                {
                    response.codigo = "40";
                    response.mensaje = global_service.get_mensaje_error(2, rest.CodigoWS);

                    return response;
                }

                response.codigo = "00";
                response.nombres = rest.Nombres;
                response.apellido_paterno = rest.ApellidoPaterno;
                response.apellido_materno = rest.ApellidoMaterno;

                return response;
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("500", ex.Message);
            }

        }
        public object generar(string conexion, ReporteCrediticio_Input model, SentinelInfo info, IHttpClientFactory client_factory)
        {

            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string id_trx_hub = "";
            string id_trans_global = "";
            GlobalService global_service = new GlobalService();

            dynamic obj = null;
            string mensaje_error = "";
            try
            {
                con_sql = new SqlConnection(conexion);

                //1) Inserta TRX_HUB y validaciones por BD

                //Obtener Distribuidor
                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;

                con_sql.Open();
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);
                con_sql.Close();

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    mensaje_error = "El código de distribuidor " + distribuidor.nu_id_distribuidor.ToString() + " no existe";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("01", mensaje_error);
                }

                //Obtener Comercio
                con_sql.Open();
                ComercioModel comercio = global_service.get_comercio(con_sql, model.codigo_comercio, model.nombre_comercio, distribuidor.nu_id_distribuidor);
                con_sql.Close();


                //Obtener Producto
                ProductoModel producto = new ProductoModel();
                producto.nu_id_producto = Convert.ToInt32(model.id_producto);
                producto.nu_id_distribuidor = distribuidor.nu_id_distribuidor;

                con_sql.Open();
                producto = global_service.get_producto(con_sql, producto);
                con_sql.Close();

                if (producto.nu_id_producto <= 0)
                {
                    mensaje_error = "El producto no existe.";
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("05", mensaje_error);
                }

                con_sql.Open();
                TipoDocIdentidadModel tipodocidentidad_solicitante = new TipoDocIdentidadModel();
                try
                {
                    tipodocidentidad_solicitante = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_solicitante), producto.nu_id_convenio ?? 2);
                }
                catch (Exception ex)
                {
                    tipodocidentidad_solicitante = global_service.get_tipo_documento_codigo(con_sql, model.tipo_documento_solicitante, producto.nu_id_convenio ?? 2);
                }

                if (tipodocidentidad_solicitante.nu_id_tipo_doc_identidad <= 0)
                {
                    _logger.Error("El tipo de documento de solicitante " + tipodocidentidad_solicitante.nu_id_tipo_doc_identidad.ToString() + " no existe");
                    return UtilSql.sOutPutTransaccion("30", "El tipo de documento de solicitante no existe.");
                }

                TipoDocIdentidadModel tipodocidentidad_consultado = new TipoDocIdentidadModel();
                try
                {
                    tipodocidentidad_consultado = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultado), producto.nu_id_convenio ?? 2);
                }
                catch (Exception ex)
                {
                    tipodocidentidad_consultado = global_service.get_tipo_documento_codigo(con_sql, model.tipo_documento_consultado, producto.nu_id_convenio ?? 2);
                }

                if (tipodocidentidad_consultado.nu_id_tipo_doc_identidad <= 0)
                {
                    _logger.Error("El tipo de documento de consultado " + tipodocidentidad_consultado.nu_id_tipo_doc_identidad.ToString() + " no existe");

                    return UtilSql.sOutPutTransaccion("31", "El tipo de documento de consultado no existe.");
                }

                //Insertar Transaccion HUB
                TrxHubModel trx_hub = new TrxHubModel();
                trx_hub.vc_cod_distribuidor = model.codigo_distribuidor;
                trx_hub.vc_cod_comercio = model.codigo_comercio;
                trx_hub.vc_nombre_comercio = model.nombre_comercio;
                trx_hub.vc_id_producto = model.id_producto;

                trx_hub.nu_id_tipo_doc_sol = tipodocidentidad_solicitante.nu_id_tipo_doc_identidad;
                trx_hub.vc_nro_doc_sol = model.numero_documento_solicitante;
                trx_hub.ch_dig_ver_sol = model.digito_verificador_solicitante;
                trx_hub.vc_nro_telefono = model.telefono_solicitante == null ? "" : model.telefono_solicitante;
                trx_hub.vc_email = model.email_solicitante;

                trx_hub.nu_id_tipo_doc_cpt = tipodocidentidad_consultado.nu_id_tipo_doc_identidad;
                trx_hub.vc_nro_doc_cpt = model.numero_documento_consultado;

                trx_hub.nu_id_tipo_comprobante = model.tipo_documento_facturacion;
                trx_hub.vc_ruc = model.numero_ruc;
                trx_hub.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;


                cmd = global_service.insTrxhub(con_sql, trx_hub);
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                }
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 2)
                {
                    mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", mensaje_error);
                }

                id_trx_hub = cmd.Parameters["@nu_tran_pkey"].Value.ToString();


                _logger.Information("idtrx: " + id_trx_hub + " / " + "Inicio de transaccion");
                _logger.Information("idtrx: " + id_trx_hub + " / " + "Modelo recibido: " + JsonConvert.SerializeObject(model));

                // 2) Validar Campos adicionales.
                if (!new EmailAddressAttribute().IsValid(model.email_solicitante))
                {
                    mensaje_error = "El email " + model.email_solicitante + " es incorrecto";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("03", mensaje_error);
                }

                if (!Regex.Match(model.id_producto, @"(^[0-9]+$)").Success)
                {
                    mensaje_error = "El id del producto " + model.id_producto + " debe ser numerico";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("04", mensaje_error);
                }
                if (string.IsNullOrEmpty(model.digito_verificador_solicitante) && producto.nu_id_convenio == 4)
                {
                    mensaje_error = "El digito verificador es obligatorio";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + " " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("22", mensaje_error);
                }

                TipoDocIdentidadModel tipodocidentidad_PDV = new TipoDocIdentidadModel();
                try
                {
                    tipodocidentidad_PDV = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_PDV), producto.nu_id_convenio ?? 2);
                }
                catch (Exception ex)
                {
                    tipodocidentidad_PDV = global_service.get_tipo_documento_codigo(con_sql, model.tipo_documento_PDV, producto.nu_id_convenio ?? 2);
                }

                if (tipodocidentidad_PDV.nu_id_tipo_doc_identidad <= 0 && producto.nu_id_convenio == 2)
                {
                    mensaje_error = "El tipo de documento del PDV " + tipodocidentidad_PDV.nu_id_tipo_doc_identidad.ToString() + " no existe";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("32", mensaje_error);
                }

                con_sql.Close();


                //Dirigir a APIS
                if (producto.nu_id_convenio == 2)
                {
                    //SENTINEL
                    Sentinel_InputModel.Ins_Transaccion model_sentinel = new Sentinel_InputModel.Ins_Transaccion();
                    model_sentinel.codigo_distribuidor = model.codigo_distribuidor;
                    model_sentinel.codigo_comercio = model.codigo_comercio;
                    model_sentinel.nombre_comercio = model.nombre_comercio;
                    model_sentinel.id_producto = model.id_producto;
                    model_sentinel.tipo_documento_solicitante = model.tipo_documento_solicitante;
                    model_sentinel.numero_documento_solicitante = model.numero_documento_solicitante;
                    model_sentinel.digito_verificador_solicitante = model.digito_verificador_solicitante;
                    model_sentinel.telefono_solicitante = model.telefono_solicitante;
                    model_sentinel.email_solicitante = model.email_solicitante;
                    model_sentinel.tipo_documento_consultado = model.tipo_documento_consultado;
                    model_sentinel.numero_documento_consultado = model.numero_documento_consultado;
                    model_sentinel.tipo_documento_facturacion = model.tipo_documento_facturacion;
                    model_sentinel.numero_ruc = model.numero_ruc;
                    model_sentinel.tipo_documento_PDV = model.tipo_documento_PDV;
                    model_sentinel.numero_documento_PDV = model.numero_documento_PDV;
                    model_sentinel.razon_social_PDV = model.razon_social_PDV;
                    model_sentinel.nro_transaccion_referencia = model.nro_transaccion_referencia;
                    model_sentinel.bono = model.bono;

                    SentinelService Sentinel_Service = new SentinelService(_logger);

                    obj = Sentinel_Service.ins_transaccion(conexion, info, model_sentinel, id_trx_hub, tipodocidentidad_solicitante, tipodocidentidad_consultado, tipodocidentidad_PDV, distribuidor, comercio, producto);
                }
                else if (producto.nu_id_convenio == 4)
                {
                    //EQUIFAX
                    GenerarReporte model_equifax = new GenerarReporte();
                    model_equifax.codigo_distribuidor = model.codigo_distribuidor;
                    model_equifax.codigo_comercio = model.codigo_comercio;
                    model_equifax.nombre_comercio = model.nombre_comercio;
                    model_equifax.id_producto = model.id_producto;
                    model_equifax.tipo_documento_solicitante = model.tipo_documento_solicitante;
                    model_equifax.numero_documento_solicitante = model.numero_documento_solicitante;
                    model_equifax.digito_verificador_solicitante = model.digito_verificador_solicitante;
                    model_equifax.email_solicitante = model.email_solicitante;
                    model_equifax.tipo_documento_consultado = model.tipo_documento_consultado;
                    model_equifax.numero_documento_consultado = model.numero_documento_consultado;
                    model_equifax.nro_transaccion_referencia = model.nro_transaccion_referencia;

                    EquifaxService Equifax_Service = new EquifaxService(_logger);

                    obj = Equifax_Service.GenerarReporte(conexion, model_equifax, id_trx_hub, tipodocidentidad_solicitante, tipodocidentidad_consultado, distribuidor, comercio, producto);

                }
                else
                {
                    mensaje_error = "El producto  " + model.id_producto + " no está habilitado para el distribuidor.";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("80", mensaje_error);
                }
                _logger.Information("idtrx: " + id_trx_hub + " / " + "Modelo enviado: " + JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));


                return obj;

            }
            catch (Exception ex)
            {
                mensaje_error = ex.Message;
                return UtilSql.sOutPutTransaccion("500", ex.Message);
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();

                if (id_trx_hub != "" && mensaje_error != "")
                {
                    con_sql.Open();
                    TrxHubModel model_hub_error = new TrxHubModel();

                    model_hub_error.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                    model_hub_error.vc_mensaje_error = mensaje_error;
                    var cmd_trxhub_error = global_service.updTrxhubError(con_sql, model_hub_error);
                    con_sql.Close();

                }

            }

        }
    }
}
