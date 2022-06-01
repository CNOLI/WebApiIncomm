using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Equifax_InputModel;
using static wa_api_incomm.Models.Equifax_ResponseModel;
using static wa_api_incomm.Models.EquifaxModel;
using wa_api_incomm.Services;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace wa_api_incomm.Services
{
    public class EquifaxService : IEquifaxService
    {
        public int nu_id_convenio = 4;

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        private readonly Serilog.ILogger _logger;

        Models.Hub.ConvenioModel hub_convenio;
        public EquifaxService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object sel_tipo_documento_identidad(string conexion, Equifax_InputModel.Busqueda_Equifax_Input input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    Sentinel_ResponseModel model_response = new Sentinel_ResponseModel();
                    ProductoModel model = new ProductoModel();
                    model.vc_tran_clve_find = input.buscador;

                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_tipo_doc_identidad", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 3;

                        UtilSql.iGet(cmd, model);
                        SqlDataReader dr = cmd.ExecuteReader();
                        var lm = new List<Sentinel_ResponseModel>();
                        while (dr.Read())
                        {
                            model_response = new Sentinel_ResponseModel();
                            if (UtilSql.Ec(dr, "NU_ID_TIPO_DOC_IDENTIDAD"))
                                model_response.id_tipo_documento_identidad = dr["NU_ID_TIPO_DOC_IDENTIDAD"].ToInt32();
                            if (UtilSql.Ec(dr, "VC_COD_TIPO_DOC_IDENTIDAD"))
                                model_response.codigo_tipo_documento_identidad = dr["VC_COD_TIPO_DOC_IDENTIDAD"].ToString();
                            if (UtilSql.Ec(dr, "VC_DESC_TIPO_DOC_IDENTIDAD"))
                                model_response.descripcion_tipo_documento_identidad = dr["VC_DESC_TIPO_DOC_IDENTIDAD"].ToString();
                            lm.Add(model_response);
                        }
                        return lm;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }
        public object GenerarReporte(string conexion, GenerarReporte model)
        {
            bool ins_bd = false;
            bool saldo_comprometido = false;
            bool transaccion_completada = false;
            string id_trx_hub = "";
            string id_trans_global = "";
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string codigo_error = "";
            string mensaje_error = "";

            EquifaxModel model_sql = new EquifaxModel();

            Generar_Reporte_Response e_response = new Generar_Reporte_Response();

            GlobalService global_service = new GlobalService();
            try
            {
                con_sql = new SqlConnection(conexion);

                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();


                //1) Inserta TRX_HUB y validaciones por BD

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;

                con_sql.Open();
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);
                con_sql.Close();

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El código de distribuidor " + model.codigo_distribuidor + " no existe");
                    return UtilSql.sOutPutTransaccion("01", "El código de distribuidor no existe");
                }

                //Obtener Comercio
                con_sql.Open();
                ComercioModel comercio = global_service.get_comercio(con_sql, model.codigo_comercio, model.nombre_comercio, distribuidor.nu_id_distribuidor);
                con_sql.Close();

                //Insertar Transaccion HUB
                con_sql.Open();

                TipoDocIdentidadModel tipodocidentidad_consultante = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultante), nu_id_convenio);
                if (tipodocidentidad_consultante.nu_id_tipo_doc_identidad <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El tipo de documento de consultante no existe.");
                    return UtilSql.sOutPutTransaccion("20", "El tipo de documento de consultante no existe.");
                }

                TipoDocIdentidadModel tipodocidentidad_consultado = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultado), nu_id_convenio);
                if (tipodocidentidad_consultado.nu_id_tipo_doc_identidad <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El tipo de documento de consultado no existe.");
                    return UtilSql.sOutPutTransaccion("21", "El tipo de documento de consultado no existe.");
                }

                TrxHubModel trx_hub = new TrxHubModel();
                trx_hub.vc_cod_distribuidor = model.codigo_distribuidor;
                trx_hub.vc_cod_comercio = model.codigo_comercio;
                trx_hub.vc_nombre_comercio = model.nombre_comercio;
                trx_hub.vc_nro_telefono = "";
                trx_hub.vc_email = model.email_consultante;
                trx_hub.vc_id_producto = model.id_producto;
                trx_hub.nu_id_tipo_doc_sol = tipodocidentidad_consultante.nu_id_tipo_doc_identidad;
                trx_hub.vc_nro_doc_sol = model.numero_documento_consultante;
                trx_hub.ch_dig_ver_sol = model.digito_verificador_consultante;
                trx_hub.nu_id_tipo_doc_cpt = tipodocidentidad_consultado.nu_id_tipo_doc_identidad;
                trx_hub.vc_nro_doc_cpt = model.numero_documento_consultado;
                trx_hub.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;
                
                cmd = global_service.insTrxhub(con_sql, trx_hub);
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    _logger.Error(cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                }
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 2)
                {
                    mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", mensaje_error);
                }

                id_trx_hub = cmd.Parameters["@nu_tran_pkey"].Value.ToString();

                con_sql.Close();

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Inicio de transaccion");

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Modelo recibido: " + JsonConvert.SerializeObject(model));

                // 2) Validar Campos adicionales.
                if (!new EmailAddressAttribute().IsValid(model.email_consultante))
                {
                    mensaje_error = "El email " + model.email_consultante + " es incorrecto";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("03", mensaje_error);
                }

                if (!Regex.Match(model.id_producto, @"(^[0-9]+$)").Success)
                {
                    mensaje_error = "El id del producto " + model.id_producto + " debe ser numerico";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("04", mensaje_error);
                }
                if (string.IsNullOrEmpty(model.digito_verificador_consultante))
                {
                    mensaje_error = "El digito verificador es obligatorio";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + " " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("22", mensaje_error);
                }

                //Obtener Producto
                ProductoModel producto = new ProductoModel();
                producto.nu_id_producto = Convert.ToInt32(model.id_producto);
                producto.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                producto.nu_id_convenio = nu_id_convenio;

                con_sql.Open();
                producto = global_service.get_producto(con_sql, producto);
                con_sql.Close();

                if (producto.nu_id_producto <= 0)
                {
                    mensaje_error = "El producto " + model.id_producto + " no existe";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("05", mensaje_error);
                }

                // 3) Obtener ID Transacción y comprometer saldo.
                con_sql.Open();
                var idtran = global_service.get_id_transaccion(con_sql);
                var fechatran = DateTime.Now;

                id_trans_global = idtran.ToString();

                TrxHubModel model_saldo = new TrxHubModel();

                model_saldo.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);

                var cmd_saldo = global_service.updTrxhubSaldo(con_sql, model_saldo);

                if (cmd_saldo.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    mensaje_error = cmd_saldo.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", mensaje_error);
                }
                saldo_comprometido = true;

                con_sql.Close();

                // 4) Enviar Solicitud al proveedor

                Generar_Reporte_Input e_input = new Generar_Reporte_Input();
                e_input.documentType = Convert.ToInt32(tipodocidentidad_consultante.vc_cod_tipo_doc_identidad);
                e_input.documentNumber = model.numero_documento_consultante;
                e_input.email = model.email_consultante;
                e_input.verifierDigit = model.digito_verificador_consultante;
                e_input.documentTypeThird = Convert.ToInt32(tipodocidentidad_consultado.vc_cod_tipo_doc_identidad);
                e_input.documentNumberThird = model.numero_documento_consultado;
                
                EquifaxApi client = new EquifaxApi(hub_convenio);
                Generar_Reporte_Response response = client.Generar_Reporte(e_input, _logger, id_trx_hub).Result;

                TransaccionModel trx = new TransaccionModel();
                trx.nu_id_trx = idtran;
                trx.nu_id_trx_hub = Int64.Parse(id_trx_hub);
                trx.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                trx.nu_id_comercio = comercio.nu_id_comercio;
                trx.dt_fecha = fechatran;
                trx.nu_id_producto = producto.nu_id_producto;
                trx.nu_precio = producto.nu_precio ?? 0;
                trx.vc_tran_usua_regi = "API";
                trx.nu_id_tipo_moneda_vta = 1; // SOLES
                trx.vc_numero_servicio = "";
                trx.nu_id_tipo_doc_sol = tipodocidentidad_consultante.nu_id_tipo_doc_identidad;
                trx.vc_nro_doc_sol = model.numero_documento_consultante;
                trx.ch_dig_ver_sol = model.digito_verificador_consultante;
                trx.vc_email_sol = model.email_consultante;
                trx.nu_id_tipo_doc_cpt = tipodocidentidad_consultado.nu_id_tipo_doc_identidad;
                trx.vc_nro_doc_cpt = model.numero_documento_consultado;
                trx.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;
                try { trx.ti_respuesta_api = (response.dt_fin - response.dt_inicio); } catch (Exception ti) { }
                

                if (response.code == "201")
                {
                    trx.vc_id_ref_trx = response.data.ToString();

                    //graba primero en BD

                    con_sql.Open();
                    tran_sql = con_sql.BeginTransaction();
                    ins_bd = true;

                    using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_equifax", con_sql, tran_sql))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx);
                        cmd.Parameters.AddWithValue("@nu_id_trx_hub", trx.nu_id_trx_hub);
                        cmd.Parameters.AddWithValue("@nu_id_distribuidor", trx.nu_id_distribuidor);
                        cmd.Parameters.AddWithValue("@nu_id_comercio", trx.nu_id_comercio);
                        cmd.Parameters.AddWithValue("@nu_id_producto", trx.nu_id_producto);
                        cmd.Parameters.AddWithValue("@nu_id_tipo_doc_sol", trx.nu_id_tipo_doc_sol);
                        cmd.Parameters.AddWithValue("@vc_nro_doc_sol", trx.vc_nro_doc_sol);
                        cmd.Parameters.AddWithValue("@ch_dig_ver_sol", trx.ch_dig_ver_sol);
                        cmd.Parameters.AddWithValue("@vc_email_sol", trx.vc_email_sol);
                        cmd.Parameters.AddWithValue("@nu_id_tipo_doc_cpt", trx.nu_id_tipo_doc_cpt);
                        cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", trx.vc_nro_doc_cpt);
                        cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", trx.vc_id_ref_trx_distribuidor);
                        cmd.Parameters.AddWithValue("@ti_respuesta_api", trx.ti_respuesta_api);

                        UtilSql.iIns(cmd, trx);
                        cmd.ExecuteNonQuery();

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran_sql.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                            mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                            return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                        }
                        trx.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();

                    }

                    using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", con_sql, tran_sql))
                    {
                        cmd_upd.CommandType = CommandType.StoredProcedure;
                        cmd_upd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx_app);
                        cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", trx.vc_id_ref_trx);
                        UtilSql.iUpd(cmd_upd, model_sql);
                        cmd_upd.ExecuteNonQuery();
                        if (cmd_upd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran_sql.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText());
                            mensaje_error = cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText();
                            return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                        }
                        cmd.Parameters["@vc_tran_codi"].Value = cmd_upd.Parameters["@vc_tran_codi"].Value;
                    }

                    using (var cmd_upd_confirmar = new SqlCommand("tisi_trx.usp_upd_transaccion_confirmar", con_sql, tran_sql))
                    {
                        cmd_upd_confirmar.CommandType = CommandType.StoredProcedure;
                        cmd_upd_confirmar.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx_app);
                        cmd_upd_confirmar.Parameters.AddWithValue("@nu_id_distribuidor", trx.nu_id_distribuidor);
                        cmd_upd_confirmar.Parameters.AddWithValue("@nu_id_comercio", trx.nu_id_comercio);
                        cmd_upd_confirmar.Parameters.AddWithValue("@bi_confirmado", true);
                        UtilSql.iUpd(cmd_upd_confirmar, model_sql);
                        cmd_upd_confirmar.ExecuteNonQuery();
                        if (cmd_upd_confirmar.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran_sql.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + id_trx_hub + " / " + cmd_upd_confirmar.Parameters["@tx_tran_mnsg"].Value.ToText());
                            mensaje_error = cmd_upd_confirmar.Parameters["@tx_tran_mnsg"].Value.ToText();
                            return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                        }
                    }

                    tran_sql.Commit();
                    ins_bd = false;
                    con_sql.Close();
                    transaccion_completada = true;

                    _logger.Information("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());

                    object info = new object();

                    info = new
                    {
                        codigo = "00",
                        mensaje = "Operación exitosa, la información le llegará al correo electrónico registrado.",
                        nro_transaccion = id_trans_global
                    };

                    return info;

                }
                else
                {
                    TransaccionModel tm = new TransaccionModel();
                    tm.nu_id_trx = trx.nu_id_trx;
                    tm.nu_id_trx_hub = trx.nu_id_trx_hub;
                    tm.nu_id_distribuidor = trx.nu_id_distribuidor;
                    tm.nu_id_comercio = trx.nu_id_comercio;
                    tm.dt_fecha = DateTime.Now;
                    tm.nu_id_producto = trx.nu_id_producto;
                    tm.nu_precio = trx.nu_precio;
                    tm.nu_id_tipo_moneda_vta = trx.nu_id_tipo_moneda_vta;
                    tm.vc_numero_servicio = "";
                    tm.vc_tran_usua_regi = "API";
                    tm.ti_respuesta_api = trx.ti_respuesta_api;

                    if (response.code == null)
                        tm.vc_cod_error = "";
                    else
                        tm.vc_cod_error = response.code;

                    if (response.message == null)
                        tm.vc_desc_error = "";
                    else
                        tm.vc_desc_error = response.message + " - " + string.Join("; ", response.errors);

                    tm.vc_desc_tipo_error = "CONVENIO";


                    SqlTransaction tran_sql_error = null;
                    con_sql.Open();

                    tran_sql_error = con_sql.BeginTransaction();
                    ins_bd = true;

                    cmd = global_service.insTransaccionError(con_sql, tran_sql_error, tm);

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql_error.Rollback();
                        _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                        mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                        return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                    }

                    tran_sql_error.Commit();
                    con_sql.Close();
                    mensaje_error = tm.vc_desc_error;
                    ins_bd = false;

                    _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);

                    return UtilSql.sOutPutTransaccion(tm.vc_cod_error, tm.vc_desc_error);

                }
            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }

                _logger.Error("idtrx: " + id_trx_hub + " / " + ex.Message);
                mensaje_error = ex.Message;

                return UtilSql.sOutPutTransaccion("500", ex.Message);
             
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();

                if (saldo_comprometido == true && transaccion_completada == false)
                {
                    con_sql.Open();
                    TrxHubModel model_saldo_extorno = new TrxHubModel();

                    model_saldo_extorno.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                    model_saldo_extorno.bi_extorno = true;
                    model_saldo_extorno.bi_error = true;
                    model_saldo_extorno.vc_mensaje_error = mensaje_error;
                    var cmd_saldo_extorno = global_service.updTrxhubSaldo(con_sql, model_saldo_extorno);
                    con_sql.Close();
                }
            }

        }

    }
}
