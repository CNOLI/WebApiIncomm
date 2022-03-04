using Amazon.Runtime.CredentialManagement;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Help;
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
        public TransaccionService(Serilog.ILogger logger,Send send)
        {
            _send = send;
            _logger = logger;
        }
        public object execute_trans(string conexion, InputTransModel input)
        {

            bool ins_bd = false;
            string id_trans_global = "";
            string id_incomm_global = "";
            string id_trx_incomm_global = "";
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            try
            {

                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                TrxHubModel trx = new TrxHubModel();
                trx.codigo_distribuidor = input.codigo_distribuidor;
                trx.codigo_comercio = input.codigo_comercio;
                trx.nombre_comercio = input.nombre_comercio;
                trx.nro_telefono = input.nro_telefono;
                trx.email = input.email;
                trx.id_producto = input.id_producto;

                cmd = insTrxhub(con_sql, trx);
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    tran_sql.Rollback();
                    _logger.Error(cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                }

                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 2)
                {
                    tran_sql.Commit();
                    _logger.Error(cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                }


                var id_trx_hub = cmd.Parameters["@nu_tran_pkey"].Value.ToString();
                id_trx_incomm_global = id_trx_hub;

                con_sql.Close();


                _logger.Information("idtrx: " + id_trx_hub + " / " + "Inicio de transaccion");

                bool bi_envio_sms_firts = true;
                bool bi_envio_email_firts = true;

                if (input.nro_telefono == "")
                {
                    bi_envio_sms_firts = false;
                }

                if (input.email == "")
                {
                    bi_envio_email_firts = false;
                }


                if (bi_envio_sms_firts)
                {
                    if (!new PhoneAttribute().IsValid(input.nro_telefono))
                    {
                        _logger.Error("idtrx: " + id_trx_hub + " / " + "El número de telefono " + input.nro_telefono + " es incorrecto");
                        return UtilSql.sOutPutTransaccion("02", "El número de telefono es incorrecto");
                    }
                }

                if (bi_envio_email_firts)
                {
                    if (!new EmailAddressAttribute().IsValid(input.email))
                    {
                        _logger.Error("idtrx: " + id_trx_hub + " / " + "El email " + input.email + " es incorrecto");
                        return UtilSql.sOutPutTransaccion("03", "El email es incorrecto");
                    }
                }


                if (!Regex.Match(input.id_producto, @"(^[0-9]+$)").Success)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El id del producto " + input.id_producto + " debe ser numerico");
                    return UtilSql.sOutPutTransaccion("04", "El id del producto debe ser numerico");
                }


                con_sql.Open();
                DistribuidorModel distribuidor = get_distribuidor(con_sql, input.codigo_distribuidor);
                
                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El código de distribuidor " + input.codigo_distribuidor + " no existe");
                    return UtilSql.sOutPutTransaccion("05", "El código de distribuidor no existe");
                }

                ComercioModel comercio =  get_comercio(con_sql, input.codigo_comercio, input.nombre_comercio, distribuidor.nu_id_distribuidor);

                ProductoModel producto = get_producto(con_sql, Convert.ToInt32(input.id_producto),distribuidor.nu_id_distribuidor);

                if (producto.nu_id_producto <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El producto " + input.id_producto + " no existe");
                    return UtilSql.sOutPutTransaccion("06", "El producto no existe");
                }

                ConvenioModel convenio = get_convenio(con_sql, 1);

                var idtran = get_id_transaccion(con_sql);

                id_trans_global = idtran.ToString();

                TransaccionIncommModel tim = new TransaccionIncommModel();
                tim.correlationId = idtran.ToString();

                var fechatran = DateTime.Now;

                tim.issueDate = fechatran.ToString("yyyyMMddHHmmss.mss");

                tim.issuerId = distribuidor.vc_cod_distribuidor;

                //if (input.id_opcion_telefono == "0")
                //{
                //    tim.phoneNumber = input.nro_telefono.Substring(3);
                //}
                //else if (input.id_opcion_telefono == "1")
                //{
                //    tim.phoneNumber = convenio.vc_nro_complete_incomm + input.nro_telefono.Substring(3);
                //}
                //else
                //{
                //    tim.phoneNumber = convenio.vc_nro_telefono_tran_incomm;
                //}

                //tim.phoneNumber = input.nro_telefono.Substring(3);

                if (!bi_envio_sms_firts)
                {
                    input.nro_telefono = convenio.vc_celular_def;
                }

                if (!bi_envio_email_firts)
                {
                    input.email = convenio.vc_email_def;
                }


                tim.phoneNumber = convenio.vc_nro_complete_incomm + input.nro_telefono.Substring(2);
                tim.email = input.email;



                tim.source = convenio.vc_source_body;
                tim.ip = convenio.vc_nro_ip;
                tim.amount = producto.nu_valor_facial.ToString();
                tim.issuerLogin = comercio.vc_cod_comercio;
                tim.eanCode = producto.vc_cod_ean;
                tim.zipCode = distribuidor.vc_zip_code;
                tim.channel = "API";

                IncommApi client = new IncommApi(convenio.vc_merchant_id, convenio.vc_clave_encrip_aut, convenio.vc_pos_id, convenio.vc_source_header);

                //ResultTransaccionIncomm result = new ResultTransaccionIncomm();
                //string path = @"c:\product.json";
                //using (StreamReader jsonStream = File.OpenText(path))
                //{
                //    var json = jsonStream.ReadToEnd();
                //    result = JsonConvert.DeserializeObject<ResultTransaccionIncomm>(json);
                //}

                var result = client.Transaccion(tim).Result;


                con_sql.Close();

                if (result.errorCode == "00")
                {

                    id_incomm_global = result.transactionId;

                    TransaccionModel tm = new TransaccionModel(); 
                    tm.nu_id_trx = idtran;
                    tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                    tm.nu_id_comercio = comercio.nu_id_comercio;
                    tm.dt_fecha = fechatran;
                    tm.nu_id_producto = producto.nu_id_producto;
                    tm.nu_precio = Math.Round(Convert.ToDecimal(result.amount),3);
                    tm.vc_id_ref_trx = result.transactionId;

                    tm.vc_cod_autorizacion = result.authorizationCode;
                    tm.vc_nro_pin = result.pin;

                    if (bi_envio_sms_firts)
                    {
                        tm.vc_telefono_sol = input.nro_telefono;
                    }
                    if (bi_envio_email_firts)
                    {
                        tm.vc_email_sol = input.email;
                        //tm.vc_email_sol = convenio.vc_email_envio;
                    }


                    con_sql.Open();
                    tran_sql = con_sql.BeginTransaction();
                    ins_bd = true;
                    cmd = insTransaccion(con_sql, tran_sql, tm);
                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql.Rollback();
                        _logger.Error("idtrx: " + id_trx_incomm_global + " / " + "id_transaccion: " + id_trans_global + " / " + "id_transaccion_incomm: " + id_incomm_global + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                    }

                    if(cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 2)
                    {

                        tran_sql.Commit();
                        _logger.Error("idtrx: " + id_trx_incomm_global + " / " + "id_transaccion: " + id_trans_global + " / " + "id_transaccion_incomm: " + id_incomm_global + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        return UtilSql.sOutPutTransaccion("99", "Error en base de datos");

                    }
                    


                    //desencryptar pin
                    AESApi _aesapi = new AESApi(convenio.vc_url_api_aes);
                    DatosPinModel dpm = new DatosPinModel();
                    dpm.key = convenio.vc_clave_aes; 
                    dpm.pin = result.pin;
                    var _rpin = _aesapi.GetPin(dpm).Result;



                    if (bi_envio_sms_firts)
                    {
                        if (producto.bi_envio_sms)
                        {
                            //envio de mensaje
                            var mensajetxt = "felicitaciones. tu pin esta listo para ser activado, el codigo es: " + _rpin.pin + ". terminos y condiciones en " + convenio.vc_url_web_terminos;
                            SendMessage(mensajetxt, input.nro_telefono, convenio.vc_aws_access_key_id, convenio.vc_aws_secrect_access_key, id_trans_global, con_sql);
                        }

                    }
                    if (bi_envio_email_firts)
                    {
                        if (producto.bi_envio_email)
                        {
                            //envio de correo
                            var body = _send.GetBodyFinal(convenio.vc_desc_empresa, producto.vc_desc_categoria, producto.vc_desc_producto, convenio.vc_color_header_email, convenio.vc_color_body_email, _rpin.pin, comercio.vc_cod_comercio, fechatran.ToString(), result.transactionId, result.authorizationCode, tm.nu_precio.ToString("0.000"), convenio.vc_url_web_terminos, distribuidor.bi_valor_pin);
                            var titulo = "Confirmación de transacción #" + result.transactionId;
                            _send.Email(id_trans_global, input.email, titulo, body, convenio.vc_email_envio, convenio.vc_password_email, convenio.vc_smtp_email, convenio.nu_puerto_smtp_email, convenio.bi_ssl_email, convenio.vc_desc_empresa, con_sql);
                        }

                    }



                    tran_sql.Commit();
                    _logger.Information("idtrx: " + id_trx_incomm_global + " / " + "id_transaccion: " + id_trans_global + " / " + "id_transaccion_incomm: " + id_incomm_global + " / " + "Transaccion incomm exitosa " + idtran + " " + result.transactionId);

                    object info = new object();

                    info = new
                    {
                        codigo = "00",
                        mensaje = "OK",
                        nro_transaccion = id_trans_global
                    };
                   

                    return info;

                }
                else 
                {
                    TransaccionModel tm = new TransaccionModel();
                    tm.nu_id_trx = idtran;
                    tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                    tm.nu_id_comercio = comercio.nu_id_comercio;
                    tm.dt_fecha = fechatran;
                    tm.nu_id_producto = producto.nu_id_producto;
                    tm.nu_precio = producto.nu_valor_facial;

                    if (result.errorMessage== null)
                        tm.vc_desc_error = "";
                    else
                        tm.vc_desc_error = result.errorMessage;

                    if (result.errorCode == null)
                        tm.vc_cod_error = "";
                    else
                        tm.vc_cod_error = result.errorCode;


                    if (result.errorType == null)
                        tm.vc_desc_tipo_error = "";
                    else
                        tm.vc_desc_tipo_error = result.errorType;


                    con_sql.Open();
                    tran_sql = con_sql.BeginTransaction();

                    ins_bd = true;

                    cmd = insTransaccionError(con_sql, tran_sql, tm);

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql.Rollback();
                        _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                        return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                    }

                    tran_sql.Commit();
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "Transaccion incomm error: " +tm.vc_cod_error + "/" + tm.vc_desc_tipo_error + "/"+ tm.vc_desc_error);
                    return UtilSql.sOutPutTransaccion("06", "Ocurrio un error en la transaccion.");

                }

            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }

                _logger.Error("idtrx: " + id_trx_incomm_global + " / " + "id_transaccion: " + id_trans_global + " / " + "id_transaccion_incomm: " + id_incomm_global + " / " + ex,ex.Message);
                return UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }


        public object pr_sms(string conexion, string nro_telefono)
        {
            SqlConnection con_sql = null;
            try
            {
                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                ConvenioModel convenio = get_convenio(con_sql, 1);
                var id_trans_global = "0";

                var mensajetxt = "felicitaciones. tu pin esta listo para ser activado, el codigo es:. terminos y condiciones en " + convenio.vc_url_web_terminos;
                SendMessage(mensajetxt, nro_telefono, convenio.vc_aws_access_key_id, convenio.vc_aws_secrect_access_key, id_trans_global, con_sql);

                return 1;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public object pr_email(string conexion, string correo_destino)
        {
            SqlConnection con_sql = null;
            try
            {

                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                ConvenioModel convenio = get_convenio(con_sql, 1);
                var id_trans_global = "0";

                //Prueba
                var body = _send.GetBodyFinal(convenio.vc_desc_empresa, "PlayStation", "Pin Virtual Peru Playstation Plus 12 Meses", convenio.vc_color_header_email, convenio.vc_color_body_email, "XXXX -XXXX-XXXX", "XXXXXXXXXXX", DateTime.Now.ToString(), "ON220113.1019.A00001", "00000000001316140195", "20.00", convenio.vc_url_web_terminos, false);
                var titulo = "Confirmación de transacción #XXXXXXXX.XXXX.XXXXXX";

                _send.Email(id_trans_global, correo_destino, titulo, body, convenio.vc_email_envio, convenio.vc_password_email, convenio.vc_smtp_email, convenio.nu_puerto_smtp_email, convenio.bi_ssl_email, convenio.vc_desc_empresa, con_sql);

                return 1;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task SendMessage(string mensaje,string numero, string vc_aws_access_key_id, string vc_aws_secrect_access_key, string id_trx, SqlConnection cn)
        {
            try
            {
                AmazonSimpleNotificationServiceClient client =
                       new AmazonSimpleNotificationServiceClient(vc_aws_access_key_id,
                                                                   vc_aws_secrect_access_key,
                                                                   Amazon.RegionEndpoint.USEast1);

                var request = new PublishRequest
                {
                    Message = mensaje,
                    PhoneNumber = numero

                };

                await client.PublishAsync(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "id_trx: "+ id_trx + ex.Message);
                ReenvioMensajeModel rm = new ReenvioMensajeModel();
                rm.nu_id_trx = id_trx;
                rm.ch_tipo_mensaje = "M";
                var cmd = insSendInfoError(cn, rm);
            } 
        }

        private static SqlCommand insTransaccion(SqlConnection cn, SqlTransaction tran, TransaccionModel model)
        {

            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_transaccion_incomm", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                cmd.Parameters.AddWithValue("@dt_fecha", model.dt_fecha);
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@nu_precio", model.nu_precio);
                cmd.Parameters.AddWithValue("@vc_id_ref_trx", model.vc_id_ref_trx);
                cmd.Parameters.AddWithValue("@vc_cod_autorizacion", model.vc_cod_autorizacion);
                cmd.Parameters.AddWithValue("@vc_nro_pin", model.vc_nro_pin);
                cmd.Parameters.AddWithValue("@vc_email_sol", model.vc_email_sol);
                cmd.Parameters.AddWithValue("@vc_telefono_sol", model.vc_telefono_sol);

                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }


        }
        private static SqlCommand insTrxhub(SqlConnection cn, TrxHubModel model)
        {

            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_trxhub", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.codigo_distribuidor);
                cmd.Parameters.AddWithValue("@vc_cod_comercio", model.codigo_comercio);
                cmd.Parameters.AddWithValue("@vc_nombre_comercio", model.nombre_comercio);
                cmd.Parameters.AddWithValue("@vc_nro_telefono", model.nro_telefono);
                cmd.Parameters.AddWithValue("@vc_email", model.email);
                cmd.Parameters.AddWithValue("@vc_id_producto", model.id_producto);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }

        }
        private static SqlCommand insSendInfoError(SqlConnection cn, ReenvioMensajeModel model)
        {

            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_reenvio_mensaje", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@ch_tipo_mensaje", model.ch_tipo_mensaje);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }

        }
        private static SqlCommand insTransaccionError(SqlConnection cn, SqlTransaction tran, TransaccionModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_transaccion_error_incomm", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                cmd.Parameters.AddWithValue("@dt_fecha", model.dt_fecha);
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@nu_precio", model.nu_precio);
                cmd.Parameters.AddWithValue("@vc_cod_error", model.vc_cod_error);
                cmd.Parameters.AddWithValue("@vc_desc_error", model.vc_desc_error);
                cmd.Parameters.AddWithValue("@vc_desc_tipo_error", model.vc_desc_tipo_error);
                
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        private DistribuidorModel get_distribuidor(SqlConnection cn, string vc_cod_distribuidor)
        {
            DistribuidorModel model = new DistribuidorModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_distribuidor_incomm", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.vc_cod_distribuidor = vc_cod_distribuidor;
                cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (UtilSql.Ec(dr, "nu_id_distribuidor"))
                        model.nu_id_distribuidor = Convert.ToInt32(dr["nu_id_distribuidor"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_distribuidor"))
                        model.vc_cod_distribuidor = dr["vc_cod_distribuidor"].ToString();
                    if (UtilSql.Ec(dr, "vc_zip_code"))
                        model.vc_zip_code = dr["vc_zip_code"].ToString();
                    if (UtilSql.Ec(dr, "bi_valor_pin"))
                        model.bi_valor_pin = dr["bi_valor_pin"].ToBool();
                }
            }
            return model;
        }
        private ComercioModel get_comercio(SqlConnection cn, string vc_cod_comercio, string vc_nombre_comercio, int nu_id_distribuidor)
        {
            ComercioModel model = new ComercioModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_comercio_incomm", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_id_distribuidor = nu_id_distribuidor;
                model.vc_cod_comercio = vc_cod_comercio;
                model.vc_nombre_comercio = vc_nombre_comercio;
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@vc_cod_comercio", model.vc_cod_comercio);
                cmd.Parameters.AddWithValue("@vc_nombre_comercio", model.vc_nombre_comercio);
                cmd.Parameters.AddWithValue("@vc_usuario", model.vc_tran_usua_regi);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())

                {
                    if (UtilSql.Ec(dr, "nu_id_comercio"))
                        model.nu_id_comercio = Convert.ToInt32(dr["nu_id_comercio"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_comercio"))
                        model.vc_cod_comercio = dr["vc_cod_comercio"].ToString();
                    if (UtilSql.Ec(dr, "vc_nombre_comercio"))
                        model.vc_nombre_comercio = dr["vc_nombre_comercio"].ToString();
                }
            }
            return model;
        }
        private ProductoModel get_producto(SqlConnection cn, int nu_id_producto,int nu_id_distribuidor)
        {
            ProductoModel _result = new ProductoModel();
            ProductoModel model = new ProductoModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_producto_incomm", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_id_producto = nu_id_producto;
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", nu_id_distribuidor);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())

                {
                    if (UtilSql.Ec(dr, "nu_id_producto"))
                        _result.nu_id_producto = Convert.ToInt32(dr["nu_id_producto"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_producto"))
                        _result.vc_cod_producto = dr["vc_cod_producto"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_producto"))
                        _result.vc_desc_producto = dr["vc_desc_producto"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_producto"))
                        _result.vc_desc_producto = dr["vc_desc_producto"].ToString();
                    if (UtilSql.Ec(dr, "vc_cod_ean"))
                        _result.vc_cod_ean = dr["vc_cod_ean"].ToString();
                    if (UtilSql.Ec(dr, "nu_valor_facial"))
                        _result.nu_valor_facial = Convert.ToInt32(dr["nu_valor_facial"].ToString());

                    if (UtilSql.Ec(dr, "vc_desc_categoria"))
                        _result.vc_desc_categoria = dr["vc_desc_categoria"].ToString();


                    if (UtilSql.Ec(dr, "bi_envio_sms"))
                        _result.bi_envio_sms =  dr["bi_envio_sms"].ToBool();
                    if (UtilSql.Ec(dr, "bi_envio_email"))
                        _result.bi_envio_email = dr["bi_envio_email"].ToBool();

                }
            }
            return _result;
        }
        private ConvenioModel get_convenio(SqlConnection cn, int nu_id_convenio)
        {
            ConvenioModel _result = new ConvenioModel();
            ConvenioModel model = new ConvenioModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_convenio_incomm", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_id_convenio = nu_id_convenio;
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())

                {
                    if (UtilSql.Ec(dr, "vc_clave_encrip_aut"))
                        _result.vc_clave_encrip_aut = dr["vc_clave_encrip_aut"].ToString();
                    if (UtilSql.Ec(dr, "vc_merchant_id"))
                        _result.vc_merchant_id = dr["vc_merchant_id"].ToString();
                    if (UtilSql.Ec(dr, "vc_pos_id"))
                        _result.vc_pos_id = dr["vc_pos_id"].ToString();
                    if (UtilSql.Ec(dr, "vc_source_header"))
                        _result.vc_source_header = dr["vc_source_header"].ToString();
                    if (UtilSql.Ec(dr, "vc_source_body"))
                        _result.vc_source_body = dr["vc_source_body"].ToString();
                    if (UtilSql.Ec(dr, "vc_aws_access_key_id"))
                        _result.vc_aws_access_key_id = dr["vc_aws_access_key_id"].ToString();
                    if (UtilSql.Ec(dr, "vc_aws_secrect_access_key"))
                        _result.vc_aws_secrect_access_key = dr["vc_aws_secrect_access_key"].ToString();
                    if (UtilSql.Ec(dr, "vc_url_web_terminos"))
                        _result.vc_url_web_terminos = dr["vc_url_web_terminos"].ToString();

                    if (UtilSql.Ec(dr, "vc_desc_empresa"))
                        _result.vc_desc_empresa = dr["vc_desc_empresa"].ToString();
                    if (UtilSql.Ec(dr, "vc_color_header_email"))
                        _result.vc_color_header_email = dr["vc_color_header_email"].ToString();
                    if (UtilSql.Ec(dr, "vc_color_body_email"))
                        _result.vc_color_body_email = dr["vc_color_body_email"].ToString();
                    if (UtilSql.Ec(dr, "vc_email_envio"))
                        _result.vc_email_envio = dr["vc_email_envio"].ToString();
                    if (UtilSql.Ec(dr, "vc_password_email"))
                        _result.vc_password_email = dr["vc_password_email"].ToString();
                    if (UtilSql.Ec(dr, "vc_smtp_email"))
                        _result.vc_smtp_email = dr["vc_smtp_email"].ToString();
                    if (UtilSql.Ec(dr, "nu_puerto_smtp_email"))
                        _result.nu_puerto_smtp_email = Convert.ToInt32(dr["nu_puerto_smtp_email"].ToString());
                    if (UtilSql.Ec(dr, "bi_ssl_email"))
                        _result.bi_ssl_email = Convert.ToBoolean(dr["bi_ssl_email"].ToString());
                    if (UtilSql.Ec(dr, "vc_url_api_aes"))
                        _result.vc_url_api_aes = dr["vc_url_api_aes"].ToString();
                    if (UtilSql.Ec(dr, "vc_clave_aes"))
                        _result.vc_clave_aes = dr["vc_clave_aes"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_ip"))
                        _result.vc_nro_ip = dr["vc_nro_ip"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_telefono_tran_incomm"))
                        _result.vc_nro_telefono_tran_incomm = dr["vc_nro_telefono_tran_incomm"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_complete_incomm"))
                        _result.vc_nro_complete_incomm = dr["vc_nro_complete_incomm"].ToString();


                    if (UtilSql.Ec(dr, "vc_celular_def"))
                        _result.vc_celular_def = dr["vc_celular_def"].ToString();
                    if (UtilSql.Ec(dr, "vc_email_def"))
                        _result.vc_email_def = dr["vc_email_def"].ToString();

                }
            }
            return _result;
        }
        private int get_id_transaccion(SqlConnection cn)
        {
            int r = 0;
            ConvenioModel model = new ConvenioModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_id_transaccion", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())

                {
                    if (UtilSql.Ec(dr, "id_trans"))
                        r = Convert.ToInt32(dr["id_trans"].ToString());
                }
            }
            return r;
        }



  



    }
}
