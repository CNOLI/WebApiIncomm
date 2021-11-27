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
        //public object sel_banco(string conexion, Sentinel_InputModel.Busqueda input)
        //{
        //    using (SqlConnection cn = new SqlConnection(conexion))
        //    {
        //        try
        //        {
        //            Sentinel_ResponseModel model_response = new Sentinel_ResponseModel();
        //            ProductoModel model = new ProductoModel();
        //            model.vc_tran_clve_find = input.buscador;

        //            cn.Open();
        //            using (var cmd = new SqlCommand("tisi_global.usp_sel_banco", cn))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                model.nu_tran_ruta = 1;

        //                UtilSql.iGet(cmd, model);
        //                SqlDataReader dr = cmd.ExecuteReader();

        //                var lm = new List<Sentinel_ResponseModel>();
        //                while (dr.Read())
        //                {
        //                    model_response = new Sentinel_ResponseModel();
        //                    if (UtilSql.Ec(dr, "NU_ID_BANCO"))
        //                        model_response.id_banco = dr["NU_ID_BANCO"].ToDecimal();
        //                    if (UtilSql.Ec(dr, "VC_DESC_BANCO"))
        //                        model_response.desc_banco = dr["VC_DESC_BANCO"].ToString();
        //                    lm.Add(model_response);
        //                }
        //                return lm;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }

        //    }
        //}
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
        public object sel_producto(string conexion, Sentinel_InputModel.Busqueda_Sentinel_Input input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    Sentinel_ResponseModel model_response = new Sentinel_ResponseModel();
                    ProductoModel model = new ProductoModel();
                    model.nu_id_convenio = nu_id_convenio;
                    model.vc_cod_distribuidor = input.codigo_distribuidor;
                    model.vc_tran_clve_find = input.buscador;
                    model.bi_estado = true;

                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_producto", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 3;
                        cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);

                        UtilSql.iGet(cmd, model);
                        SqlDataReader dr = cmd.ExecuteReader();
                        var lm = new List<Sentinel_ResponseModel>();
                        while (dr.Read())
                        {
                            model_response = new Sentinel_ResponseModel();
                            if (UtilSql.Ec(dr, "NU_ID_PRODUCTO"))
                                model_response.id_producto = dr["NU_ID_PRODUCTO"].ToDecimal();
                            if (UtilSql.Ec(dr, "VC_COD_PRODUCTO"))
                                model_response.codigo_producto = dr["VC_COD_PRODUCTO"].ToString();
                            if (UtilSql.Ec(dr, "VC_DESC_PRODUCTO"))
                                model_response.nombre_producto = dr["VC_DESC_PRODUCTO"].ToString();
                            if (UtilSql.Ec(dr, "NU_PRECIO"))
                                model_response.precio = dr["NU_PRECIO"].ToDecimal();
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
        //public object get_precio_producto(string conexion, Sentinel_InputModel.Get_Producto input)
        //{
        //    using (SqlConnection cn = new SqlConnection(conexion))
        //    {
        //        try
        //        {
        //            Sentinel_ResponseModel model_response = new Sentinel_ResponseModel();
        //            ProductoModel model = new ProductoModel();
        //            model.nu_id_convenio = nu_id_convenio;
        //            model.vc_cod_producto = input.cod_producto;

        //            cn.Open();
        //            using (var cmd = new SqlCommand("tisi_global.usp_get_producto", cn))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                model.nu_tran_ruta = 1;
        //                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
        //                cmd.Parameters.AddWithValue("@vc_cod_producto", model.vc_cod_producto);

        //                UtilSql.iGet(cmd, model);
        //                SqlDataReader dr = cmd.ExecuteReader();

        //                if (dr.Read())
        //                {
        //                    if (UtilSql.Ec(dr, "NU_PRECIO"))
        //                        model_response.precio = dr["NU_PRECIO"].ToDecimal();

        //                    return model_response;
        //                }
        //                return "";
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }

        //    }
        //}
        //public object get_saldo_distribuidor(string conexion, Sentinel_InputModel.Get_Distribuidor input)
        //{
        //    using (SqlConnection cn = new SqlConnection(conexion))
        //    {
        //        try
        //        {
        //            Sentinel_ResponseModel model_response = new Sentinel_ResponseModel();
        //            DistribuidorModel model = new DistribuidorModel();
        //            model.vc_cod_distribuidor = input.cod_distribuidor;

        //            cn.Open();
        //            using (var cmd = new SqlCommand("tisi_global.usp_get_distribuidor", cn))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                model.nu_tran_ruta = 1;
        //                cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);

        //                UtilSql.iGet(cmd, model);
        //                SqlDataReader dr = cmd.ExecuteReader();

        //                if (dr.Read())
        //                {
        //                    if (UtilSql.Ec(dr, "NU_SALDO"))
        //                        model_response.saldo = dr["NU_SALDO"].ToDecimal();

        //                    return model_response;
        //                }
        //                return "";
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }

        //    }
        //}
        //public object sel_transaccion(string conexion, Sentinel_InputModel.Sel_Transaccion input)
        //{
        //    using (SqlConnection cn = new SqlConnection(conexion))
        //    {
        //        try
        //        {
        //            Sentinel_ResponseModel model_response = new Sentinel_ResponseModel();
        //            TransaccionModel model = new TransaccionModel();
        //            model.vc_cod_distribuidor = input.cod_distribuidor;
        //            model.vc_cod_comercio = input.cod_comercio;
        //            model.dt_fec_inicio = input.fec_inicio;
        //            model.dt_fec_final = input.fec_final;

        //            cn.Open();
        //            using (var cmd = new SqlCommand("tisi_trx.usp_sel_transaccion", cn))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                model.nu_tran_ruta = 2;
        //                cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
        //                cmd.Parameters.AddWithValue("@vc_cod_comercio", model.vc_cod_comercio);
        //                cmd.Parameters.AddWithValue("@dt_fec_inicio", model.dt_fec_inicio);
        //                cmd.Parameters.AddWithValue("@dt_fec_final", model.dt_fec_final);

        //                UtilSql.iGet(cmd, model);
        //                SqlDataReader dr = cmd.ExecuteReader();

        //                var lm = new List<Sentinel_ResponseModel>();
        //                while (dr.Read())
        //                {
        //                    model_response = new Sentinel_ResponseModel();
        //                    if (UtilSql.Ec(dr, "NU_ID_TRX"))
        //                        model_response.id_transaccion = dr["NU_ID_TRX"].ToDecimal();
        //                    if (UtilSql.Ec(dr, "DT_FECHA"))
        //                        model_response.fecha = dr["DT_FECHA"].ToDateTime().Value.ToString("yyyy-MM-dd");
        //                    if (UtilSql.Ec(dr, "NU_ID_PRODUCTO"))
        //                        model_response.id_producto = dr["NU_ID_PRODUCTO"].ToDecimal();
        //                    if (UtilSql.Ec(dr, "VC_DESC_PRODUCTO"))
        //                        model_response.desc_producto = dr["VC_DESC_PRODUCTO"].ToString();
        //                    if (UtilSql.Ec(dr, "NU_PRECIO"))
        //                        model_response.precio = dr["NU_PRECIO"].ToDecimal();
        //                    lm.Add(model_response);
        //                }
        //                return lm;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }

        //    }
        //}


        public object get_validar_titular(string conexion, SentinelInfo info, Sentinel_InputModel.Consultado model)
        {
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
            TipoDocIdentidadModel tipodocidentidad_consultado = get_tipo_documento_codigo(con_sql, model.tipo_documento_consultado);
            if (tipodocidentidad_consultado.nu_id_tipo_doc_identidad <= 0)
            {
                //_logger.Error("idtrx: " + id_trx_hub + " / " + "El producto " + model.id_producto + " no existe");
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
                switch (rest.CodigoWS)
                {
                    case "1":
                        mensaje_error = "Usuario Incorrecto";
                        break;
                    case "2":
                        mensaje_error = "Servicio Inválido";
                        break;
                    case "3":
                        mensaje_error = "Documento inválido(No existe)";
                        break;
                    case "4":
                        mensaje_error = "No tiene autorización a ver dicho CPT";
                        break;
                    case "6":
                        mensaje_error = "El usuario no tiene permiso de consultar nuevos documentos";
                        break;
                    case "7":
                        mensaje_error = "El servicio está suspendido";
                        break;
                    case "8":
                        mensaje_error = "El usuario está suspendido";
                        break;
                    case "9":
                        mensaje_error = "El usuario está bloqueado";
                        break;
                    case "10":
                        mensaje_error = "El servicio no tiene disponible este producto";
                        break;
                    case "12":
                        mensaje_error = "Usuario no se encuentra en servicio";
                        break;
                    case "97":
                        mensaje_error = "Servicio no cuenta con acceso al web service";
                        break;
                    case "98":
                        mensaje_error = "Error en credenciales usuario o password";
                        break;
                    case "99":
                        mensaje_error = "Error en funcionamiento del web service";
                        break;
                }
                //response.nu_tran_stdo = 0;
                //response.vc_tran_codi = rest.CodigoWS;
                //response.tx_tran_mnsg = mensaje_error;
                response.codigo = rest.CodigoWS;
                response.mensaje = mensaje_error;

                return response;
            }

            //response.nu_tran_stdo = 1;
            response.codigo = "00";
            response.nombres = rest.Nombres;
            response.apellido_paterno = rest.ApellidoPaterno;
            response.apellido_materno = rest.ApellidoMaterno;

            return response;
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
                con_sql = new SqlConnection(conexion);

                if (!new EmailAddressAttribute().IsValid(model.email_consultante))
                {
                    //_logger.Error("idtrx: " + id_trx_hub + " / " + "El email " + model.email_consultante + " es incorrecto");
                    return UtilSql.sOutPutTransaccion("03", "El email es incorrecto");
                }

                if (!Regex.Match(model.id_producto, @"(^[0-9]+$)").Success)
                {
                    //_logger.Error("idtrx: " + id_trx_hub + " / " + "El id del producto " + model.id_producto + " debe ser numerico");
                    return UtilSql.sOutPutTransaccion("04", "El id del producto debe ser numerico");
                }

                con_sql.Open();
                DistribuidorModel distribuidor = get_distribuidor(con_sql, model.codigo_distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    //_logger.Error("idtrx: " + id_trx_hub + " / " + "El código de distribuidor " + model.codigo_distribuidor + " no existe");
                    return UtilSql.sOutPutTransaccion("05", "El código de distribuidor no existe");
                }

                ComercioModel comercio = get_comercio(con_sql, model.codigo_comercio, model.nombre_comercio, distribuidor.nu_id_distribuidor);

                ProductoModel producto = get_producto(con_sql, Convert.ToInt32(model.id_producto), distribuidor.nu_id_distribuidor);

                if (producto.nu_id_producto <= 0)
                {
                    //_logger.Error("idtrx: " + id_trx_hub + " / " + "El producto " + model.id_producto + " no existe");
                    return UtilSql.sOutPutTransaccion("06", "El producto no existe");
                }

                TipoDocIdentidadModel tipodocidentidad_consultante = get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultante));
                if (tipodocidentidad_consultante.nu_id_tipo_doc_identidad <= 0)
                {
                    //_logger.Error("idtrx: " + id_trx_hub + " / " + "El producto " + model.id_producto + " no existe");
                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento de consultante no existe.");
                }

                TipoDocIdentidadModel tipodocidentidad_consultado = get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultado));
                if (tipodocidentidad_consultado.nu_id_tipo_doc_identidad <= 0)
                {
                    //_logger.Error("idtrx: " + id_trx_hub + " / " + "El producto " + model.id_producto + " no existe");
                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento de consultado no existe.");
                }

                TipoDocIdentidadModel tipodocidentidad_PDV = get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_PDV));
                if (tipodocidentidad_PDV.nu_id_tipo_doc_identidad <= 0)
                {
                    //_logger.Error("idtrx: " + id_trx_hub + " / " + "El producto " + model.id_producto + " no existe");
                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento del PDV no existe.");
                }


                con_sql.Close();

                TransaccionModel trx = new TransaccionModel();
                trx.vc_cod_distribuidor = model.codigo_distribuidor;
                trx.vc_cod_comercio = model.codigo_comercio;
                trx.vc_cod_tipo_doc_sol = tipodocidentidad_consultante.vc_cod_tipo_doc_identidad;
                trx.vc_nro_doc_sol = model.numero_documento_consultante;
                trx.ch_dig_ver_sol = model.digito_verificador_consultante;
                trx.vc_telefono_sol = model.telefono_consultante;
                trx.vc_email_sol = model.email_consultante;
                trx.vc_tipo_comprobante = model.tipo_documento_facturacion;
                trx.vc_cod_tipo_doc_cpt = tipodocidentidad_consultado.vc_cod_tipo_doc_identidad;
                trx.vc_nro_doc_cpt = model.numero_documento_consultado;
                trx.vc_cod_producto = producto.vc_cod_producto;
                trx.vc_ruc = model.numero_ruc;
                trx.PDVTipoDoc = tipodocidentidad_PDV.vc_cod_tipo_doc_identidad;
                trx.PDVNroDoc = model.numero_documento_PDV;
                trx.PDVRazSocNom = model.razon_social_PDV;

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
                    return UtilSql.sOutPutTransaccion(encripta_rest.coderror.ToString(), mensaje_error);
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

                modelo.Gx_Key = info.Gx_Key;
                modelo.SDT_TitMas.Add(new ConsultaTitularDet()
                {
                    TipoDocCPT = trx.vc_cod_tipo_doc_cpt,
                    NroDocCPT = trx.vc_nro_doc_cpt,
                    TipoReporte = trx.vc_cod_producto == "G" ? "R" : trx.vc_cod_producto,
                    Pago = trx.vc_cod_producto == "G" ? "N" : "S"
                });

                con_sql.Open();

                //Variables BD
                var idtran = get_id_transaccion(con_sql);
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
                    cmd.Parameters.AddWithValue("@vc_cod_distribuidor", trx.vc_cod_distribuidor);
                    cmd.Parameters.AddWithValue("@vc_cod_comercio", trx.vc_cod_comercio);
                    cmd.Parameters.AddWithValue("@vc_cod_producto", trx.vc_cod_producto);
                    cmd.Parameters.AddWithValue("@vc_cod_tipo_doc_sol", trx.vc_cod_tipo_doc_sol);
                    cmd.Parameters.AddWithValue("@vc_nro_doc_sol", trx.vc_nro_doc_sol);
                    cmd.Parameters.AddWithValue("@ch_dig_ver_sol", trx.ch_dig_ver_sol);
                    cmd.Parameters.AddWithValue("@vc_email_sol", trx.vc_email_sol);
                    cmd.Parameters.AddWithValue("@vc_telefono_sol", trx.vc_telefono_sol);
                    cmd.Parameters.AddWithValue("@vc_cod_tipo_doc_cpt", trx.vc_cod_tipo_doc_cpt);
                    cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", trx.vc_nro_doc_cpt);
                    cmd.Parameters.AddWithValue("@vc_id_ref_trx", "");
                    cmd.Parameters.AddWithValue("@vc_tipo_comprobante", trx.vc_tipo_comprobante);
                    cmd.Parameters.AddWithValue("@vc_ruc", trx.vc_ruc);

                    UtilSql.iIns(cmd, trx);
                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                    {
                        tran_sql.Rollback();
                        _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                    }
                    trx.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();
                    modelo.ReferenceCode = trx.nu_id_trx_app.ToString();
                    var a = JsonConvert.SerializeObject(modelo);

                    //PRODUCCION
                    ConsultaTitularRest response = api.ConsultaTitularFacturacion(modelo).Result;

                    //QA
                    //ConsultaTitularRest response = new ConsultaTitularRest();
                    //response.CodigoWS = "0";
                    //response.ID_Transaccion = "TestOK";

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
                                _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                                return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                            }
                            cmd.Parameters["@vc_tran_codi"].Value = cmd_upd.Parameters["@vc_tran_codi"].Value;
                        }

                        tran_sql.Commit();

                        object _info = new object();

                        _info = new
                        {
                            codigo = "00",
                            mensaje = "Operación exitosa, la información le llegará al correo electrónico registrado.",
                            nro_transaccion = id_trans_global
                        };
                        con_sql.Close();

                        return _info;
                                               
                    }

                    else
                    {
                        tran_sql.Rollback();
                        con_sql.Close();

                        TransaccionModel tm = new TransaccionModel();
                        tm.nu_id_trx = Convert.ToInt32(id_trans_global);
                        tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                        tm.nu_id_comercio = comercio.nu_id_comercio;
                        tm.dt_fecha = DateTime.Now;
                        tm.nu_id_producto = producto.nu_id_producto;
                        tm.nu_precio = producto.nu_precio ?? 0;

                        switch (response.CodigoWS)
                        {
                            case "1":
                                mensaje_error = "Usuario Incorrecto";
                                break;
                            case "2":
                                mensaje_error = "Servicio Inválido";
                                break;
                            case "3":
                                mensaje_error = "Documento inválido(No existe)";
                                break;
                            case "4":
                                mensaje_error = "No tiene autorización a ver dicho CPT";
                                break;
                            case "6":
                                mensaje_error = "El usuario no tiene permiso de consultar nuevos documentos";
                                break;
                            case "7":
                                mensaje_error = "El servicio está suspendido";
                                break;
                            case "8":
                                mensaje_error = "El usuario está suspendido";
                                break;
                            case "9":
                                mensaje_error = "El usuario está bloqueado";
                                break;
                            case "10":
                                mensaje_error = "El servicio no tiene disponible este producto";
                                break;
                            case "12":
                                mensaje_error = "Usuario no se encuentra en servicio";
                                break;
                            case "30":
                                mensaje_error = "No puede consultar CPT por no tener consultas disponibles";
                                break;
                            case "40":
                                mensaje_error = "Ingrese el tipo y número de documento del solicitante";
                                break;
                            case "41":
                                mensaje_error = "Ingrese los datos de la persona o empresa a consultar";
                                break;
                            case "42":
                                mensaje_error = "El correo ingresado no es valido";
                                break;
                            case "43":
                                mensaje_error = "No se puede consultar, hay consulta(s) disponible(s) que ya fue(ron) asignada(s)";
                                break;
                            case "44":
                                mensaje_error = "El servicio ya no cuenta con consultas disponibles";
                                break;
                            case "45":
                                mensaje_error = "El tiempo de duración del paquete de consultas ha vencido";
                                break;
                            case "46":
                                mensaje_error = "El usuario no cuenta con consultas disponibles";
                                break;
                            case "49":
                                mensaje_error = "No se enviaron los Términos y Condiciones";
                                break;
                            case "50":
                                mensaje_error = "Se ha realizado el envío de los Términos y Condiciones al correo del solicitante.Se ha guardado su consulta";
                                break;
                            case "53":
                                mensaje_error = "No tiene Información SUNAT";
                                break;
                            case "54":
                                mensaje_error = "No tiene permiso de usar el WebService";
                                break;
                            case "55":
                                mensaje_error = "Solicitante Inválido (datos incorrectos)";
                                break;
                            case "56":
                                mensaje_error = "Dígito Verificador Inválido (sólo aplica para DNI";
                                break;
                            case "57":
                                mensaje_error = "Ha superado la cantidad máxima de consultas gratuitas de su Nro. Doc.";
                                break;
                            case "58":
                                mensaje_error = "ReferenceCode inválido, ya se encuentra registrado o campo vacío";
                                break;
                            case "59":
                                mensaje_error = "Tipo de Comprobante Inválido";
                                break;
                            case "60":
                                mensaje_error = "Número de documento de facturación inválido (No existe)";
                                break;
                            case "99":
                                mensaje_error = "Error en funcionamiento del web service";
                                break;
                        }

                        if (response.CodigoWS == null)
                            tm.vc_cod_error = "";
                        else
                            tm.vc_cod_error = response.CodigoWS;

                        if (response.CodigoWS == null)
                            tm.vc_desc_error = "";
                        else
                            tm.vc_desc_error = mensaje_error;

                        tm.vc_desc_tipo_error = "";


                        SqlTransaction tran_sql_error = null;
                        con_sql.Open();

                        tran_sql_error = con_sql.BeginTransaction();
                        ins_bd = true;

                        cmd = insTransaccionError(con_sql, tran_sql_error, tm);

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                        {
                            tran_sql.Rollback();
                            _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                            return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                        }

                        tran_sql_error.Commit();
                        _logger.Error("idtrx: " + id_trx_hub + " / " + "Transaccion error: " + tm.vc_cod_error + "/" + tm.vc_desc_tipo_error + "/" + tm.vc_desc_error);
                        return UtilSql.sOutPutTransaccion(tm.vc_cod_error, tm.vc_desc_error);
                        //return UtilSql.sOutPutTransaccion("06", "Ocurrio un error en la transaccion.");


                    }

                }
            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }

                _logger.Error("idtrx: " + id_trx_hub + " / " + "id_transaccion: " + id_trans_global + " / " + ex, ex.Message);
                return UtilSql.sOutPutTransaccion("500", ex.Message);
                //return UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }


        }

        private DistribuidorModel get_distribuidor(SqlConnection cn, string vc_cod_distribuidor)
        {
            DistribuidorModel model = new DistribuidorModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_distribuidor", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 2;
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
                    if (UtilSql.Ec(dr, "vc_desc_distribuidor"))
                        model.vc_desc_distribuidor = dr["vc_desc_distribuidor"].ToString();
                    if (UtilSql.Ec(dr, "vc_zip_code"))
                        model.vc_zip_code = dr["vc_zip_code"].ToString();
                }
            }
            return model;
        }
        private ComercioModel get_comercio(SqlConnection cn, string vc_cod_comercio, string vc_nombre_comercio, int nu_id_distribuidor)
        {
            ComercioModel model = new ComercioModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_comercio", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 1;
                model.nu_id_distribuidor = nu_id_distribuidor;
                model.vc_cod_comercio = vc_cod_comercio;
                model.vc_nombre_comercio = vc_nombre_comercio;
                model.vc_tran_usua_regi = "API";
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
            using (var cmd = new SqlCommand("tisi_global.usp_get_distribuidor_producto", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 1;
                model.nu_id_producto = nu_id_producto;
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_convenio", nu_id_convenio);
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
                    if (UtilSql.Ec(dr, "nu_precio"))
                        _result.nu_precio = dr["nu_precio"].ToDecimal();

                }
            }
            return _result;
        }
        private TipoDocIdentidadModel get_tipo_documento(SqlConnection cn, int nu_id_tipo_doc_identidad)
        {
            TipoDocIdentidadModel _result = new TipoDocIdentidadModel();
            TipoDocIdentidadModel model = new TipoDocIdentidadModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_tipo_doc_identidad", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 2;
                model.nu_id_tipo_doc_identidad = nu_id_tipo_doc_identidad;
                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_identidad", nu_id_tipo_doc_identidad);
                cmd.Parameters.AddWithValue("@nu_id_convenio", nu_id_convenio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())

                {
                    if (UtilSql.Ec(dr, "nu_id_tipo_doc_identidad"))
                        _result.nu_id_tipo_doc_identidad = Convert.ToInt32(dr["nu_id_tipo_doc_identidad"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_tipo_doc_identidad"))
                        _result.vc_cod_tipo_doc_identidad = dr["vc_cod_tipo_doc_identidad"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_tipo_doc_identidad"))
                        _result.vc_desc_tipo_doc_identidad = dr["vc_desc_tipo_doc_identidad"].ToString();

                }
            }
            return _result;
        }
        private TipoDocIdentidadModel get_tipo_documento_codigo(SqlConnection cn, string vc_cod_tipo_doc_identidad)
        {
            TipoDocIdentidadModel _result = new TipoDocIdentidadModel();
            TipoDocIdentidadModel model = new TipoDocIdentidadModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_tipo_doc_identidad", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 3;
                model.vc_cod_tipo_doc_identidad = vc_cod_tipo_doc_identidad;
                cmd.Parameters.AddWithValue("@vc_cod_tipo_doc_identidad", model.vc_cod_tipo_doc_identidad);
                cmd.Parameters.AddWithValue("@nu_id_convenio", nu_id_convenio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())

                {
                    if (UtilSql.Ec(dr, "nu_id_tipo_doc_identidad"))
                        _result.nu_id_tipo_doc_identidad = Convert.ToInt32(dr["nu_id_tipo_doc_identidad"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_tipo_doc_identidad"))
                        _result.vc_cod_tipo_doc_identidad = dr["vc_cod_tipo_doc_identidad"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_tipo_doc_identidad"))
                        _result.vc_desc_tipo_doc_identidad = dr["vc_desc_tipo_doc_identidad"].ToString();

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

        private static SqlCommand insTransaccionError(SqlConnection cn, SqlTransaction tran, TransaccionModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_transaccion_error", cn, tran))
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
    }
}
