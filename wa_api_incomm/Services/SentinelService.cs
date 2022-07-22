using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Services
{
    public class SentinelService : ISentinelService
    {
        public int nu_id_convenio = 2;

        private readonly Serilog.ILogger _logger;

        Models.Hub.ConvenioModel hub_convenio;
        public SentinelService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object sel_tipo_documento_identidad(string conexion, Sentinel_InputModel.Busqueda_Sentinel_Input input)
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
                        model.nu_tran_ruta = 2;

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
        public object get_validar_titular(string conexion, SentinelInfo info, Sentinel_InputModel.Consultado model)
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

                string vc_cod_error_hub = "99";
                string vc_desc_error_hub = "Error desconocido.";


                SqlConnection con_sql = null;
                con_sql = new SqlConnection(conexion);


                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                SentinelApi api = new SentinelApi(hub_convenio);

                con_sql.Open();
                TipoDocIdentidadModel tipodocidentidad_consultado = new TipoDocIdentidadModel();
                try
                {
                    tipodocidentidad_consultado = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultado), nu_id_convenio);
                }
                catch (Exception ex)
                {
                    tipodocidentidad_consultado = global_service.get_tipo_documento_codigo(con_sql, model.tipo_documento_consultado, nu_id_convenio);
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

                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, encripta_rest.coderror.ToString(), ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);
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

                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, encripta_rest.coderror.ToString(), ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);
                }
                modelo.Gx_PasEnc = encripta_rest.encriptado;

                modelo.Gx_Key = info.Gx_Key;
                modelo.TipoDocSol = tipodocidentidad_consultado.vc_cod_tipo_doc_identidad;
                modelo.NroDocSol = model.numero_documento_consultado;

                rest = api.ConsultaPersona(modelo).Result;

                rest.MensajeWS = "Consulta exitosa.";

                if (rest.CodigoWS != "0")
                {
                    response.codigo = rest.CodigoWS;

                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, rest.CodigoWS, ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);
                }

                response.codigo = "00";
                response.nombres = rest.Nombres;
                response.apellido_paterno = rest.ApellidoPaterno;
                response.apellido_materno = rest.ApellidoMaterno;

                return response;
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
            }

        }
        public object ins_transaccion(string conexion, SentinelInfo info,  Sentinel_InputModel.Ins_Transaccion model, string id_trx_hub, TipoDocIdentidadModel tipodocidentidad_solicitante, TipoDocIdentidadModel tipodocidentidad_consultado, TipoDocIdentidadModel tipodocidentidad_PDV, DistribuidorModel distribuidor, ComercioModel comercio, ProductoModel producto)
        {
            bool ins_bd = false;
            bool saldo_comprometido = false;
            bool transaccion_completada = false;
            //string id_trx_hub = "";
            string id_trans_global = "";

            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;

            string codigo_error = "";

            string vc_cod_error_hub = "99";
            string vc_desc_error_hub = "Error desconocido.";

            Encripta encripta = null;
            EncriptaRest encripta_rest = null;
            ConsultaTitularFac modelo = new ConsultaTitularFac();
            string mensaje_error = "";

            GlobalService global_service = new GlobalService();
            try
            {
                con_sql = new SqlConnection(conexion);

                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                SentinelApi api = new SentinelApi(hub_convenio);

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
                    return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                }
                saldo_comprometido = true;

                con_sql.Close();

                // 4) Enviar Solicitud al proveedor

                //encriptar el usuario
                encripta = new Encripta();
                encripta.keysentinel = (model.bono ? info.keysentinel_bonos : info.keysentinel);
                //encripta.keysentinel = info.keysentinel;
                encripta.parametro = (model.bono ? info.Usuario_bonos : info.Usuario);
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
                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, encripta_rest.coderror.ToString(), ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);

                }
                modelo.Gx_UsuEnc = encripta_rest.encriptado;

                //encriptar la contraseña
                encripta = new Encripta();
                encripta.keysentinel = (model.bono ? info.keysentinel_bonos : info.keysentinel);
                //encripta.keysentinel = info.keysentinel;
                encripta.parametro = (model.bono ? info.Contrasena_bonos : info.Contrasena);
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
                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, encripta_rest.coderror.ToString(), ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);

                }
                modelo.Gx_PasEnc = encripta_rest.encriptado;

                modelo.TipoDocSol = tipodocidentidad_solicitante.vc_cod_tipo_doc_identidad;
                modelo.NroDocSol = model.numero_documento_solicitante;
                modelo.DigVerSol = model.digito_verificador_solicitante;
                modelo.CorreoSol = model.email_solicitante;
                modelo.TelefSol = model.telefono_solicitante;

                modelo.TipoDocComprobante = model.tipo_documento_facturacion;
                modelo.NroDocumentoFAC = model.tipo_documento_facturacion == "FAC" ? model.numero_ruc : "";

                modelo.PDVTipoDoc = tipodocidentidad_PDV.vc_cod_tipo_doc_identidad;
                modelo.PDVNroDoc = model.numero_documento_PDV;
                modelo.PDVRazSocNom = model.razon_social_PDV;

                modelo.Gx_Key = (model.bono ? info.Gx_Key_bonos : info.Gx_Key);
                //modelo.Gx_Key = info.Gx_Key;
                modelo.SDT_TitMas.Add(new ConsultaTitularDet()
                {
                    TipoDocCPT = tipodocidentidad_consultado.vc_cod_tipo_doc_identidad,
                    NroDocCPT = model.numero_documento_consultado,
                    TipoReporte = producto.vc_cod_producto == "G" ? "R" : producto.vc_cod_producto,
                    Pago = producto.vc_cod_producto == "G" ? "N" : "S"
                });

                modelo.ReferenceCode = id_trans_global.ToString();
                ConsultaTitularRest response = new ConsultaTitularRest();

                //QA
                if (hub_convenio.vc_url_api_1.Contains("wsrestqa") == true)
                {
                    modelo.ReferenceCode = "QA" + modelo.ReferenceCode;

                    response = new ConsultaTitularRest();
                    response.CodigoWS = "0";
                    response.ID_Transaccion = "TestOK";
                }

                //PRODUCCION
                if (hub_convenio.vc_url_api_1.Contains("wsrestqa") == false || info.EnvioSentinelQA == "1")
                {
                    if (hub_convenio.vc_url_api_1.Contains("wsrestqa") == false)
                    {
                        modelo.ReferenceCode = "HUB" + modelo.ReferenceCode;
                    }
                    if (model.bono == true && info.EnvioAPIBono == "1")
                    {
                        response = api.ConsultaTitularSinFacturacion(modelo, _logger, id_trx_hub).Result;
                    }
                    else
                    {
                        response = api.ConsultaTitularFacturacion(modelo, _logger, id_trx_hub).Result;
                    }
                }
                
                TransaccionModel trx = new TransaccionModel();
                trx.nu_id_trx = idtran;
                trx.nu_id_trx_hub = Int64.Parse(id_trx_hub);
                trx.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                trx.vc_cod_distribuidor = model.codigo_distribuidor;
                trx.nu_id_comercio = comercio.nu_id_comercio;
                trx.vc_cod_comercio = model.codigo_comercio;
                trx.nu_id_tipo_doc_sol = tipodocidentidad_solicitante.nu_id_tipo_doc_identidad;
                trx.vc_cod_tipo_doc_sol = tipodocidentidad_solicitante.vc_cod_tipo_doc_identidad;
                trx.vc_nro_doc_sol = model.numero_documento_solicitante;
                trx.ch_dig_ver_sol = model.digito_verificador_solicitante;
                trx.vc_telefono_sol = model.telefono_solicitante;
                trx.vc_email_sol = model.email_solicitante;
                trx.vc_tipo_comprobante = model.tipo_documento_facturacion;
                trx.nu_id_tipo_doc_cpt = tipodocidentidad_consultado.nu_id_tipo_doc_identidad;
                trx.vc_cod_tipo_doc_cpt = tipodocidentidad_consultado.vc_cod_tipo_doc_identidad;
                trx.vc_nro_doc_cpt = model.numero_documento_consultado;
                trx.nu_id_producto = producto.nu_id_producto;
                trx.vc_cod_producto = producto.vc_cod_producto;
                trx.vc_ruc = model.numero_ruc;

                trx.PDVTipoDoc = tipodocidentidad_PDV.vc_cod_tipo_doc_identidad;
                trx.PDVNroDoc = model.numero_documento_PDV;
                trx.PDVRazSocNom = model.razon_social_PDV;
                trx.vc_tran_usua_regi = "API";
                trx.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;                
                try { trx.ti_respuesta_api = (response.dt_fin - response.dt_inicio); } catch (Exception ti) { }

                if (response.CodigoWS == "0")
                {
                    trx.vc_id_ref_trx = response.ID_Transaccion.ToString();

                    //graba primero en BD
                    con_sql.Open();
                    tran_sql = con_sql.BeginTransaction();
                    ins_bd = true;

                    using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_sentinel", con_sql, tran_sql))
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
                        cmd.Parameters.AddWithValue("@vc_telefono_sol", trx.vc_telefono_sol);
                        cmd.Parameters.AddWithValue("@nu_id_tipo_doc_cpt", trx.nu_id_tipo_doc_cpt);
                        cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", trx.vc_nro_doc_cpt);
                        cmd.Parameters.AddWithValue("@vc_tipo_comprobante", trx.vc_tipo_comprobante);
                        cmd.Parameters.AddWithValue("@vc_ruc", trx.vc_ruc);
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

                    object _info = new object();

                    _info = new
                    {
                        codigo = "00",
                        mensaje = "Operación exitosa, la información le llegará al correo electrónico registrado.",
                        nro_transaccion = id_trans_global
                    };

                    return _info;

                }

                else
                {
                    string codigo = "";
                    string descripcion = "";
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


                    if (response.CodigoWS == null)
                    {
                        tm.vc_cod_error = "";
                        tm.vc_desc_error = "";
                    }
                    else
                    {
                        con_sql.Open();
                        global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, response.CodigoWS, ref vc_cod_error_hub, ref vc_desc_error_hub);
                        con_sql.Close();

                        codigo = vc_cod_error_hub;
                        descripcion = vc_desc_error_hub;

                        tm.vc_cod_error = response.CodigoWS;
                        tm.vc_desc_error = vc_desc_error_hub;

                        foreach (var item in response.SDT_TitMas_Out)
                        {
                            con_sql.Open();
                            global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, item.CodigoVal, ref vc_cod_error_hub, ref vc_desc_error_hub);
                            con_sql.Close();
                            
                            codigo = String.Concat(tm.vc_cod_error, vc_cod_error_hub, " - ");
                            descripcion = String.Concat(tm.vc_desc_error, vc_desc_error_hub, " - ");

                            tm.vc_cod_error = String.Concat(tm.vc_cod_error, item.CodigoVal, " - ");
                            tm.vc_desc_error = String.Concat(tm.vc_desc_error, vc_desc_error_hub, " - ");

                        }

                        if (codigo.Length > 0)
                            codigo = codigo.Substring(0, codigo.Length - 3);
                        if (descripcion.Length > 0)
                            descripcion = descripcion.Substring(0, descripcion.Length - 3);

                        if (tm.vc_cod_error.Length > 0)
                            tm.vc_cod_error = tm.vc_cod_error.Substring(0, tm.vc_cod_error.Length - 3);
                        if (tm.vc_desc_error.Length > 0)
                            tm.vc_desc_error = tm.vc_desc_error.Substring(0, tm.vc_desc_error.Length - 3);
                    }

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

                    _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);
                    
                    return UtilSql.sOutPutTransaccion(codigo, descripcion);

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
                
                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
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

                if (transaccion_completada == false && id_trx_hub != "")
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
