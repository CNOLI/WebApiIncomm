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
    public class IncommService : IIncommService
    {
        public int nu_id_convenio = 1;

        private readonly Serilog.ILogger _logger;
        private readonly Send _send;

        Models.Hub.ConvenioModel hub_convenio;
        public IncommService(Serilog.ILogger logger, Send send)
        {
            _send = send;
            _logger = logger;
        }
        public object execute_trans(string conexion, Incomm_InputTransModel input)
        {

            bool ins_bd = false;
            bool saldo_comprometido = false;
            bool transaccion_completada = false;
            string id_trans_global = "";
            string id_trx_hub = "";
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlTransaction tran_sql_error = null;
            SqlCommand cmd = null;
            string mensaje_error = "";
            GlobalService global_service = new GlobalService();
            try
            {
                con_sql = new SqlConnection(conexion);

                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                //1) Inserta TRX_HUB y validaciones por BD

                con_sql.Open();

                DistribuidorModel distribuidor = get_distribuidor(con_sql, input.codigo_distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("01", "El código de distribuidor no existe");
                }

                ComercioModel comercio = get_comercio(con_sql, input.codigo_comercio, input.nombre_comercio, distribuidor.nu_id_distribuidor);

                //Completar 51 para Perú
                if (input.nro_telefono.Length == 9)
                {
                    input.nro_telefono = "51" + input.nro_telefono;
                }

                TrxHubModel trx_hub = new TrxHubModel();
                trx_hub.vc_cod_distribuidor = input.codigo_distribuidor;
                trx_hub.vc_cod_comercio = input.codigo_comercio;
                trx_hub.vc_nombre_comercio = input.nombre_comercio;
                trx_hub.vc_nro_telefono = input.nro_telefono;
                trx_hub.vc_email = input.email;
                trx_hub.vc_id_producto = input.id_producto;
                trx_hub.vc_id_ref_trx_distribuidor = input.nro_transaccion_referencia;

                cmd = global_service.insTrxhub(con_sql, trx_hub);
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    _logger.Error(cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                }
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 2)
                {
                    _logger.Error(cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    return UtilSql.sOutPutTransaccion("99", cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                }

                id_trx_hub = cmd.Parameters["@nu_tran_pkey"].Value.ToString();

                con_sql.Close();

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Inicio de transaccion");

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Modelo recibido: " + JsonConvert.SerializeObject(input));

                // 2) Validar Campos adicionales.

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
                        mensaje_error = "El número de telefono " + input.nro_telefono + " es incorrecto";
                        _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                        return UtilSql.sOutPutTransaccion("02", mensaje_error);
                    }
                }

                if (bi_envio_email_firts)
                {
                    if (!new EmailAddressAttribute().IsValid(input.email))
                    {
                        mensaje_error = "El email " + input.email + " es incorrecto";
                        _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                        return UtilSql.sOutPutTransaccion("03", mensaje_error);
                    }
                }

                if (!Regex.Match(input.id_producto, @"(^[0-9]+$)").Success)
                {
                    mensaje_error = "El id del producto " + input.id_producto + " debe ser numerico";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("04", mensaje_error);
                }


                con_sql.Open();
                ProductoModel producto = get_producto(con_sql, Convert.ToInt32(input.id_producto), distribuidor.nu_id_distribuidor);

                if (producto.nu_id_producto <= 0)
                {
                    mensaje_error = "El producto " + input.id_producto + " no existe";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("05", mensaje_error);
                }
                ConvenioModel convenio = get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                // 3) Obtener ID Transacción y comprometer saldo.
                con_sql.Open();
                var idtran = global_service.get_id_transaccion(con_sql);

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

                // 4) Enviar Solicitud al proveedor.

                var fechatran = DateTime.Now;

                TransaccionIncommModel tim = new TransaccionIncommModel();
                tim.correlationId = id_trans_global;
                tim.issueDate = fechatran.ToString("yyyyMMddHHmmss.mss");
                tim.issuerId = distribuidor.vc_cod_distribuidor;
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

                //QA
                //ResultTransaccionIncomm result = new ResultTransaccionIncomm();
                //result.errorCode = "00";
                //result.transactionId = "ON222222.2222.A22222";
                //result.authorizationCode = "00000000002222222222";
                //result.pin = "QnappNOLbkFd+9/2XeOAs3+Swu3c50odsq6DLx3ifyQxn0YeclHuqcqq";

                //PROD
                IncommApi client = new IncommApi(hub_convenio, convenio.vc_merchant_id, convenio.vc_clave_encrip_aut, convenio.vc_pos_id, convenio.vc_source_header);
                var result = client.Transaccion(tim, _logger, id_trx_hub).Result;

                TransaccionModel tm = new TransaccionModel();
                tm.nu_id_trx = idtran;
                tm.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                tm.nu_id_comercio = comercio.nu_id_comercio;
                tm.dt_fecha = fechatran;
                tm.nu_id_producto = producto.nu_id_producto;
                tm.nu_precio = producto.nu_precio ?? 0;
                tm.vc_tran_usua_regi = "API";
                tm.nu_id_tipo_moneda_vta = 1;
                tm.vc_numero_servicio = "";
                tm.vc_id_ref_trx_distribuidor = input.nro_transaccion_referencia;
                try { tm.ti_respuesta_api = (result.dt_fin - result.dt_inicio); } catch (Exception ti) { }

                if (result.errorCode == "00")
                {
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
                    }

                    con_sql.Open();
                    tran_sql = con_sql.BeginTransaction();
                    ins_bd = true;
                    cmd = insTransaccion(con_sql, tran_sql, tm);
                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql.Rollback();
                        ins_bd = false;
                        _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                        return UtilSql.sOutPutTransaccion("99", cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    }
                    tm.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();


                    using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", con_sql, tran_sql))
                    {
                        cmd_upd.CommandType = CommandType.StoredProcedure;
                        cmd_upd.Parameters.AddWithValue("@nu_id_trx", tm.nu_id_trx_app);
                        cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", tm.vc_id_ref_trx);
                        cmd_upd.Parameters.AddWithValue("@vc_cod_autorizacion", tm.vc_cod_autorizacion);
                        cmd_upd.Parameters.AddWithValue("@vc_nro_pin", tm.vc_nro_pin);
                        UtilSql.iUpd(cmd_upd, trx_hub);
                        cmd_upd.ExecuteNonQuery();
                        if (cmd_upd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran_sql.Rollback();
                            ins_bd = false;
                            _logger.Error("idtrx: " + id_trx_hub + " / " + cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText());
                            mensaje_error = cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText();
                            return UtilSql.sOutPutTransaccion("99", cmd_upd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        }
                        cmd.Parameters["@vc_tran_codi"].Value = cmd_upd.Parameters["@vc_tran_codi"].Value;
                    }                                  

                    tran_sql.Commit();
                    ins_bd = false;
                    con_sql.Close();
                    transaccion_completada = true;

                    _logger.Information("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());

                    object info = new object();

                    info = new
                    {
                        codigo = "00",
                        mensaje = "Transacción realizada correctamente.",
                        nro_transaccion = id_trans_global
                    };

                    return info;

                }
                else
                {

                    if (result.errorMessage == null)
                        tm.vc_desc_error = "";
                    else
                        tm.vc_desc_error = result.errorMessage;

                    if (result.errorCode == null)
                        tm.vc_cod_error = "";
                    else
                        tm.vc_cod_error = result.errorCode;

                    tm.vc_desc_tipo_error = "CONVENIO";

                    con_sql.Open();
                    tran_sql_error = con_sql.BeginTransaction();

                    cmd = global_service.insTransaccionError(con_sql, tran_sql_error, tm);

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql_error.Rollback();
                        _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                        mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                        return UtilSql.sOutPutTransaccion("99", cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    }

                    tran_sql_error.Commit();
                    con_sql.Close();
                    ins_bd = false;
                    mensaje_error = tm.vc_desc_error;
                    _logger.Error("idtrx: " + id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);

                    return UtilSql.sOutPutTransaccion("99", "Ocurrio un error en la transaccion.");

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

                return UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion");
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

                if (transaccion_completada == false)
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
        public object extornar(string conexion, Incomm_InputTransExtornoModel input)
        {

            bool ins_bd = false;
            string id_trans_global = "";
            string id_trx_hub = "";
            bool saldo_comprometido = false;
            bool transaccion_completada = false;
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlTransaction tran_sql_error = null;
            SqlCommand cmd = null;
            string mensaje_error = "";
            GlobalService global_service = new GlobalService();
            try
            {
                con_sql = new SqlConnection(conexion);

                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                //1) Inserta TRX_HUB y validaciones por BD

                con_sql.Open();

                DistribuidorModel distribuidor = get_distribuidor(con_sql, input.codigo_distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("01", "El código de distribuidor no existe");
                }

                TrxHubModel trx = new TrxHubModel();
                trx.vc_cod_distribuidor = input.codigo_distribuidor;
                trx.vc_cod_comercio = input.codigo_comercio;
                trx.vc_nombre_comercio = input.nombre_comercio;
                trx.vc_nro_telefono = input.nro_telefono;
                trx.vc_email = input.email;
                trx.vc_id_producto = input.id_producto;
                trx.vc_id_ref_trx_distribuidor = input.nro_transaccion_referencia;
                trx.bi_extorno = true;

                cmd = global_service.insTrxhub(con_sql, trx);
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    _logger.Error(cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                }
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 2)
                {
                    _logger.Error(cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    return UtilSql.sOutPutTransaccion("99", cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                }

                id_trx_hub = cmd.Parameters["@nu_tran_pkey"].Value.ToString();

                con_sql.Close();


                _logger.Information("idtrx: " + id_trx_hub + " / " + "Inicio de transaccion extorno");

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Modelo recibido: " + JsonConvert.SerializeObject(input));

                // 2) Validar Campos adicionales.
                            
                con_sql.Open();

                ComercioModel comercio = new ComercioModel();
                comercio.vc_cod_comercio = input.codigo_comercio;
                comercio = global_service.get_comercio_busqueda(con_sql, input.codigo_comercio, distribuidor.nu_id_distribuidor);

                if (comercio.nu_id_comercio <= 0)
                {
                    return UtilSql.sOutPutTransaccion("07", "El código de comercio no existe");
                }

                if (!Regex.Match(input.id_producto, @"(^[0-9]+$)").Success)
                {
                    mensaje_error = "El id del producto " + input.id_producto + " debe ser numerico";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("04", mensaje_error);
                }


                ProductoModel producto = get_producto(con_sql, Convert.ToInt32(input.id_producto), distribuidor.nu_id_distribuidor);

                if (producto.nu_id_producto <= 0)
                {
                    mensaje_error = "El producto " + input.id_producto + " no existe";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("05", mensaje_error);
                }

                //ConvenioModel convenio = get_convenio(con_sql, nu_id_convenio);

                con_sql.Close();

                // 3) Obtener ID Transacción y comprometer saldo.
                con_sql.Open();
                var idtran = global_service.get_id_transaccion(con_sql);

                id_trans_global = idtran.ToString();

                TrxHubModel model_saldo = new TrxHubModel();

                model_saldo.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                model_saldo.bi_extorno = true;

                var cmd_saldo = global_service.updTrxhubSaldo(con_sql, model_saldo);

                if (cmd_saldo.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    mensaje_error = cmd_saldo.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", mensaje_error);
                }
                saldo_comprometido = true;

                con_sql.Close();

                // 4) Enviar Solicitud al proveedor.

                var fechatran = DateTime.Now;

                //Validar Datos

                TransaccionModel tm_validar = new TransaccionModel();
                tm_validar.vc_id_ref_trx_distribuidor = input.nro_transaccion_referencia;
                tm_validar.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                tm_validar.nu_id_comercio = comercio.nu_id_comercio;
                tm_validar.nu_id_producto = producto.nu_id_producto;
                tm_validar.dt_fecha = fechatran;
                tm_validar.nu_precio = producto.nu_precio ?? 0;
                tm_validar.vc_email_sol = input.email;
                tm_validar.vc_telefono_sol = input.nro_telefono;
                tm_validar.bi_extorno = true;

                con_sql.Open();
                tm_validar = global_service.get_transaccion_validar_datos_extorno(con_sql, tm_validar);
                con_sql.Close();
                if (tm_validar.nu_id_trx == null)
                {
                    mensaje_error = tm_validar.vc_observacion;
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", mensaje_error);
                }

                TransaccionModel tm = new TransaccionModel();
                tm.nu_id_trx = idtran;
                tm.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                tm.nu_id_comercio = comercio.nu_id_comercio;
                tm.dt_fecha = fechatran;
                tm.nu_id_producto = producto.nu_id_producto;
                tm.nu_precio = producto.nu_precio ?? 0;
                tm.vc_tran_usua_regi = "API";
                tm.nu_id_tipo_moneda_vta = 1;
                tm.vc_numero_servicio = "";
                tm.vc_telefono_sol = input.nro_telefono;
                tm.vc_email_sol = input.email;
                tm.nu_id_trx_ref = tm_validar.nu_id_trx;

                //CUANDO SE HABILITE EL API PARA EXTORNO DIRECTO CON INCOMM
                //IncommApi client = new IncommApi(hub_convenio, convenio.vc_merchant_id, convenio.vc_clave_encrip_aut, convenio.vc_pos_id, convenio.vc_source_header);
                //var result = client.Transaccion(tim, _logger, id_trx_hub).Result; 


                var result = new ResultTransaccionIncomm();

                result.errorCode = "00";

                if (result.errorCode == "00")
                {

                    tm.vc_id_ref_trx = result.transactionId;
                    tm.vc_cod_autorizacion = result.authorizationCode;
                    tm.vc_nro_pin = result.pin;

                    //CUANDO SE HABILITE EL API PARA EXTORNO DIRECTO CON INCOMM
                    //tm.bi_confirmado = true;

                    con_sql.Open();
                    tran_sql = con_sql.BeginTransaction();
                    ins_bd = true;
                    cmd = global_service.insTransaccionExtorno(con_sql, tran_sql, tm);
                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql.Rollback();
                        ins_bd = false;
                        mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToString();
                        _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                        return UtilSql.sOutPutTransaccion("99", mensaje_error);
                    }
                    tm.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();

                    tran_sql.Commit();
                    ins_bd = false;
                    con_sql.Close();
                    transaccion_completada = true;

                    _logger.Information("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());

                    object info = new object();

                    info = new
                    {
                        codigo = "00",
                        mensaje = "Transacción extornada correctamente.",
                        nro_transaccion = id_trans_global,
                        nro_transaccion_venta = tm.nu_id_trx_ref.ToString()
                    };

                    return info;

                }
                else
                {
                    tran_sql.Rollback();
                    ins_bd = false;
                    con_sql.Close();

                    if (result.errorMessage == null)
                        tm.vc_desc_error = "";
                    else
                        tm.vc_desc_error = result.errorMessage;

                    if (result.errorCode == null)
                        tm.vc_cod_error = "";
                    else
                        tm.vc_cod_error = result.errorCode;

                    tm.vc_desc_tipo_error = "CONVENIO";


                    con_sql.Open();
                    tran_sql_error = con_sql.BeginTransaction();

                    cmd = global_service.insTransaccionError(con_sql, tran_sql_error, tm);

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql_error.Rollback();
                        mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                        _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                        return UtilSql.sOutPutTransaccion("99", mensaje_error);
                    }

                    tran_sql_error.Commit();
                    con_sql.Close();
                    _logger.Error("idtrx: " + id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);

                    return UtilSql.sOutPutTransaccion("99", "Ocurrio un error en la transaccion.");

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

                return UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();

                if (saldo_comprometido == true && transaccion_completada == false)
                {
                    con_sql.Open();
                    TrxHubModel model_saldo_extorno = new TrxHubModel();

                    model_saldo_extorno.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                    model_saldo_extorno.bi_extorno = false;
                    model_saldo_extorno.bi_error = true;
                    model_saldo_extorno.vc_mensaje_error = mensaje_error;
                    var cmd_saldo_extorno = global_service.updTrxhubSaldo(con_sql, model_saldo_extorno);
                    con_sql.Close();
                }

                if (transaccion_completada == false)
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

        public object pr_sms(string conexion, string nro_telefono)
        {
            SqlConnection con_sql = null;
            try
            {
                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                ConvenioModel convenio = get_convenio(con_sql, 1);
                var id_trans_global = "0";

                var mensajefrom = "POSA";
                var mensajetxt = "Gracias por tu compra. Tu PIN está listo para ser activado, el código es:. Términos y condiciones en " + convenio.vc_url_web_terminos;
                bool bi_informado = true;
                _send.SendMessage(mensajefrom, mensajetxt, nro_telefono, convenio.vc_aws_access_key_id, convenio.vc_aws_secrect_access_key, id_trans_global, id_trans_global, con_sql, ref bi_informado);

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
                var body = _send.GetBodyIncomm(convenio.vc_desc_empresa, "PlayStation", "Pin Virtual Peru Playstation Plus 12 Meses", convenio.vc_color_header_email, convenio.vc_color_body_email, "XXXX -XXXX-XXXX", "XXXXXXXXXXX", DateTime.Now.ToString(), "ON220113.1019.A00001", "00000000001316140195", "20.00", convenio.vc_url_web_terminos, false, "");
                var titulo = "Confirmación de transacción #XXXXXXXX.XXXX.XXXXXX";
                bool bi_informado = false;
                _send.Email(id_trans_global, id_trans_global, correo_destino, titulo, body, convenio.vc_email_envio, convenio.vc_password_email, convenio.vc_smtp_email, convenio.nu_puerto_smtp_email??587, convenio.bi_ssl_email??false, convenio.vc_desc_empresa, con_sql, ref bi_informado);

                return 1;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task SendMessage(string from, string mensaje, string numero, string vc_aws_access_key_id, string vc_aws_secrect_access_key, string id_trx, SqlConnection cn)
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

                if (from.Length > 11)
                {
                    from = from.Substring(0, 11);
                }

                Dictionary<string, MessageAttributeValue> MessageAttributes = new Dictionary<string, MessageAttributeValue>
                  {
                    {
                      "AWS.SNS.SMS.SenderID", new MessageAttributeValue
                        { DataType = "String", StringValue = from.Replace(" ","-")}
                    }
                  };
                request.MessageAttributes = MessageAttributes;

                await client.PublishAsync(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "id_trx: " + id_trx + ex.Message);
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
                cmd.Parameters.AddWithValue("@nu_id_trx_hub", model.nu_id_trx_hub);
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
                cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", model.vc_id_ref_trx_distribuidor);
                cmd.Parameters.AddWithValue("@ti_respuesta_api", model.ti_respuesta_api);

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
                    if (UtilSql.Ec(dr, "nu_seg_encolamiento"))
                        model.nu_seg_encolamiento = dr["nu_seg_encolamiento"].ToInt();
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
        private ProductoModel get_producto(SqlConnection cn, int nu_id_producto, int nu_id_distribuidor)
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
                    if (UtilSql.Ec(dr, "nu_precio_vta"))
                        _result.nu_precio = Convert.ToDecimal(dr["nu_precio_vta"].ToString());


                    if (UtilSql.Ec(dr, "bi_envio_sms"))
                        _result.bi_envio_sms = dr["bi_envio_sms"].ToBool();
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
        private IncommModel get_transaccion(SqlConnection cn, IncommModel model)
        {
            IncommModel _result = new IncommModel();
            using (var cmd = new SqlCommand("tisi_trx.usp_get_transaccion_incomm", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())

                {
                    if (UtilSql.Ec(dr, "vc_url_api_aes"))
                        _result.vc_url_api_aes = dr["vc_url_api_aes"].ToString();
                    if (UtilSql.Ec(dr, "vc_clave_aes"))
                        _result.vc_clave_aes = dr["vc_clave_aes"].ToString();
                    if (UtilSql.Ec(dr, "vc_aws_access_key_id"))
                        _result.vc_aws_access_key_id = dr["vc_aws_access_key_id"].ToString();
                    if (UtilSql.Ec(dr, "vc_aws_secrect_access_key"))
                        _result.vc_aws_secrect_access_key = dr["vc_aws_secrect_access_key"].ToString();

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
                        _result.nu_puerto_smtp_email = dr["nu_puerto_smtp_email"].ToInt();
                    if (UtilSql.Ec(dr, "bi_ssl_email"))
                        _result.bi_ssl_email = dr["bi_ssl_email"].ToBool();
                    if (UtilSql.Ec(dr, "vc_url_web_terminos"))
                        _result.vc_url_web_terminos = dr["vc_url_web_terminos"].ToString();

                    if (UtilSql.Ec(dr, "nu_id_trx"))
                        _result.nu_id_trx = Convert.ToInt64(dr["nu_id_trx"].ToString());
                    if (UtilSql.Ec(dr, "nu_id_trx_hub"))
                        _result.nu_id_trx_hub = Convert.ToInt64(dr["nu_id_trx_hub"].ToString());
                    if (UtilSql.Ec(dr, "nu_id_distribuidor"))
                        _result.nu_id_distribuidor = Convert.ToInt32(dr["nu_id_distribuidor"].ToString());
                    if (UtilSql.Ec(dr, "nu_id_comercio"))
                        _result.nu_id_comercio = Convert.ToInt32(dr["nu_id_comercio"].ToString());
                    if (UtilSql.Ec(dr, "dt_fec_reg"))
                        _result.dt_fec_reg = dr["dt_fec_reg"].ToDateTime();
                    if (UtilSql.Ec(dr, "vc_telefono_sol"))
                        _result.vc_telefono_sol = dr["vc_telefono_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_email_sol"))
                        _result.vc_email_sol = dr["vc_email_sol"].ToString();
                    if (UtilSql.Ec(dr, "nu_precio_vta"))
                        _result.nu_precio_vta = dr["nu_precio_vta"].ToDecimal();
                    if (UtilSql.Ec(dr, "vc_nro_pin"))
                        _result.vc_nro_pin = dr["vc_nro_pin"].ToString();
                    if (UtilSql.Ec(dr, "vc_id_ref_trx"))
                        _result.vc_id_ref_trx = dr["vc_id_ref_trx"].ToString();
                    if (UtilSql.Ec(dr, "vc_cod_autorizacion"))
                        _result.vc_cod_autorizacion = dr["vc_cod_autorizacion"].ToString();

                    if (UtilSql.Ec(dr, "vc_cod_comercio"))
                        _result.vc_cod_comercio = dr["vc_cod_comercio"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_categoria"))
                        _result.vc_desc_categoria = dr["vc_desc_categoria"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_producto"))
                        _result.vc_desc_producto = dr["vc_desc_producto"].ToString();
                    if (UtilSql.Ec(dr, "vc_url_web_terminos"))
                        _result.vc_url_web_terminos = dr["vc_url_web_terminos"].ToString();
                    if (UtilSql.Ec(dr, "bi_valor_pin"))
                        _result.bi_valor_pin = dr["bi_valor_pin"].ToBool();

                    if (UtilSql.Ec(dr, "bi_confirmado"))
                        _result.bi_confirmado = dr["bi_confirmado"].ToBool();
                    if (UtilSql.Ec(dr, "bi_informado"))
                        _result.bi_informado = dr["bi_informado"].ToBool();

                }
            }
            return _result;
        }







    }
}
