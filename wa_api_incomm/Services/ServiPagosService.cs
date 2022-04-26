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
        public ServiPagosService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object RealizarRecarga(string conexion, ServiPagos_Input model)
        {
            bool ins_bd = false;
            string id_trans_global = "";
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string codigo_error = "";
            string mensaje_error = "";

            GlobalService global_service = new GlobalService();

            ServiPagosModel model_sql = new ServiPagosModel();
            try
            {
                _logger.Information("idtrx: " + model.id_trx_hub + " / " + "Inicio de transaccion");

                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                if (!Regex.Match(model.numero_servicio, @"(^[0-9]+$)").Success)
                {
                    return UtilSql.sOutPutTransaccion("500", "El número del cliente debe ser numerico");
                }

                if (!Regex.Match(model.importe_recarga, @"^[0-9]+(\.[0-9]{1,2})?$").Success)
                {
                    return UtilSql.sOutPutTransaccion("500", "El importe de recarga debe ser numerico");
                }

                if (!Regex.Match(model.id_producto, @"(^[0-9]+$)").Success)
                {
                    return UtilSql.sOutPutTransaccion("04", "El id del producto debe ser numerico");
                }

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;

                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("05", "El código de distribuidor no existe");
                }

                ComercioModel comercio = global_service.get_comercio(con_sql, model.codigo_comercio, model.nombre_comercio, distribuidor.nu_id_distribuidor);

                ProductoModel producto = new ProductoModel();
                producto.nu_id_producto = Convert.ToInt32(model.id_producto);
                producto.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                producto.nu_id_convenio = nu_id_convenio;

                producto = global_service.get_producto(con_sql, producto);

                if (producto.nu_id_producto <= 0)
                {
                    return UtilSql.sOutPutTransaccion("06", "El producto no existe");
                }

                con_sql.Close();

                con_sql.Open();

                //Variables BD
                var idtran = global_service.get_id_transaccion(con_sql);
                id_trans_global = idtran.ToString();
                var fechatran = DateTime.Now;

                con_sql.Close();
                //graba primero en BD

                con_sql.Open();
                tran_sql = con_sql.BeginTransaction();
                ins_bd = true;

                model_sql.vc_numero_servicio = model.numero_servicio;
                model_sql.nu_id_tipo_moneda_vta = 1; // SOLES
                model_sql.nu_precio_vta = Convert.ToDecimal(model.importe_recarga);
                model_sql.vc_tran_usua_regi = "API";
                model_sql.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;

                using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_recargas", con_sql, tran_sql))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nu_id_trx", id_trans_global);
                    cmd.Parameters.AddWithValue("@nu_id_trx_hub", model.id_trx_hub);
                    cmd.Parameters.AddWithValue("@nu_id_distribuidor", distribuidor.nu_id_distribuidor);
                    cmd.Parameters.AddWithValue("@nu_id_comercio", comercio.nu_id_comercio);
                    cmd.Parameters.AddWithValue("@nu_id_producto", producto.nu_id_producto);
                    cmd.Parameters.AddWithValue("@vc_numero_servicio", model_sql.vc_numero_servicio);
                    cmd.Parameters.AddWithValue("@nu_id_tipo_moneda_vta", model_sql.nu_id_tipo_moneda_vta);
                    cmd.Parameters.AddWithValue("@nu_precio_vta", model_sql.nu_precio_vta);
                    cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", model_sql.vc_id_ref_trx_distribuidor);

                    UtilSql.iIns(cmd, model_sql);
                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                    {
                        tran_sql.Rollback();
                        ins_bd = false;
                        _logger.Error("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        return UtilSql.sOutPutTransaccion("99", cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    }
                    model_sql.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();

                    ServiPagosApi api = new ServiPagosApi();

                    ServiPagos_InputModel model_api = new ServiPagos_InputModel();
                    model_api.vc_cod_producto = producto.vc_cod_producto;
                    model_api.vc_numero_servicio = model_sql.vc_numero_servicio;
                    model_api.nu_precio_vta = model_sql.nu_precio_vta.ToString();

                    var response = api.Recargar(model_api, model_sql.nu_id_trx_app, _logger, model.id_trx_hub).Result;
                    
                    if (response.respuesta.resultado == "200")
                    {
                        model_sql.vc_id_ref_trx = response.respuesta.transacid.ToString();
                        model_sql.vc_cod_autorizacion = (response.respuesta.nro_op == null) ? "" : response.respuesta.nro_op.ToString();

                        using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", con_sql, tran_sql))
                        {
                            cmd_upd.CommandType = CommandType.StoredProcedure;
                            cmd_upd.Parameters.AddWithValue("@nu_id_trx", model_sql.nu_id_trx_app);
                            cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", model_sql.vc_id_ref_trx);
                            cmd_upd.Parameters.AddWithValue("@vc_cod_autorizacion", model_sql.vc_cod_autorizacion);
                            UtilSql.iUpd(cmd_upd, model_sql);
                            cmd_upd.ExecuteNonQuery();
                            if (cmd_upd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                            {
                                tran_sql.Rollback();
                                ins_bd = false;
                                _logger.Error("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                                return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                            }
                            cmd.Parameters["@vc_tran_codi"].Value = cmd_upd.Parameters["@vc_tran_codi"].Value;
                        }

                        using (var cmd_upd_confirmar = new SqlCommand("tisi_trx.usp_upd_transaccion_confirmar", con_sql, tran_sql))
                        {
                            cmd_upd_confirmar.CommandType = CommandType.StoredProcedure;
                            cmd_upd_confirmar.Parameters.AddWithValue("@nu_id_trx", model_sql.nu_id_trx_app);
                            cmd_upd_confirmar.Parameters.AddWithValue("@nu_id_distribuidor", distribuidor.nu_id_distribuidor);
                            cmd_upd_confirmar.Parameters.AddWithValue("@nu_id_comercio", comercio.nu_id_comercio);
                            cmd_upd_confirmar.Parameters.AddWithValue("@bi_confirmado", true);
                            UtilSql.iUpd(cmd_upd_confirmar, model_sql);
                            cmd_upd_confirmar.ExecuteNonQuery();
                            if (cmd_upd_confirmar.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                            {
                                tran_sql.Rollback();
                                ins_bd = false;
                                _logger.Error("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                                return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                            }
                        }

                        tran_sql.Commit();
                        ins_bd = false;
                        con_sql.Close();

                        _logger.Information("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());

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
                        tran_sql.Rollback();
                        con_sql.Close();

                        TransaccionModel tm = new TransaccionModel();
                        tm.nu_id_trx = Convert.ToInt32(id_trans_global);
                        tm.nu_id_trx_hub = Convert.ToInt64(model.id_trx_hub);
                        tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                        tm.nu_id_comercio = comercio.nu_id_comercio;
                        tm.dt_fecha = DateTime.Now;
                        tm.nu_id_producto = producto.nu_id_producto;
                        tm.nu_precio = model_sql.nu_precio_vta ?? 0;
                        tm.nu_id_tipo_moneda_vta = model_sql.nu_id_tipo_moneda_vta;
                        tm.vc_numero_servicio = model_sql.vc_numero_servicio;
                        tm.vc_tran_usua_regi = "API";

                        tm.vc_cod_error = response.respuesta.resultado;

                        tm.vc_desc_error = global_service.get_mensaje_error(nu_id_convenio, tm.vc_cod_error) + (response.respuesta.obs != "" ? (" - " + response.respuesta.obs) : "");
                        
                        tm.vc_desc_tipo_error = "CONVENIO";

                        SqlTransaction tran_sql_error = null;
                        con_sql.Open();

                        tran_sql_error = con_sql.BeginTransaction();

                        cmd = global_service.insTransaccionError(con_sql, tran_sql_error, tm);

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                        {
                            tran_sql_error.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                            return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                        }

                        tran_sql_error.Commit();
                        ins_bd = false;
                        _logger.Error("idtrx: " + model.id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);
                        
                        return UtilSql.sOutPutTransaccion(tm.vc_cod_error, tm.vc_desc_error);

                    }
                }
            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }
                
                _logger.Error("idtrx: " + model.id_trx_hub + " / " + ex.Message);
                return UtilSql.sOutPutTransaccion("500", ex.Message);
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }

        }
    }
}
