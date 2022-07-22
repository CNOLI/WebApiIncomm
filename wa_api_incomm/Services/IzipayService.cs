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
using static wa_api_incomm.Models.Izipay_InputModel;
using static wa_api_incomm.Models.Izipay_ResponseModel;
using static wa_api_incomm.Models.IzipayModel;
using wa_api_incomm.Models.IzipayFinazas;
using wa_api_incomm.Services;
using System.Text.RegularExpressions;
using System.Net.Http;
using wa_api_incomm.Models.Servicios;
using System.Globalization;

namespace wa_api_incomm.Services
{
    public class IzipayService : IIzipayService
    {
        public int nu_id_convenio = 5;

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();


        private readonly Serilog.ILogger _logger;

        Models.Hub.ConvenioModel hub_convenio;
        public IzipayService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object CrearComercio(string conexion, Crear_Comercio_Input model)
        {
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            List<DistribuidorModel> ls_distribuidor = new List<DistribuidorModel>();
            int cantidad = 0;
            GlobalService global_service = new GlobalService();
            try
            {
                con_sql = new SqlConnection(conexion);

                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                con_sql.Open();

                //Obtener la lista de distribuidores activos
                ls_distribuidor = sel_distribuidor(con_sql, model.id_distribuidor);

                foreach (var item in ls_distribuidor)
                {
                    if (item.bi_izipay == false && item.nu_id_comercio != null)
                    {

                        if (String.IsNullOrEmpty(item.vc_nombre_contacto))
                        {
                            return UtilSql.sOutPutTransaccion("XX", "El nombre de contacto es obligatorio.");
                        }
                        if (String.IsNullOrEmpty(item.vc_email_contacto))
                        {
                            return UtilSql.sOutPutTransaccion("XX", "El email de contacto es obligatorio.");
                        }
                        if (String.IsNullOrEmpty(item.vc_celular_contacto))
                        {
                            return UtilSql.sOutPutTransaccion("XX", "El celular de contacto es obligatorio.");
                        }

                        IzipayApi api = new IzipayApi(hub_convenio);
                        //consultar si el comercio existe
                        ResultApi result = api.Obtener_Comercio(new
                        {
                            secuencia = new Random().Next(100000, 999999),
                            fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                            userid = "HUB",
                            id_estab = item.nu_id_comercio.ToString()
                        }).Result;

                        //modelo para el insert/update
                        object obj_comercio = new
                        {
                            secuencia = new Random().Next(100000, 999999),
                            fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                            userid = "HUB",
                            establecimiento = new
                            {
                                bin_acq = config.GetSection("IzipayInfo:bin_acq").Value.ToString(),
                                id_estab = item.nu_id_comercio.ToString(),
                                id_distrib = config.GetSection("IzipayInfo:id_distrib").Value.ToString(),
                                nombre_comercial = item.vc_desc_distribuidor,
                                contacto = item.vc_nombre_contacto,
                                email = item.vc_email_contacto,
                                activo = "1"
                            }
                        };

                        if (result.rc == "00")
                        {
                            //actualizar el comercio
                            result = api.Actualizar_Comercio(obj_comercio).Result;
                        }
                        else
                        {
                            //crear un comercio
                            result = api.Crear_Comercio(obj_comercio).Result;
                        }
                        if (result == null)
                        {
                            return UtilSql.sOutPutTransaccion("XX", "Error al crear/actualizar comercio en IZIPAY.");
                        }
                        else if (result.rc != "00")
                        {
                            return UtilSql.sOutPutTransaccion("XX", result.descripcion);
                        }

                        item.vc_contraseña = "IZI" + item.vc_cod_distribuidor;
                        //Crear Terminal
                        object obj_terminal = new
                        {
                            secuencia = new Random().Next(100000, 999999),
                            fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                            userid = "HUB",
                            terminal = new
                            {
                                bin_acq = config.GetSection("IzipayInfo:bin_acq").Value.ToString(),
                                id_estab = item.nu_id_comercio.ToString(),
                                id_term = item.vc_celular_contacto,
                                secreto = item.vc_contraseña
                            }
                        };
                        result = api.Crear_Terminal(obj_terminal).Result;
                        if (result == null)
                        {
                            return UtilSql.sOutPutTransaccion("XX", "Error al crear terminal " + item.vc_celular_contacto + ".");
                        }
                        else if (result.rc != "00" && result.rc != "94")
                        {
                            return UtilSql.sOutPutTransaccion("XX", result.descripcion);
                        }
                        Upd_Contraseña(con_sql, item);
                        cantidad++;
                    }
                }

                object info = new object();
                info = new
                {
                    codigo = "00",
                    mensaje = "Operación realizada correctamente. " + "(" + cantidad.ToString() + ")"
                };
                return info;

            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }

        }
        public object ActualizarRegla(string conexion, Actualizar_Regla_Input model)
        {
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            int cantidad = 0;
            List<DistribuidorProductoModel> ls_distribuidor_producto = new List<DistribuidorProductoModel>();
            GlobalService global_service = new GlobalService();
            try
            {
                con_sql = new SqlConnection(conexion);

                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                con_sql.Open();

                //Obtener la lista de productos izipay del distribuidor activos
                ls_distribuidor_producto = sel_distribuidor_producto(con_sql, model.id_distribuidor, model.id_producto);

                foreach (var item in ls_distribuidor_producto)
                {
                    if (item.nu_id_regla_izipay == null)
                    {

                        IzipayApi api = new IzipayApi(hub_convenio);
                        //Crear Regla

                        object obj_regla = new
                        {
                            secuencia = new Random().Next(100000, 999999),
                            fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                            userid = "HUB",
                            regla = new
                            {
                                bin_acq = config.GetSection("IzipayInfo:bin_acq").Value.ToString(),
                                id_servicio = item.vc_cod_producto,
                                id_distrib = config.GetSection("IzipayInfo:id_distrib").Value.ToString(),
                                id_estab = item.nu_id_comercio.ToString(),
                                posicion = "1",
                                com_bolsa_tipo = "F",//item.ch_tipo_comision,
                                com_bolsa_val = "0.00",//item.nu_valor_comision,
                                activo = "1",
                                glosa = item.vc_desc_producto
                            }
                        };
                        ResultRegla result = api.Crear_Regla(obj_regla).Result;
                        if (result == null)
                        {
                            return UtilSql.sOutPutTransaccion("XX", "Error al registra la comisión.");
                        }
                        else if (result.rc != "00")
                        {
                            return UtilSql.sOutPutTransaccion("XX", result.descripcion);
                        }

                        item.nu_id_regla_izipay = result.id_regla.ToInt32();
                        Upd_Regla(con_sql, item);

                    }
                    else
                    {

                        IzipayApi api = new IzipayApi(hub_convenio);

                        object obj_regla = new
                        {
                            secuencia = new Random().Next(100000, 999999),
                            fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                            userid = "HUB",
                            regla = new
                            {
                                id_regla = item.nu_id_regla_izipay.ToString(),
                                id_servicio = item.vc_cod_producto,
                                bin_acq = config.GetSection("IzipayInfo:bin_acq").Value.ToString(),
                                id_distrib = config.GetSection("IzipayInfo:id_distrib").Value.ToString(),
                                posicion = "1",
                                com_bolsa_tipo = "F",//item.ch_tipo_comision,
                                com_bolsa_val = "0.00",//item.nu_valor_comision,
                                activo = "1",
                                glosa = item.vc_desc_producto
                            }
                        };

                        ResultApi result = api.Actualizar_Regla(obj_regla).Result;
                        if (result == null)
                        {
                            return UtilSql.sOutPutTransaccion("XX", "Error al actualizar regla en Izipay.");
                        }
                        else if (result.rc != "00")
                        {
                            return UtilSql.sOutPutTransaccion("XX", result.descripcion);
                        }
                    }
                    cantidad++;
                }

                object info = new object();
                info = new
                {
                    codigo = "00",
                    mensaje = "Operación realizada correctamente. " + "(" + cantidad.ToString() + ")"
                };
                return info;
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }


        }

