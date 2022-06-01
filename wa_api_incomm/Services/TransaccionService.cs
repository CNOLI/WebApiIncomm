using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using wa_api_incomm.Smtp;

namespace wa_api_incomm.Services
{
    public class TransaccionService : ITransaccionService
    {
        private readonly Serilog.ILogger _logger;
        private readonly Send _send;
        Models.Hub.ConvenioModel hub_convenio;
        public TransaccionService(Serilog.ILogger logger, Send send)
        {
            _send = send;
            _logger = logger;
        }
        public object Confirmar(string conexion, TransaccionModel.Transaccion_Input_Confirmar model)
        {
            bool ins_bd = false;
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlTransaction tran_sql_error = null;
            SqlCommand cmd = null;
            GlobalService global_service = new GlobalService();

            try
            {
                con_sql = new SqlConnection(conexion);
                               
                con_sql.Open();

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("05", "El código de distribuidor no existe");
                }
                ComercioModel comercio = new ComercioModel();
                comercio.vc_cod_comercio = model.codigo_comercio;
                comercio = global_service.get_comercio_busqueda(con_sql, model.codigo_comercio, distribuidor.nu_id_distribuidor);

                if (comercio.nu_id_comercio <= 0)
                {
                    return UtilSql.sOutPutTransaccion("07", "El código de comercio no existe");
                }

                TransaccionModel model_sql = new TransaccionModel();
                model_sql.nu_id_trx = Convert.ToInt64(model.nro_transaccion);
                model_sql.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                model_sql.nu_id_comercio = comercio.nu_id_comercio;

                model_sql = global_service.get_transaccion(con_sql, model_sql);

                if (model_sql.nu_id_trx <= 0)
                {
                    return UtilSql.sOutPutTransaccion("99", "No se encontró transacción con los datos enviados.");
                }

                var cmd_upd_confirmar = global_service.upd_confirmar(con_sql, model_sql);

                if (cmd_upd_confirmar.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                {
                    _logger.Information("idtrx: " + model_sql.nu_id_trx_hub.ToString() + " / " + cmd_upd_confirmar.Parameters["@tx_tran_mnsg"].Value.ToString());
                    return UtilSql.sOutPutTransaccion("99", cmd_upd_confirmar.Parameters["@tx_tran_mnsg"].Value.ToString());
                }
                //Obtener Producto
                ProductoModel producto = new ProductoModel();
                producto.nu_id_producto = Convert.ToInt32(model_sql.nu_id_producto);
                producto.nu_id_distribuidor = distribuidor.nu_id_distribuidor;

                producto = global_service.get_producto(con_sql, producto);
                
                hub_convenio = global_service.get_convenio(con_sql, producto.nu_id_convenio);

                AESApi _aesapi = new AESApi(hub_convenio.vc_url_api_aes);
                DatosPinModel dpm = new DatosPinModel();
                dpm.key = hub_convenio.vc_clave_aes;
                dpm.pin = model_sql.vc_nro_pin;
                model_sql.vc_nro_pin_desencriptado = _aesapi.GetPin(dpm).Result.pin;

                if (producto.bi_envio_sms && model.envio_sms && (distribuidor.nu_seg_encolamiento == 0 || model_sql.bi_informado == true))
                {
                    bool bi_informado = false;

                    if (hub_convenio.nu_id_convenio == 1)
                    {
                        //envio de mensaje
                        var mensajefrom = "POSA";
                        var mensajetxt = _send.GetBodyIncommSMS(model_sql.vc_nro_pin_desencriptado, hub_convenio.vc_url_web_terminos, model_sql.vc_email_sol);
                        _send.SendMessage(mensajefrom, mensajetxt, model_sql.vc_telefono_sol, hub_convenio.vc_aws_access_key_id, hub_convenio.vc_aws_secrect_access_key, model_sql.nu_id_trx.ToString(), model_sql.nu_id_trx_hub.ToString(), con_sql, ref bi_informado);
                    }

                    if (bi_informado)
                    {
                        var cmd_upd_informar_sms = global_service.upd_informar(con_sql, model_sql);
                    }
                }

                if (producto.bi_envio_email && model.envio_email && (distribuidor.nu_seg_encolamiento == 0 || model_sql.bi_informado == true))
                {
                    bool bi_informado = false;
                    //envio de correo

                    if (hub_convenio.nu_id_convenio == 1)
                    {
                        var body = _send.GetBodyIncomm(hub_convenio.vc_desc_empresa, model_sql.vc_desc_categoria, model_sql.vc_desc_producto, hub_convenio.vc_color_header_email, hub_convenio.vc_color_body_email, model_sql.vc_nro_pin_desencriptado, model_sql.vc_cod_comercio, model_sql.vc_fecha_reg, model_sql.vc_id_ref_trx, model_sql.vc_cod_autorizacion, (model_sql.nu_precio_vta ?? 0).ToString("0.000"), hub_convenio.vc_url_web_terminos, distribuidor.bi_valor_pin ?? false, model_sql.vc_telefono_sol);
                        var titulo = "Confirmación de transacción #" + model_sql.vc_id_ref_trx;
                        _send.Email(model_sql.nu_id_trx.ToString(), model_sql.nu_id_trx_hub.ToString(), model_sql.vc_email_sol, titulo, body, hub_convenio.vc_email_envio, hub_convenio.vc_password_email, hub_convenio.vc_smtp_email, hub_convenio.nu_puerto_smtp_email ?? 587, hub_convenio.bi_ssl_email ?? true, hub_convenio.vc_desc_empresa, con_sql, ref bi_informado);
                    }
                    if (bi_informado)
                    {
                        var cmd_upd_informar_email = global_service.upd_informar(con_sql, model_sql);
                    }
                }

                con_sql.Close();

                object info = new object();


                info = new
                {
                    codigo = "00",
                    mensaje = "Transacción confirmada correctamente.",
                    nro_transaccion = model_sql.nu_id_trx
                };

                _logger.Information("idtrx: " + model_sql.nu_id_trx_hub.ToString() + " / " + "Transacción confirmada correctamente.");

                return info;

            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }

