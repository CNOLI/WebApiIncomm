using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Models.ServiPagos;
using static wa_api_incomm.Models.ServiPagos.ServiPagos_InputModel;

namespace wa_api_incomm.Services
{
    public class ServiPagosService
    {
        public int nu_id_convenio = 6;

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        private readonly Serilog.ILogger _logger;

        Models.Hub.ConvenioModel hub_convenio;
        public ServiPagosService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object RealizarRecarga(string conexion, ServiPagos_Input model)
        {
            bool ins_bd = false;
            SqlConnection con_sql = null;
            bool saldo_comprometido = false;
            bool transaccion_completada = false;
            string id_trans_global = "";
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string codigo_error = "";
            string mensaje_error = "";
            GlobalService global_service = new GlobalService();
            try
            {
                con_sql = new SqlConnection(conexion);
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                // 3) Obtener ID Transacción y comprometer saldo.
                con_sql.Open();
                var idtran = global_service.get_id_transaccion(con_sql);
                var fechatran = DateTime.Now;

                id_trans_global = idtran.ToString();

                TrxHubModel model_saldo = new TrxHubModel();

                model_saldo.nu_id_trx_hub = Convert.ToInt64(model.id_trx_hub);

                var cmd_saldo = global_service.updTrxhubSaldo(con_sql, model_saldo);

                if (cmd_saldo.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", mensaje_error);
                }
                saldo_comprometido = true;

                con_sql.Close();

                // 4) Enviar Solicitud al proveedor
                ServiPagosApi api = new ServiPagosApi(hub_convenio);

                ServiPagos_InputModel model_api = new ServiPagos_InputModel();
                model_api.vc_cod_producto = model.vc_cod_producto;
                model_api.vc_numero_servicio = model.numero_servicio;
                model_api.nu_precio_vta = model.importe_recarga.ToString();
                model_api.ubigeo = model.ubigeo;

                var response = api.Recargar(model_api, idtran, _logger, model.id_trx_hub).Result;

                TransaccionModel trx = new TransaccionModel();
                trx.nu_id_trx = idtran;
                trx.nu_id_trx_hub = Int64.Parse(model.id_trx_hub);
                trx.nu_id_distribuidor = int.Parse(model.id_distribuidor);
                trx.nu_id_comercio = int.Parse(model.id_comercio);
                trx.dt_fecha = fechatran;
                trx.nu_id_producto = int.Parse(model.id_producto);
                trx.nu_precio = Convert.ToDecimal(model.importe_recarga);
                trx.nu_id_tipo_moneda_vta = 1; // SOLES
                trx.vc_tran_usua_regi = "API";
                trx.vc_numero_servicio = model.numero_servicio;
                trx.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;
                trx.vc_ubigeo = model.ubigeo;
                try { trx.ti_respuesta_api = (response.dt_fin - response.dt_inicio); } catch (Exception ti) { }
                
                //Validar Timeout
                if (response.timeout == true)
                {
                    var response_consulta = api.Consultar(model_api, idtran, _logger, model.id_trx_hub).Result;

                    if (response_consulta.respuesta.resultado == "200")
                    {
                        response = new ServiPagos_ResponseModel();
                        if (response_consulta.respuesta.datos.resultado == "")
                        {
                            response.respuesta.resultado = "99";
                            response.respuesta.transacid = response_consulta.respuesta.datos.transacid;
                            response.respuesta.nro_op = response_consulta.respuesta.datos.nro_op;
                        }
                        else
                        {
                            response.respuesta.resultado = response_consulta.respuesta.datos.resultado;
                            response.respuesta.transacid = response_consulta.respuesta.datos.transacid;
                            response.respuesta.nro_op = response_consulta.respuesta.datos.nro_op;
                        }
                    }
                    else
                    {
                        response = new ServiPagos_ResponseModel();
                        response.respuesta.resultado = response_consulta.respuesta.resultado;
                        response.respuesta.obs = response_consulta.respuesta.obs;
                    }
                }


                if (response.respuesta.resultado == "200")
                {
                    trx.vc_id_ref_trx = response.respuesta.transacid.ToString();
                    trx.vc_cod_autorizacion = (response.respuesta.nro_op == null) ? "" : response.respuesta.nro_op.ToString();
                    
                    //Graba BD
                    con_sql.Open();
                    tran_sql = con_sql.BeginTransaction();
                    ins_bd = true;

                    using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_recargas", con_sql, tran_sql))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx);
                        cmd.Parameters.AddWithValue("@nu_id_trx_hub", trx.nu_id_trx_hub);
                        cmd.Parameters.AddWithValue("@nu_id_distribuidor", trx.nu_id_distribuidor);
                        cmd.Parameters.AddWithValue("@nu_id_comercio", trx.nu_id_comercio);
                        cmd.Parameters.AddWithValue("@nu_id_producto", trx.nu_id_producto);
                        cmd.Parameters.AddWithValue("@vc_numero_servicio", trx.vc_numero_servicio);
                        cmd.Parameters.AddWithValue("@nu_id_tipo_moneda_vta", trx.nu_id_tipo_moneda_vta);
                        cmd.Parameters.AddWithValue("@nu_precio_vta", trx.nu_precio);
                        cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", trx.vc_id_ref_trx_distribuidor);
                        cmd.Parameters.AddWithValue("@ti_respuesta_api", trx.ti_respuesta_api);
                        cmd.Parameters.AddWithValue("@vc_ubigeo", trx.vc_ubigeo);

                        UtilSql.iIns(cmd, trx);
                        cmd.ExecuteNonQuery();

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran_sql.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                            mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                            return UtilSql.sOutPutTransaccion("99", cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        }
                        trx.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();

                    }

                    using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", con_sql, tran_sql))
                    {
                        cmd_upd.CommandType = CommandType.StoredProcedure;
                        cmd_upd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx_app);
                        cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", trx.vc_id_ref_trx);
                        cmd_upd.Parameters.AddWithValue("@vc_cod_autorizacion", trx.vc_cod_autorizacion);
                        UtilSql.iUpd(cmd_upd, trx);
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
                        UtilSql.iUpd(cmd_upd_confirmar, trx);
                        cmd_upd_confirmar.ExecuteNonQuery();
                        if (cmd_upd_confirmar.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran_sql.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd_upd_confirmar.Parameters["@tx_tran_mnsg"].Value.ToText());
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
                        mensaje = "Operación exitosa, la recarga se hará efectiva en unos segundos.",
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
                    tm.vc_numero_servicio = trx.vc_numero_servicio;
                    tm.vc_tran_usua_regi = "API";
                    tm.ti_respuesta_api = trx.ti_respuesta_api;

                    tm.vc_cod_error = response.respuesta.resultado;

                    tm.vc_desc_error = global_service.get_mensaje_error(nu_id_convenio, tm.vc_cod_error) + (response.respuesta.obs != "" ? (" - " + response.respuesta.obs) : "");

                    tm.vc_desc_tipo_error = "CONVENIO";

                    SqlTransaction tran_sql_error = null;
                    con_sql.Open();

                    tran_sql_error = con_sql.BeginTransaction();
                    ins_bd = true;

                    cmd = global_service.insTransaccionError(con_sql, tran_sql_error, tm);

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql_error.Rollback();
                        ins_bd = false;
                        _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                        mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                        return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                    }

                    tran_sql_error.Commit();
                    con_sql.Close();
                    mensaje_error = tm.vc_desc_error;
                    ins_bd = false;

                    _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);

                    //return UtilSql.sOutPutTransaccion(tm.vc_cod_error, tm.vc_desc_error);
                    return UtilSql.sOutPutTransaccion("99", tm.vc_desc_error);
                }
            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }
                
                _logger.Error("idtrx: " + model.id_trx_hub + " / " + ex.Message);
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

                    model_saldo_extorno.nu_id_trx_hub = Convert.ToInt64(model.id_trx_hub);
                    model_saldo_extorno.bi_extorno = true;
                    model_saldo_extorno.bi_error = true;
                    model_saldo_extorno.vc_mensaje_error = mensaje_error;
                    var cmd_saldo_extorno = global_service.updTrxhubSaldo(con_sql, model_saldo_extorno);
                    con_sql.Close();
                }
                if (transaccion_completada == false)
                {
                    con_sql.Open();
                    TrxHubModel model_hub_error = new TrxHubModel();

                    model_hub_error.nu_id_trx_hub = Convert.ToInt64(model.id_trx_hub);
                    model_hub_error.vc_mensaje_error = mensaje_error;
                    var cmd_trxhub_error = global_service.updTrxhubError(con_sql, model_hub_error);
                    con_sql.Close();

                }
            }

        }
    }
}