        public object RealizarRecarga(string conexion, Pago_Directo_Input model, IHttpClientFactory client_factory)
        {
            bool ins_bd = false;
            bool saldo_comprometido = false;
            bool transaccion_completada = false;
            string id_trans_global = "";
            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string codigo_error = "";
            string mensaje_error = "";

            GlobalService global_service = new GlobalService();
            try
            {

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
                    return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                }
                saldo_comprometido = true;

                con_sql.Close();


                // 4) Enviar Solicitud al proveedor

                Token token = new Token()
                {
                    id_estab = config.GetSection("IzipayFinanzasInfo:id_estab").Value.ToString(),
                    id_term = config.GetSection("IzipayFinanzasInfo:id_term").Value.ToString(),
                    secreto = config.GetSection("IzipayFinanzasInfo:secreto").Value.ToString()
                };
                IziPayFinanzasApi api = new IziPayFinanzasApi(hub_convenio, client_factory, token);

                object model_api = new object();
                model_api = new
                {
                    secuencia = new Random().Next(100000, 999999),
                    fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                    pago = new
                    {
                        numero_cliente = model.numero_servicio.ToString(),
                        id_servicio = model.vc_cod_producto.ToString(),
                        importe = model.importe_recarga.ToString(),
                        cod_moneda = "604",
                        tipo_term = config.GetSection("IzipayFinanzasInfo:tipo_term").Value.ToString(),
                        tipo_cargo = "B",
                        merchant_type = config.GetSection("IzipayFinanzasInfo:merchant_type").Value.ToString(),
                        ubicacion = model.direccion + model.nombre_ciudad + model.codigo_provincia + model.codigo_pais,
                        id_term = config.GetSection("IzipayFinanzasInfo:id_term").Value.ToString(),
                        id_estab = config.GetSection("IzipayFinanzasInfo:id_estab").Value.ToString(),
                        bin_acq = config.GetSection("IzipayFinanzasInfo:bin_acq").Value.ToString(),
                        tipo_medio_pago = "EF"
                    }

                };
                var response = api.PagoDirecto(model_api, _logger, model.id_trx_hub).Result;

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
                try { trx.ti_respuesta_api = (response.dt_fin - response.dt_inicio); } catch (Exception ti) { }

