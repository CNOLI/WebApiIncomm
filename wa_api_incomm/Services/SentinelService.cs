using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
        public object sel_tipo_documento_identidad(string conexion, Sentinel_InputModel.Busqueda input)
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
                                model_response.id_tipo_doc_identidad = dr["NU_ID_TIPO_DOC_IDENTIDAD"].ToDecimal();
                            if (UtilSql.Ec(dr, "VC_COD_TIPO_DOC_IDENTIDAD"))
                                model_response.cod_tipo_doc_identidad = dr["VC_COD_TIPO_DOC_IDENTIDAD"].ToString();
                            if (UtilSql.Ec(dr, "VC_DESC_TIPO_DOC_IDENTIDAD"))
                                model_response.desc_tipo_doc_identidad = dr["VC_DESC_TIPO_DOC_IDENTIDAD"].ToString();
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
        public object sel_producto(string conexion, Sentinel_InputModel.Busqueda input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    Sentinel_ResponseModel model_response = new Sentinel_ResponseModel();
                    ProductoModel model = new ProductoModel();
                    model.nu_id_convenio = nu_id_convenio;
                    model.vc_cod_distribuidor = input.cod_distribuidor;
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
                                model_response.cod_producto = dr["VC_COD_PRODUCTO"].ToString();
                            if (UtilSql.Ec(dr, "VC_DESC_PRODUCTO"))
                                model_response.desc_producto = dr["VC_DESC_PRODUCTO"].ToString();
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


        public object get_validar_titular(SentinelInfo info, Sentinel_InputModel.Consultado model)
        {
            SentinelApi api = new SentinelApi();
            Encripta encripta = null;
            EncriptaRest encripta_rest = null;
            ConsultaPersona modelo = new ConsultaPersona();
            ConsultaPersonaRest rest = new ConsultaPersonaRest();
            Sentinel_ResponseModel response = new Sentinel_ResponseModel();
            string mensaje_error = "";

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
                rest.CodigoWS = mensaje_error + " - Usuario";
                return rest;
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
                rest.CodigoWS = mensaje_error + " - Contraseña";
                return rest;
            }
            modelo.Gx_PasEnc = encripta_rest.encriptado;

            modelo.Gx_Key = info.Gx_Key;
            modelo.TipoDocSol = model.cod_tip_doc_consultado;
            modelo.NroDocSol = model.nro_doc_consultado;

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
                response.nu_tran_stdo = 0;
                response.vc_tran_codi = rest.CodigoWS;
                response.tx_tran_mnsg = mensaje_error;
                return response;
            }

            response.nu_tran_stdo = 1;
            response.nombres = rest.Nombres;
            response.apellido_paterno = rest.ApellidoPaterno;
            response.apellido_materno = rest.ApellidoMaterno;

            return response;
        }
        public object ins_transaccion(string conexion, SentinelInfo info, Sentinel_InputModel.Ins_Transaccion input)
        {
            SentinelApi api = new SentinelApi();
            Encripta encripta = null;
            EncriptaRest encripta_rest = null;
            ConsultaTitularFac modelo = new ConsultaTitularFac();
            string mensaje_error = "";

            SqlTransaction tran = null;
            SqlTransaction tran2 = null;
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    TransaccionModel model = new TransaccionModel();
                    model.vc_cod_distribuidor   = input.cod_distribuidor;
                    model.vc_cod_comercio       = input.cod_comercio;
                    model.vc_cod_tipo_doc_sol   = input.cod_tip_doc_solicitante;
                    model.vc_nro_doc_sol        = input.nro_doc_solicitante;
                    model.ch_dig_ver_sol        = input.dig_ver_solicitante;
                    model.vc_email_sol          = input.email_solicitante;
                    model.vc_telefono_sol       = input.tel_solicitante;
                    model.vc_tipo_comprobante   = input.tip_doc_facturacion;
                    model.vc_cod_tipo_doc_cpt   = input.cod_tip_doc_consultado;
                    model.vc_nro_doc_cpt        = input.nro_doc_consultado;
                    model.vc_cod_producto       = input.cod_producto;
                    model.vc_ruc                = input.nro_ruc;

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
                        return Utilitarios.JsonErrorTran(new Exception(mensaje_error + " - Usuario"));
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
                        return Utilitarios.JsonErrorTran(new Exception(mensaje_error + " - Usuario"));
                    }
                    modelo.Gx_PasEnc = encripta_rest.encriptado;

                    modelo.TipoDocSol           = model.vc_cod_tipo_doc_sol;
                    modelo.NroDocSol            = model.vc_nro_doc_sol;
                    modelo.DigVerSol            = model.ch_dig_ver_sol;
                    modelo.CorreoSol            = model.vc_email_sol;
                    modelo.TelefSol             = model.vc_telefono_sol;
                    modelo.TipoDocComprobante   = model.vc_tipo_comprobante;
                    modelo.NroDocumentoFAC      = model.vc_tipo_comprobante == "FAC" ? model.vc_ruc : "";

                    modelo.Gx_Key = info.Gx_Key;
                    modelo.SDT_TitMas.Add(new ConsultaTitularDet()
                    {
                        TipoDocCPT      = model.vc_cod_tipo_doc_cpt,
                        NroDocCPT       = model.vc_nro_doc_cpt,
                        TipoReporte     = model.vc_cod_producto == "G" ? "R" : model.vc_cod_producto,
                        Pago            = model.vc_cod_producto == "G" ? "N" : "S"
                    });

                    //graba primero en BD
                    cn.Open();

                    tran = cn.BeginTransaction();

                    using (var cmd = new SqlCommand("tisi_global.usp_ins_transaccion_sentinel", cn, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                        cmd.Parameters.AddWithValue("@vc_cod_comercio", model.vc_cod_comercio);
                        cmd.Parameters.AddWithValue("@vc_cod_producto", model.vc_cod_producto);
                        cmd.Parameters.AddWithValue("@vc_cod_tipo_doc_sol", model.vc_cod_tipo_doc_sol);
                        cmd.Parameters.AddWithValue("@vc_nro_doc_sol", model.vc_nro_doc_sol);
                        cmd.Parameters.AddWithValue("@ch_dig_ver_sol", model.ch_dig_ver_sol);
                        cmd.Parameters.AddWithValue("@vc_email_sol", model.vc_email_sol);
                        cmd.Parameters.AddWithValue("@vc_telefono_sol", model.vc_telefono_sol);
                        cmd.Parameters.AddWithValue("@vc_cod_tipo_doc_cpt", model.vc_cod_tipo_doc_cpt);
                        cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", model.vc_nro_doc_cpt);
                        cmd.Parameters.AddWithValue("@vc_id_ref_trx", "XX");
                        cmd.Parameters.AddWithValue("@vc_tipo_comprobante", model.vc_tipo_comprobante);
                        cmd.Parameters.AddWithValue("@vc_ruc", model.vc_ruc);
                        UtilSql.iIns(cmd, model);
                        cmd.ExecuteNonQuery();

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                        {
                            tran.Rollback();
                            return UtilSql.sOutPut(cmd);
                        }
                        model.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();
                        modelo.ReferenceCode = model.nu_id_trx_app.ToString();

                        //ConsultaTitularRest rest = api.ConsultaTitularFacturacion(modelo).Result;
                        //if (rest.CodigoWS != "0")
                        //{

                        //    switch (rest.CodigoWS)
                        //    {
                        //        case "1":
                        //            mensaje_error = "Usuario Incorrecto";
                        //            break;
                        //        case "2":
                        //            mensaje_error = "Servicio Inválido";
                        //            break;
                        //        case "3":
                        //            mensaje_error = "Documento inválido(No existe)";
                        //            break;
                        //        case "4":
                        //            mensaje_error = "No tiene autorización a ver dicho CPT";
                        //            break;
                        //        case "6":
                        //            mensaje_error = "El usuario no tiene permiso de consultar nuevos documentos";
                        //            break;
                        //        case "7":
                        //            mensaje_error = "El servicio está suspendido";
                        //            break;
                        //        case "8":
                        //            mensaje_error = "El usuario está suspendido";
                        //            break;
                        //        case "9":
                        //            mensaje_error = "El usuario está bloqueado";
                        //            break;
                        //        case "10":
                        //            mensaje_error = "El servicio no tiene disponible este producto";
                        //            break;
                        //        case "12":
                        //            mensaje_error = "Usuario no se encuentra en servicio";
                        //            break;
                        //        case "30":
                        //            mensaje_error = "No puede consultar CPT por no tener consultas disponibles";
                        //            break;
                        //        case "40":
                        //            mensaje_error = "Ingrese el tipo y número de documento del solicitante";
                        //            break;
                        //        case "41":
                        //            mensaje_error = "Ingrese los datos de la persona o empresa a consultar";
                        //            break;
                        //        case "42":
                        //            mensaje_error = "El correo ingresado no es valido";
                        //            break;
                        //        case "43":
                        //            mensaje_error = "No se puede consultar, hay consulta(s) disponible(s) que ya fue(ron) asignada(s)";
                        //            break;
                        //        case "44":
                        //            mensaje_error = "El servicio ya no cuenta con consultas disponibles";
                        //            break;
                        //        case "45":
                        //            mensaje_error = "El tiempo de duración del paquete de consultas ha vencido";
                        //            break;
                        //        case "46":
                        //            mensaje_error = "El usuario no cuenta con consultas disponibles";
                        //            break;
                        //        case "49":
                        //            mensaje_error = "No se enviaron los Términos y Condiciones";
                        //            break;
                        //        case "50":
                        //            mensaje_error = "Se ha realizado el envío de los Términos y Condiciones al correo del solicitante.Se ha guardado su consulta";
                        //            break;
                        //        case "53":
                        //            mensaje_error = "No tiene Información SUNAT";
                        //            break;
                        //        case "54":
                        //            mensaje_error = "No tiene permiso de usar el WebService";
                        //            break;
                        //        case "55":
                        //            mensaje_error = "Solicitante Inválido (datos incorrectos)";
                        //            break;
                        //        case "56":
                        //            mensaje_error = "Dígito Verificador Inválido (sólo aplica para DNI";
                        //            break;
                        //        case "57":
                        //            mensaje_error = "Ha superado la cantidad máxima de consultas gratuitas de su Nro. Doc.";
                        //            break;
                        //        case "58":
                        //            mensaje_error = "ReferenceCode inválido, ya se encuentra registrado o campo vacío";
                        //            break;
                        //        case "59":
                        //            mensaje_error = "Tipo de Comprobante Inválido";
                        //            break;
                        //        case "60":
                        //            mensaje_error = "Número de documento de facturación inválido (No existe)";
                        //            break;
                        //        case "99":
                        //            mensaje_error = "Error en funcionamiento del web service";
                        //            break;
                        //    }


                        //    tran.Rollback();
                        //    return Utilitarios.JsonErrorTran(new Exception(mensaje_error));
                        //}
                        //model.vc_id_ref_trx = rest.ID_Transaccion.ToString();
                        
                        model.vc_id_ref_trx = "TestOK";

                        using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", cn, tran))
                        {
                            cmd_upd.CommandType = CommandType.StoredProcedure;
                            cmd_upd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx_app);
                            cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", model.vc_id_ref_trx);
                            UtilSql.iUpd(cmd_upd, model);
                            cmd_upd.ExecuteNonQuery();
                            if (cmd_upd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                            {
                                tran.Rollback();
                                return UtilSql.sOutPut(cmd_upd);
                            }
                            cmd.Parameters["@vc_tran_codi"].Value = cmd_upd.Parameters["@vc_tran_codi"].Value;
                        }

                        tran.Commit();
                        return UtilSql.sOutPut(cmd);
                    }
                }
                catch (Exception ex)
                {
                    return Utilitarios.JsonErrorTran(ex);
                }
            }
        }


    }
}