                _logger.Error("idtrx_app: " + model.nro_transaccion + " / " + ex.Message);

                return UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }

        public object Informar(string conexion, TransaccionModel.Transaccion_Input_Confirmar model)
        {

            bool ins_bd = false;
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            con_sql = new SqlConnection(conexion);
            try
            {
                GlobalService global_service = new GlobalService();

                con_sql.Open();

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("05", "El código de distribuidor no existe");
                }
                ComercioModel comercio = new ComercioModel();
                comercio.vc_cod_comercio = model.codigo_comercio;
                comercio = global_service.get_comercio_busqueda(con_sql, model.codigo_comercio, distribuidor.nu_id_distribuidor);

                if (comercio.nu_id_comercio <= 0)
                {
                    return UtilSql.sOutPutTransaccion("07", "El código de comercio no existe");
                }

                TransaccionModel model_sql = new TransaccionModel();
                model_sql.nu_id_trx = Convert.ToInt64(model.nro_transaccion);
                model_sql.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                model_sql.nu_id_comercio = comercio.nu_id_comercio;

                model_sql = global_service.get_transaccion(con_sql, model_sql);

                if (model_sql.nu_id_trx <= 0)
                {
                    return UtilSql.sOutPutTransaccion("99", "No se encontró transacción con los datos enviados.");
                }

                hub_convenio = global_service.get_convenio(con_sql, model_sql.nu_id_convenio);
                
                if (hub_convenio.nu_id_convenio == 1)
                {
                    AESApi _aesapi = new AESApi(hub_convenio.vc_url_api_aes);
                    DatosPinModel dpm = new DatosPinModel();
                    dpm.key = hub_convenio.vc_clave_aes;
                    dpm.pin = model_sql.vc_nro_pin;
                    model_sql.vc_nro_pin_desencriptado = _aesapi.GetPin(dpm).Result.pin;
                }

                bool bi_informado = false;

                if (model.envio_sms)
                {
                    if (hub_convenio.nu_id_convenio == 1)
                    {
                        //envio de mensaje
                        var mensajefrom = "POSA";
                        var mensajetxt = _send.GetBodyIncommSMS(model_sql.vc_nro_pin_desencriptado, hub_convenio.vc_url_web_terminos, model_sql.vc_email_sol);
                        _send.SendMessage(mensajefrom, mensajetxt, model_sql.vc_telefono_sol, hub_convenio.vc_aws_access_key_id, hub_convenio.vc_aws_secrect_access_key, model_sql.nu_id_trx.ToString(), model_sql.nu_id_trx_hub.ToString(), con_sql, ref bi_informado);
                    }

                    if (bi_informado)
                    {
                        var cmd_upd_informar_sms = global_service.upd_informar(con_sql, model_sql);
                    }
                }

                //envio de correo

                if (model.envio_email)
                {
                    if (hub_convenio.nu_id_convenio == 1)
                    {
                        var body = _send.GetBodyIncomm(hub_convenio.vc_desc_empresa, model_sql.vc_desc_categoria, model_sql.vc_desc_producto, hub_convenio.vc_color_header_email, hub_convenio.vc_color_body_email, model_sql.vc_nro_pin_desencriptado, model_sql.vc_cod_comercio, model_sql.vc_fecha_reg, model_sql.vc_id_ref_trx, model_sql.vc_cod_autorizacion, (model_sql.nu_precio_vta ?? 0).ToString("0.000"), hub_convenio.vc_url_web_terminos, distribuidor.bi_valor_pin ?? false, model_sql.vc_telefono_sol);
                        var titulo = "Confirmación de transacción #" + model_sql.vc_id_ref_trx;
                        _send.Email(model_sql.nu_id_trx.ToString(), model_sql.nu_id_trx_hub.ToString(), model_sql.vc_email_sol, titulo, body, hub_convenio.vc_email_envio, hub_convenio.vc_password_email, hub_convenio.vc_smtp_email, hub_convenio.nu_puerto_smtp_email ?? 587, hub_convenio.bi_ssl_email ?? true, hub_convenio.vc_desc_empresa, con_sql, ref bi_informado);
                    }
                    if (bi_informado)
                    {
                        var cmd_upd_informar_email = global_service.upd_informar(con_sql, model_sql);
                    }
                }

                con_sql.Close();

                object info = new object();

                if (bi_informado)
                {
                    info = new
                    {
                        codigo = "00",
                        mensaje = "Transacción informada correctamente.",
                        nro_transaccion = model_sql.nu_id_trx
                    };
                    _logger.Information("idtrx: " + model_sql.nu_id_trx_hub.ToString() + " / " + "Transacción informada correctamente. SMS: " + model_sql.vc_telefono_sol + " - EMAIL: " + model_sql.vc_email_sol);

                }
                else
                {
                    info = new
                    {
                        codigo = "99",
                        mensaje = "Transacción no ha sido informada correctamente.",
                        nro_transaccion = model_sql.nu_id_trx
                    };
                    _logger.Information("idtrx: " + model_sql.nu_id_trx_hub.ToString() + " / " + "Transacción no ha sido informada correctamente.");
                }
                return info;

            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }

                _logger.Error("idtrx_app: " + model.nro_transaccion + " / " + ex.Message);

                return UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }
    }
}