                if (response.rc == "00")
                {
                    trx.vc_id_ref_trx = response.id_pago.ToString();
                    trx.vc_cod_autorizacion = response.codigo_autorizacion.ToString();
                    trx.nu_saldo_izipay = Convert.ToDecimal(response.nuevo_saldo);

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

                        UtilSql.iIns(cmd, trx);
                        cmd.ExecuteNonQuery();

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran_sql.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                            mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                            return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                        }
                        trx.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();
                    }


                    using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", con_sql, tran_sql))
                    {
                        cmd_upd.CommandType = CommandType.StoredProcedure;
                        cmd_upd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx_app);
                        cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", trx.vc_id_ref_trx);
                        cmd_upd.Parameters.AddWithValue("@vc_cod_autorizacion", trx.vc_cod_autorizacion);
                        cmd_upd.Parameters.AddWithValue("@nu_saldo_izipay", trx.nu_saldo_izipay);
                        UtilSql.iUpd(cmd_upd, trx);
                        cmd_upd.ExecuteNonQuery();
                        if (cmd_upd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran_sql.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText());
                            mensaje_error = cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText();
                            return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
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
                            return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
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
                    string vc_cod_error_hub = "99";
                    string vc_desc_error_hub = "Error desconocido.";

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

                    if (response.rc == null)
                        tm.vc_cod_error = "";
                    else
                        tm.vc_cod_error = response.rc;

