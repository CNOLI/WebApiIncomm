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
                SentinelApi api = new SentinelApi();
                Encripta encripta = null;
                EncriptaRest encripta_rest = null;
                ConsultaPersona modelo = new ConsultaPersona();
                ConsultaPersonaRest rest = new ConsultaPersonaRest();
                Sentinel_ResponseModel response = new Sentinel_ResponseModel();
                string mensaje_error = "";


                SqlConnection con_sql = null;
                con_sql = new SqlConnection(conexion);

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
                    response.codigo = rest.CodigoWS;
                    response.mensaje = global_service.get_mensaje_error(nu_id_convenio, rest.CodigoWS);

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
        public object ins_transaccion(string conexion, SentinelInfo info, Sentinel_InputModel.Ins_Transaccion model)
        {
            bool ins_bd = false;
            string id_trx_hub = "";
            string id_trans_global = "";

            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;

            string codigo_error = "";


            SentinelApi api = new SentinelApi();
            Encripta encripta = null;
            EncriptaRest encripta_rest = null;
            ConsultaTitularFac modelo = new ConsultaTitularFac();
            string mensaje_error = "";

            try
            {
                GlobalService global_service = new GlobalService();

                con_sql = new SqlConnection(conexion);

                con_sql.Open();

                TrxHubModel trx_hub = new TrxHubModel();
                trx_hub.codigo_distribuidor = model.codigo_distribuidor;
                trx_hub.codigo_comercio = model.codigo_comercio;
                trx_hub.nombre_comercio = model.nombre_comercio;
                trx_hub.nro_telefono = "";
                trx_hub.email = model.email_consultante;
                trx_hub.id_producto = model.id_producto;

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

                if (!new EmailAddressAttribute().IsValid(model.email_consultante))
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El email " + model.email_consultante + " es incorrecto");
                    return UtilSql.sOutPutTransaccion("03", "El email es incorrecto");
                }

                if (!Regex.Match(model.id_producto, @"(^[0-9]+$)").Success)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El id del producto " + model.id_producto.ToString() + " debe ser numerico");
                    return UtilSql.sOutPutTransaccion("04", "El id del producto debe ser numerico");
                }

                con_sql.Open();
                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El código de distribuidor " + distribuidor.nu_id_distribuidor.ToString() + " no existe");
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
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El producto  " + producto.nu_id_producto.ToString() + " no existe");
                    return UtilSql.sOutPutTransaccion("06", "El producto no existe");
                }

                TipoDocIdentidadModel tipodocidentidad_consultante = new TipoDocIdentidadModel();
                try
                {
                    tipodocidentidad_consultante = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultante), nu_id_convenio);
                }
                catch (Exception ex)
                {
                    tipodocidentidad_consultante = global_service.get_tipo_documento_codigo(con_sql, model.tipo_documento_consultante, nu_id_convenio);
                }

                if (tipodocidentidad_consultante.nu_id_tipo_doc_identidad <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El tipo de documento de consultante " + tipodocidentidad_consultante.nu_id_tipo_doc_identidad.ToString() + " no existe");
                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento de consultante no existe.");
                }

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
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El tipo de documento de consultado " + tipodocidentidad_consultado.nu_id_tipo_doc_identidad.ToString() + " no existe");

                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento de consultado no existe.");
                }

                TipoDocIdentidadModel tipodocidentidad_PDV = new TipoDocIdentidadModel();
                try
                {
                    tipodocidentidad_PDV = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_PDV), nu_id_convenio);
                }
                catch (Exception ex)
                {
                    tipodocidentidad_PDV = global_service.get_tipo_documento_codigo(con_sql, model.tipo_documento_PDV, nu_id_convenio);
                }

                if (tipodocidentidad_PDV.nu_id_tipo_doc_identidad <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El tipo de documento del PDV " + tipodocidentidad_PDV.nu_id_tipo_doc_identidad.ToString() + " no existe");

                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento del PDV no existe.");
                }


                con_sql.Close();

                TransaccionModel trx = new TransaccionModel();
                trx.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                trx.vc_cod_distribuidor = model.codigo_distribuidor;
                trx.nu_id_comercio = comercio.nu_id_comercio;
                trx.vc_cod_comercio = model.codigo_comercio;
                trx.nu_id_tipo_doc_sol = tipodocidentidad_consultante.nu_id_tipo_doc_identidad;
                trx.vc_cod_tipo_doc_sol = tipodocidentidad_consultante.vc_cod_tipo_doc_identidad;
                trx.vc_nro_doc_sol = model.numero_documento_consultante;
                trx.ch_dig_ver_sol = model.digito_verificador_consultante;
                trx.vc_telefono_sol = model.telefono_consultante;
                trx.vc_email_sol = model.email_consultante;
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

                //encriptar el usuario
                encripta = new Encripta();
                encripta.keysentinel = (model.bono ? info.keysentinel_bonos : info.keysentinel);
                //encripta.keysentinel = info.keysentinel;
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
                    return UtilSql.sOutPutTransaccion(encripta_rest.coderror.ToString(), mensaje_error);
                }
                modelo.Gx_UsuEnc = encripta_rest.encriptado;

                //encriptar la contraseña
                encripta = new Encripta();
                encripta.keysentinel = (model.bono ? info.keysentinel_bonos : info.keysentinel);
                //encripta.keysentinel = info.keysentinel;
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

                    return UtilSql.sOutPutTransaccion(encripta_rest.coderror.ToString(), mensaje_error);
                }
                modelo.Gx_PasEnc = encripta_rest.encriptado;

                modelo.TipoDocSol = trx.vc_cod_tipo_doc_sol;
                modelo.NroDocSol = trx.vc_nro_doc_sol;
                modelo.DigVerSol = trx.ch_dig_ver_sol;
                modelo.CorreoSol = trx.vc_email_sol;
                modelo.TelefSol = trx.vc_telefono_sol;
                modelo.TipoDocComprobante = trx.vc_tipo_comprobante;
                modelo.NroDocumentoFAC = trx.vc_tipo_comprobante == "FAC" ? trx.vc_ruc : "";

                modelo.PDVTipoDoc = trx.PDVTipoDoc;
                modelo.PDVNroDoc = trx.PDVNroDoc;
                modelo.PDVRazSocNom = trx.PDVRazSocNom;

                modelo.Gx_Key = (model.bono ? info.Gx_Key_bonos : info.Gx_Key);
                //modelo.Gx_Key = info.Gx_Key;
                modelo.SDT_TitMas.Add(new ConsultaTitularDet()
                {
                    TipoDocCPT = trx.vc_cod_tipo_doc_cpt,
                    NroDocCPT = trx.vc_nro_doc_cpt,
                    TipoReporte = trx.vc_cod_producto == "G" ? "R" : trx.vc_cod_producto,
                    Pago = trx.vc_cod_producto == "G" ? "N" : "S"
                });

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

                using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_sentinel", con_sql, tran_sql))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nu_id_trx", id_trans_global);
                    cmd.Parameters.AddWithValue("@nu_id_trx_hub", id_trx_hub);
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

                    UtilSql.iIns(cmd, trx);
                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                    {
                        tran_sql.Rollback();
                        ins_bd = false;
                        _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                    }
                    trx.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();
                    modelo.ReferenceCode = trx.nu_id_trx_app.ToString();
                    var a = JsonConvert.SerializeObject(modelo);

                    ConsultaTitularRest response = new ConsultaTitularRest();

                    //QA
                    if (Config.bi_produccion == false)
                    {
                        modelo.ReferenceCode = "QA" + modelo.ReferenceCode;

                        response = new ConsultaTitularRest();
                        response.CodigoWS = "0";
                        response.ID_Transaccion = "TestOK";
                    }

                    //PRODUCCION
                    if (Config.bi_produccion == true || info.EnvioSentinelQA == "1")
                    {
                        if (Config.bi_produccion)
                        {
                            modelo.ReferenceCode = "HUB" + modelo.ReferenceCode;
                        }
                        if (model.bono)
                        {
                            response = api.ConsultaTitularSinFacturacion(modelo, _logger, id_trx_hub).Result;
                        }
                        else
                        {
                            response = api.ConsultaTitularFacturacion(modelo, _logger, id_trx_hub).Result;
                        }

                    }

                    if (response.CodigoWS == "0")
                    {
                        trx.vc_id_ref_trx = response.ID_Transaccion.ToString();

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
                                _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
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
                                _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                                return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                            }
                        }

                        tran_sql.Commit();
                        ins_bd = false;
                        con_sql.Close();

                        _logger.Information("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());

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
                        tran_sql.Rollback();
                        ins_bd = false;
                        con_sql.Close();

                        TransaccionModel tm = new TransaccionModel();
                        tm.nu_id_trx = Convert.ToInt32(id_trans_global);
                        tm.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                        tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                        tm.nu_id_comercio = comercio.nu_id_comercio;
                        tm.dt_fecha = DateTime.Now;
                        tm.nu_id_producto = producto.nu_id_producto;
                        tm.nu_precio = producto.nu_precio ?? 0;
                        tm.nu_id_tipo_moneda_vta = 1;
                        tm.vc_numero_servicio = "";
                        tm.vc_tran_usua_regi = "API";


                        if (response.CodigoWS == null)
                        {
                            tm.vc_cod_error = "";
                            tm.vc_desc_error = "";
                        }
                        else
                        {
                            tm.vc_cod_error = response.CodigoWS;
                            tm.vc_desc_error = global_service.get_mensaje_error(nu_id_convenio, response.CodigoWS);
                            foreach (var item in response.SDT_TitMas_Out)
                            {
                                tm.vc_cod_error += " - " + item.CodigoVal;
                                tm.vc_desc_error += " - " + global_service.get_mensaje_error(nu_id_convenio, item.CodigoVal);
                            }
                        }


                        tm.vc_desc_tipo_error = "CONVENIO";


                        SqlTransaction tran_sql_error = null;
                        con_sql.Open();

                        tran_sql_error = con_sql.BeginTransaction();

                        cmd = global_service.insTransaccionError(con_sql, tran_sql_error, tm);

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                        {
                            tran_sql_error.Rollback();
                            _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                            return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                        }

                        tran_sql_error.Commit();
                        _logger.Error("idtrx: " + id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);
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

                _logger.Error("idtrx: " + id_trx_hub + " / " + ex.Message);
                return UtilSql.sOutPutTransaccion("500", ex.Message);
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }


        }

    }
}
