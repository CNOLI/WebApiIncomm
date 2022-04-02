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

namespace wa_api_incomm.Services
{
    public class IzipayService : IIzipayService
    {
        public int nu_id_convenio = 5;

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();


        private readonly Serilog.ILogger _logger;

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
            try
            {
                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                //Obtener la lista de distribuidores activos
                ls_distribuidor = sel_distribuidor(con_sql, model.id_distribuidor);

                foreach (var item in ls_distribuidor)
                {
                    if (item.bi_izipay == false && item.nu_id_comercio != null )
                    {

                        if (String.IsNullOrEmpty(item.vc_nombre_contacto))
                        {
                            return UtilSql.sOutPutTransaccion("99", "El nombre de contacto es obligatorio.");
                        }
                        if (String.IsNullOrEmpty(item.vc_email_contacto))
                        {
                            return UtilSql.sOutPutTransaccion("99", "El email de contacto es obligatorio.");
                        }
                        if (String.IsNullOrEmpty(item.vc_celular_contacto))
                        {
                            return UtilSql.sOutPutTransaccion("99", "El celular de contacto es obligatorio.");
                        }

                        IzipayApi api = new IzipayApi();
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
                            return UtilSql.sOutPutTransaccion("99", "Error al crear/actualizar comercio en IZIPAY.");
                        }
                        else if (result.rc != "00")
                        {
                            return UtilSql.sOutPutTransaccion("99", result.descripcion);
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
                            return UtilSql.sOutPutTransaccion("99", "Error al crear terminal " + item.vc_celular_contacto + ".");
                        }
                        else if (result.rc != "00" && result.rc != "94")
                        {
                            return UtilSql.sOutPutTransaccion("99", result.descripcion);
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
                return UtilSql.sOutPutTransaccion("500", ex.Message);
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
            try
            {
                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                //Obtener la lista de productos izipay del distribuidor activos
                ls_distribuidor_producto = sel_distribuidor_producto(con_sql, model.id_distribuidor, model.id_producto);

                foreach (var item in ls_distribuidor_producto)
                {
                    if (item.nu_id_regla_izipay == null)
                    {

                        IzipayApi api = new IzipayApi();
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
                            return UtilSql.sOutPutTransaccion("99", "Error al registra la comisión.");
                        }
                        else if (result.rc != "00")
                        {
                            return UtilSql.sOutPutTransaccion("99", result.descripcion);
                        }

                        item.nu_id_regla_izipay = result.id_regla.ToInt32();
                        Upd_Regla(con_sql, item);

                    }
                    else
                    {

                        IzipayApi api = new IzipayApi();

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
                            return UtilSql.sOutPutTransaccion("99", "Error al actualizar regla en Izipay.");
                        }
                        else if (result.rc != "00")
                        {
                            return UtilSql.sOutPutTransaccion("99", result.descripcion);
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
                return UtilSql.sOutPutTransaccion("500", ex.Message);
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }


        }

        public object RealizarRecarga(string conexion, Pago_Directo_Input model, IHttpClientFactory client_factory)
        {
            bool ins_bd = false;
            string id_trans_global = "";
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string codigo_error = "";
            string mensaje_error = "";

            IzipayModel model_sql = new IzipayModel();
            try
            {
                GlobalService global_service = new GlobalService();

                _logger.Information("idtrx: " + model.id_trx_hub + " / " + "Inicio de transaccion");

                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                if (!Regex.Match(model.numero_servicio, @"(^[0-9]+$)").Success)
                {
                    _logger.Error("idtrx: " + model.id_trx_hub + " / " + "El id del producto " + model.id_producto + " debe ser numerico");
                    return UtilSql.sOutPutTransaccion("99", "El número del cliente debe ser numerico");
                }

                if (!Regex.Match(model.importe_recarga, @"^[0-9]+(\.[0-9]{1,2})?$").Success)
                {
                    _logger.Error("idtrx: " + model.id_trx_hub + " / " + "El id del producto " + model.id_producto + " debe ser numerico");
                    return UtilSql.sOutPutTransaccion("99", "El importe de recarga debe ser numerico");
                }

                if (!Regex.Match(model.id_producto, @"(^[0-9]+$)").Success)
                {
                    _logger.Error("idtrx: " + model.id_trx_hub + " / " + "El id del producto " + model.id_producto + " debe ser numerico");
                    return UtilSql.sOutPutTransaccion("04", "El id del producto debe ser numerico");
                }

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    _logger.Error("idtrx: " + model.id_trx_hub + " / " + "El código de distribuidor " + model.codigo_distribuidor + " no existe");
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
                    _logger.Error("idtrx: " + model.id_trx_hub + " / " + "El producto " + model.id_producto + " no existe");
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

                    UtilSql.iIns(cmd, model_sql);
                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                    {
                        tran_sql.Rollback();
                        _logger.Error("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        return UtilSql.sOutPutTransaccion("99", cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    }
                    model_sql.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();
                              
                    Token token = new Token()
                    {
                        id_estab = distribuidor.nu_id_comercio.ToString(),
                        id_term = distribuidor.vc_celular_contacto,
                        secreto = "IZI" + distribuidor.vc_cod_distribuidor
                    };
                    IziPayFinanzasApi api = new IziPayFinanzasApi(client_factory, token);

                    object model_api = new object();
                    model_api = new
                    {
                        secuencia = new Random().Next(100000, 999999),
                        fecha_hora = DateTime.Now.ToString("yyyyMMddHHMMss"),
                        pago = new
                        {
                            numero_cliente = model.numero_servicio.ToString(),
                            id_servicio = producto.vc_cod_producto.ToString(),
                            importe = model.importe_recarga.ToString(),
                            cod_moneda = "604",
                            tipo_term = config.GetSection("IzipayFinanzasInfo:tipo_term").Value.ToString(),
                            tipo_cargo = "B",
                            merchant_type = config.GetSection("IzipayFinanzasInfo:merchant_type").Value.ToString(),
                            ubicacion = model.direccion + model.nombre_ciudad + model.codigo_provincia + model.codigo_pais,
                            id_term = distribuidor.vc_celular_contacto,
                            id_estab = distribuidor.nu_id_comercio.ToString(),
                            bin_acq = config.GetSection("IzipayFinanzasInfo:bin_acq").Value.ToString(),
                            tipo_medio_pago = "EF"
                        }

                    };
                    var response = api.PagoDirecto(model_api, _logger, model.id_trx_hub).Result;

                    if (response.rc == "00")
                    {
                        model_sql.vc_id_ref_trx = response.id_pago.ToString();
                        model_sql.vc_cod_autorizacion = response.codigo_autorizacion.ToString();
                        model_sql.nu_saldo_izipay = Convert.ToDecimal(response.nuevo_saldo);

                        using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", con_sql, tran_sql))
                        {
                            cmd_upd.CommandType = CommandType.StoredProcedure;
                            cmd_upd.Parameters.AddWithValue("@nu_id_trx", model_sql.nu_id_trx_app);
                            cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", model_sql.vc_id_ref_trx);
                            cmd_upd.Parameters.AddWithValue("@vc_cod_autorizacion", model_sql.vc_cod_autorizacion);
                            cmd_upd.Parameters.AddWithValue("@nu_saldo_izipay", model_sql.nu_saldo_izipay);
                            UtilSql.iUpd(cmd_upd, model_sql);
                            cmd_upd.ExecuteNonQuery();
                            if (cmd_upd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                            {
                                tran_sql.Rollback();
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
                        _logger.Information("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());

                        object info = new object();


                        info = new
                        {
                            codigo = "00",
                            mensaje = "Operación exitosa, la recarga se hará efectiva en unos segundos.",
                            nro_transaccion = id_trans_global
                        };
                        con_sql.Close();

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
                            _logger.Error("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                            return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                        }

                        tran_sql_error.Commit();
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