                    if (response.descripcion == null)
                        tm.vc_desc_error = "";
                    else
                        tm.vc_desc_error = response.descripcion;

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
                        return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                    }

                    tran_sql_error.Commit();
                    con_sql.Close();
                    mensaje_error = tm.vc_desc_error;
                    ins_bd = false;

                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, response.rc, ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);

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

                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
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
                if (transaccion_completada == false && model.id_trx_hub != "")
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


        public object ConsultaRecibos(string conexion, DeudaModel.Deuda_Input model, IHttpClientFactory client_factory)
        {
            GlobalService global_service = new GlobalService();
            List<DeudaModel> ls_deuda = new List<DeudaModel>();
            try
            {
                SqlConnection con_sql = new SqlConnection(conexion);
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                Token token = new Token()
                {
                    id_estab = config.GetSection("IzipayFinanzasInfo:id_estab").Value.ToString(),
                    id_term = config.GetSection("IzipayFinanzasInfo:id_term").Value.ToString(),
                    secreto = config.GetSection("IzipayFinanzasInfo:secreto").Value.ToString()
                };
                IziPayFinanzasApi client = new IziPayFinanzasApi(hub_convenio, client_factory, token);

                object model_api = new object();
                model_api = new
                {
                    secuencia = new Random().Next(100000, 999999),
                    fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                    consulta = new
                    {
                        numero_cliente = model.numero_servicio.ToString(),
                        id_servicio = model.vc_cod_convenio.ToString(),
                        importe = "0",
                        cod_moneda = "604",
                        tipo_term = config.GetSection("IzipayFinanzasInfo:tipo_term").Value.ToString(),
                        tipo_cargo = "B",
                        merchant_type = config.GetSection("IzipayFinanzasInfo:merchant_type").Value.ToString(),
                        ubicacion = model.direccion + model.nombre_ciudad + model.codigo_provincia + model.codigo_pais,
                        id_term = config.GetSection("IzipayFinanzasInfo:id_term").Value.ToString(),
                        id_estab = config.GetSection("IzipayFinanzasInfo:id_estab").Value.ToString(),
                        bin_acq = config.GetSection("IzipayFinanzasInfo:bin_acq").Value.ToString(),
                        tipo_medio_pago = "EF"
                    }
                };

                var result = client.ConsultarRecibos(model_api, null).Result;


                if (result.rc != "00")
                {
                    string vc_cod_error_hub = "99";
                    string vc_desc_error_hub = "Error desconocido.";

                    string codigo = result.rc;
                    string mensaje = result.descripcion;

                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, result.rc, ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);
                }
                foreach (var e_datos in result.recibos)
                {
                    DeudaModel e_deuda = new DeudaModel();

                    e_deuda.fecha_vencimento = e_datos.fecha_recibo.Substring(0, 4) + "-" + e_datos.fecha_recibo.Substring(4, 2) + "-" + e_datos.fecha_recibo.Substring(6, 2);
                    e_deuda.cliente = result.nombre_cliente == "NO DISPONIBLE" ? "" : result.nombre_cliente;

                    e_deuda.monto_documento = Decimal.Round(Convert.ToDecimal((e_datos.saldo_recibo ?? 0).ToString("N")), 2);

                    e_deuda.comision_cliente = Decimal.Round(Convert.ToDecimal((e_datos.importe_com_cli ?? 0).ToString("N")), 2) + Decimal.Round(Convert.ToDecimal(model.comision_usuario_final), 2);

                    e_deuda.monto_deuda = Decimal.Round(Convert.ToDecimal((e_datos.total_a_pagar ?? 0).ToString("N")), 2) + Decimal.Round(Convert.ToDecimal(model.comision_usuario_final), 2);
                    e_deuda.numero_documento = e_datos.numero_recibo;
                    e_deuda.moneda = e_datos.cod_moneda;

                    string monedaISO = "";
                    switch (e_deuda.moneda)
                    {
                        case "604":
                            monedaISO = "PEN";
                            break;
                        case "840":
                            monedaISO = "USD";
                            break;
                        default:
                            monedaISO = "";
                            break;
                    }
                    RegionInfo region = CultureInfo
                     .GetCultures(CultureTypes.SpecificCultures)
                     .Select(ct => new RegionInfo(ct.LCID))
                     .Where(ri => ri.ISOCurrencySymbol == monedaISO).FirstOrDefault();

                    e_deuda.simbolo_moneda = region != null ? region.CurrencySymbol : "";

                    e_deuda.fecha_factura = e_datos.fecha_recibo.Substring(0, 4) + "-" + e_datos.fecha_recibo.Substring(4, 2) + "-" + e_datos.fecha_recibo.Substring(6, 2);

                    ls_deuda.Add(e_deuda);
                }
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
            }
            return ls_deuda;

        }

        public object RealizarPago(string conexion, PagoModel.Pago_Input model, IHttpClientFactory client_factory)
        {
            bool ins_bd = false;
            bool saldo_comprometido = false;
            bool transaccion_completada = false;
            string id_trans_global = "";

            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;

            string mensaje_error = "";
            GlobalService global_service = new GlobalService();

            try
            {
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
                    mensaje_error = cmd_saldo.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                }
                saldo_comprometido = true;

                con_sql.Close();

                DeudaModel.Deuda_Input Deuda_Model = new DeudaModel.Deuda_Input();

                Deuda_Model.numero_servicio = model.numero_servicio;
                Deuda_Model.vc_cod_convenio = model.vc_cod_convenio;

                Token token = new Token()
                {
                    id_estab = config.GetSection("IzipayFinanzasInfo:id_estab").Value.ToString(),
                    id_term = config.GetSection("IzipayFinanzasInfo:id_term").Value.ToString(),
                    secreto = config.GetSection("IzipayFinanzasInfo:secreto").Value.ToString()
                };
                IziPayFinanzasApi client = new IziPayFinanzasApi(hub_convenio, client_factory, token);

                object model_api = new object();
                model_api = new
                {
                    secuencia = new Random().Next(100000, 999999),
                    fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                    consulta = new
                    {
                        numero_cliente = model.numero_servicio.ToString(),
                        id_servicio = model.vc_cod_convenio.ToString(),
                        importe = "0",
                        cod_moneda = "604",
                        tipo_term = config.GetSection("IzipayFinanzasInfo:tipo_term").Value.ToString(),
                        tipo_cargo = "B",
                        merchant_type = config.GetSection("IzipayFinanzasInfo:merchant_type").Value.ToString(),
                        ubicacion = model.direccion + model.nombre_ciudad + model.codigo_provincia + model.codigo_pais,
                        id_term = config.GetSection("IzipayFinanzasInfo:id_term").Value.ToString(),
                        id_estab = config.GetSection("IzipayFinanzasInfo:id_estab").Value.ToString(),
                        bin_acq = config.GetSection("IzipayFinanzasInfo:bin_acq").Value.ToString(),
                        tipo_medio_pago = "EF"
                    }
                };

                var result = client.ConsultarRecibos(model_api, _logger, model.id_trx_hub).Result;


                if (result.rc != "00")
                {
                    string vc_cod_error_hub = "99";
                    string vc_desc_error_hub = "Error desconocido.";

                    string codigo = result.rc;
                    string mensaje = result.descripcion;

                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, result.rc, ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    mensaje_error = mensaje;

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);
                }

                // 4) Enviar Solicitud al proveedor
                PagoModel e_pago = new PagoModel();
                foreach (var e_datos in result.recibos)
                {

                    if (e_datos.numero_recibo == model.numero_documento)
                    {

                        IziPayFinanzasApi client_pago = new IziPayFinanzasApi(hub_convenio, client_factory, token);

                        int secuencia_pago = new Random().Next(100000, 999999);
                        string fecha_hora_pago = DateTime.Now.ToString("yyyyMMddHHMMss");

                        model_api = new
                        {
                            secuencia = secuencia_pago,
                            fecha_hora = fecha_hora_pago,
                            pago = new
                            {
                                numero_cliente = model.numero_servicio.ToString(),
                                numero_recibo = model.numero_documento.ToString(),
                                fecha_recibo = e_datos.fecha_recibo,
                                id_servicio = model.vc_cod_convenio.ToString(),
                                importe = (model.importe_pago.ToDecimal() - model.comision_usuario_final.ToDecimal()).ToString(),
                                cod_moneda = "604",
                                tipo_term = config.GetSection("IzipayFinanzasInfo:tipo_term").Value.ToString(),
                                tipo_cargo = "B",
                                merchant_type = config.GetSection("IzipayFinanzasInfo:merchant_type").Value.ToString(),
                                ubicacion = model.direccion + model.nombre_ciudad + model.codigo_provincia + model.codigo_pais,
                                id_term = config.GetSection("IzipayFinanzasInfo:id_term").Value.ToString(),
                                id_estab = config.GetSection("IzipayFinanzasInfo:id_estab").Value.ToString(),
                                bin_acq = config.GetSection("IzipayFinanzasInfo:bin_acq").Value.ToString(),
                                tipo_medio_pago = "EF"
                            }
                        };

                        var result_pago = client_pago.PagoRecibo(model_api, _logger, model.id_trx_hub).Result;

                        //Solo para provocar el extorno
                        //result_pago.timeout = true;
                        //result_pago.rc = "999";

                        TransaccionModel trx = new TransaccionModel();
                        trx.nu_id_trx = idtran;
                        trx.nu_id_trx_hub = Int64.Parse(model.id_trx_hub);
                        trx.nu_id_distribuidor = int.Parse(model.id_distribuidor);
                        trx.nu_id_comercio = int.Parse(model.id_comercio);
                        trx.dt_fecha = fechatran;
                        trx.nu_id_producto = int.Parse(model.id_servicio);
                        trx.nu_precio = Convert.ToDecimal(model.importe_pago);
                        trx.vc_tran_usua_regi = "API";
                        if (e_datos.cod_moneda == "604")
                        {
                            trx.nu_id_tipo_moneda_vta = 1; //SOLES
                        }
                        else
                        {
                            trx.nu_id_tipo_moneda_vta = 2; //DOLARES
                        }
                        trx.vc_numero_servicio = model.numero_servicio;
                        trx.vc_nro_doc_pago = model.numero_documento;
                        trx.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;
                        try { trx.ti_respuesta_api = (result_pago.dt_fin - result_pago.dt_inicio); } catch (Exception ti) { }
                        trx.vc_datos_adicionales = secuencia_pago + "|" + fecha_hora_pago;
                        trx.nu_imp_com_usuario_final = model.comision_usuario_final.ToDecimal();

                        if (result_pago.rc == "00")
                        {
                            trx.vc_id_ref_trx = result_pago.id_pago.ToString();
                            trx.vc_cod_autorizacion = result_pago.codigo_autorizacion.ToString();
                            trx.nu_saldo_izipay = Convert.ToDecimal(result_pago.nuevo_saldo);

                            //Graba BD
                            con_sql.Open();
                            tran_sql = con_sql.BeginTransaction();
                            ins_bd = true;

                            using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_servicios", con_sql, tran_sql))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx);
                                cmd.Parameters.AddWithValue("@nu_id_trx_hub", trx.nu_id_trx_hub);
                                cmd.Parameters.AddWithValue("@nu_id_distribuidor", trx.nu_id_distribuidor);
                                cmd.Parameters.AddWithValue("@nu_id_comercio", trx.nu_id_comercio);
                                cmd.Parameters.AddWithValue("@nu_id_producto", trx.nu_id_producto);
                                cmd.Parameters.AddWithValue("@vc_numero_servicio", trx.vc_numero_servicio);
                                cmd.Parameters.AddWithValue("@vc_nro_doc_pago", trx.vc_nro_doc_pago);
                                cmd.Parameters.AddWithValue("@nu_id_tipo_moneda_vta", trx.nu_id_tipo_moneda_vta);
                                cmd.Parameters.AddWithValue("@nu_precio_vta", trx.nu_precio);
                                cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", trx.vc_id_ref_trx_distribuidor);
                                cmd.Parameters.AddWithValue("@ti_respuesta_api", trx.ti_respuesta_api);
                                cmd.Parameters.AddWithValue("@vc_datos_adicionales", trx.vc_datos_adicionales);
                                cmd.Parameters.AddWithValue("@nu_imp_com_usuario_final", trx.nu_imp_com_usuario_final);

                                UtilSql.iIns(cmd, trx);
                                cmd.ExecuteNonQuery();

                                if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                                {
                                    tran_sql.Rollback();
                                    ins_bd = false;
                                    _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                                    mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                                    return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                                }
                                trx.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();

                            }

                            using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", con_sql, tran_sql))
                            {
                                cmd_upd.CommandType = CommandType.StoredProcedure;
                                cmd_upd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx_app);
                                cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", trx.vc_id_ref_trx);
                                cmd_upd.Parameters.AddWithValue("@vc_cod_autorizacion", trx.vc_cod_autorizacion);
                                cmd_upd.Parameters.AddWithValue("@nu_saldo_izipay", trx.nu_saldo_izipay);
                                UtilSql.iUpd(cmd_upd, trx);
                                cmd_upd.ExecuteNonQuery();
                                if (cmd_upd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                                {
                                    tran_sql.Rollback();
                                    ins_bd = false;
                                    _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText());
                                    mensaje_error = cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText();
                                    return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
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
                                    return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
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
                                mensaje = "Se realizó el pago correctamente.",
                                nro_transaccion = id_trans_global
                            };

                            return info;
                        }
                        else
                        {
                            string vc_cod_error_hub = "99";
                            string vc_desc_error_hub = "Error desconocido.";

                            try { trx.vc_id_ref_trx = result_pago.id_pago.ToString(); } catch (Exception) { }
                            try { trx.vc_cod_autorizacion = result_pago.codigo_autorizacion.ToString(); } catch (Exception) { }
                            try { trx.nu_saldo_izipay = Convert.ToDecimal(result_pago.nuevo_saldo); } catch (Exception) { }

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

                            if (result_pago.rc == null)
                                tm.vc_cod_error = "";
                            else
                                tm.vc_cod_error = result_pago.rc;

                            if (result_pago.descripcion == null)
                                tm.vc_desc_error = "";
                            else
                                tm.vc_desc_error = result_pago.descripcion;

                            tm.vc_desc_tipo_error = "CONVENIO";

                            SqlTransaction tran_sql_error = null;
                            con_sql.Open();

                            tran_sql_error = con_sql.BeginTransaction();

                            cmd = global_service.insTransaccionError(con_sql, tran_sql_error, tm);

                            if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                            {
                                tran_sql_error.Rollback();
                                _logger.Error("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                                mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                            }

                            tran_sql_error.Commit();
                            con_sql.Close();
                            mensaje_error = tm.vc_desc_error;
                            ins_bd = false;

                            con_sql.Open();
                            global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, result_pago.rc, ref vc_cod_error_hub, ref vc_desc_error_hub);
                            con_sql.Close();

                            _logger.Error("idtrx: " + model.id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);

                            return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);
                        }
                    }

                }

                mensaje_error = "No se encontro deuda con el número de documento " + model.numero_documento;
                return UtilSql.sOutPutTransaccion("69", mensaje_error);
            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }

                _logger.Error("idtrx: " + model.id_trx_hub + " / " + ex.Message);
                mensaje_error = ex.Message;

                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
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
                if (transaccion_completada == false && model.id_trx_hub != "")
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
        private List<DistribuidorModel> sel_distribuidor(SqlConnection cn, decimal? nu_id_distribuidor)
        {
            List<DistribuidorModel> ls_model = new List<DistribuidorModel>();
            DistribuidorModel model = new DistribuidorModel();
            using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 3;
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", nu_id_distribuidor);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    model = new DistribuidorModel();
                    if (UtilSql.Ec(dr, "nu_id_distribuidor"))
                        model.nu_id_distribuidor = Convert.ToInt32(dr["nu_id_distribuidor"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_distribuidor"))
                        model.vc_cod_distribuidor = dr["vc_cod_distribuidor"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_distribuidor"))
                        model.vc_desc_distribuidor = dr["vc_desc_distribuidor"].ToString();
                    if (UtilSql.Ec(dr, "vc_nombre_contacto"))
                        model.vc_nombre_contacto = dr["vc_nombre_contacto"].ToString();
                    if (UtilSql.Ec(dr, "vc_email_contacto"))
                        model.vc_email_contacto = dr["vc_email_contacto"].ToString();
                    if (UtilSql.Ec(dr, "vc_celular_contacto"))
                        model.vc_celular_contacto = dr["vc_celular_contacto"].ToString();
                    if (UtilSql.Ec(dr, "bi_izipay"))
                        model.bi_izipay = dr["bi_izipay"].ToBool();
                    if (UtilSql.Ec(dr, "nu_id_comercio"))
                        model.nu_id_comercio = Convert.ToInt32(dr["nu_id_comercio"].ToString());

                    ls_model.Add(model);
                }
            }
            return ls_model;
        }
        private List<DistribuidorProductoModel> sel_distribuidor_producto(SqlConnection cn, decimal? nu_id_distribuidor, decimal? nu_id_producto)
        {
            List<DistribuidorProductoModel> ls_model = new List<DistribuidorProductoModel>();
            DistribuidorProductoModel model = new DistribuidorProductoModel();
            using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor_producto", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 5;
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_producto", nu_id_producto);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    model = new DistribuidorProductoModel();
                    if (UtilSql.Ec(dr, "nu_id_distribuidor"))
                        model.nu_id_distribuidor = Convert.ToInt32(dr["nu_id_distribuidor"].ToString());
                    if (UtilSql.Ec(dr, "nu_id_producto"))
                        model.nu_id_producto = Convert.ToInt32(dr["nu_id_producto"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_producto"))
                        model.vc_cod_producto = dr["vc_cod_producto"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_producto"))
                        model.vc_desc_producto = dr["vc_desc_producto"].ToString();
                    if (UtilSql.Ec(dr, "ch_tipo_comision"))
                        model.ch_tipo_comision = dr["ch_tipo_comision"].ToString();
                    if (UtilSql.Ec(dr, "nu_valor_comision"))
                        model.nu_valor_comision = Convert.ToDecimal(dr["nu_valor_comision"].ToString());
                    if (UtilSql.Ec(dr, "bi_envio_email"))
                        model.bi_envio_email = dr["bi_envio_email"].ToBool();
                    if (UtilSql.Ec(dr, "bi_envio_sms"))
                        model.bi_envio_sms = dr["bi_envio_sms"].ToBool();
                    if (UtilSql.Ec(dr, "nu_id_regla_izipay"))
                        model.nu_id_regla_izipay = dr["nu_id_regla_izipay"].ToInt32();
                    if (UtilSql.Ec(dr, "nu_id_comercio"))
                        model.nu_id_comercio = Convert.ToInt32(dr["nu_id_comercio"].ToString());

                    ls_model.Add(model);
                }
            }
            return ls_model;
        }
        public static SqlCommand Upd_Regla(SqlConnection cn, DistribuidorProductoModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_upd_distribuidor_producto_regla_izipay", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@nu_id_regla_izipay", model.nu_id_regla_izipay);
                UtilSql.iUpd(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        public static SqlCommand Upd_Contraseña(SqlConnection cn, DistribuidorModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_upd_distribuidor_contraseña_izipay", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@vc_contraseña", model.vc_contraseña);
                UtilSql.iUpd(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
    }
}
