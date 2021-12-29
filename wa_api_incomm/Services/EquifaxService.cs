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
using static wa_api_incomm.Models.Equifax_InputModel;
using static wa_api_incomm.Models.Equifax_ResponseModel;
using static wa_api_incomm.Models.EquifaxModel;
using wa_api_incomm.Services;
using System.Text.RegularExpressions;

namespace wa_api_incomm.Services
{
    public class EquifaxService : IEquifaxService
    {
        public int nu_id_convenio = 4;
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();


        private readonly Serilog.ILogger _logger;

        public EquifaxService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object sel_tipo_documento_identidad(string conexion, Equifax_InputModel.Busqueda_Equifax_Input input)
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
                        model.nu_tran_ruta = 3;

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
        public object GenerarReporte(string conexion, GenerarReporte model)
        {
            bool ins_bd = false;
            string id_trx_hub = "";
            string id_trans_global = "";
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string codigo_error = "";
            string mensaje_error = "";

            EquifaxModel model_sql = new EquifaxModel();

            Generar_Reporte_Input e_input = new Generar_Reporte_Input();
            Generar_Reporte_Response e_response = new Generar_Reporte_Response();
            try
            {
                con_sql = new SqlConnection(conexion);
                con_sql.Open();

                if (!new EmailAddressAttribute().IsValid(model.email_consultante))
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El email " + model.email_consultante + " es incorrecto");
                    return UtilSql.sOutPutTransaccion("03", "El email es incorrecto");
                }

                if (!Regex.Match(model.id_producto, @"(^[0-9]+$)").Success)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El id del producto " + model.id_producto + " debe ser numerico");
                    return UtilSql.sOutPutTransaccion("04", "El id del producto debe ser numerico");
                }
                if (string.IsNullOrEmpty(model.digito_verificador_consultante))
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + " El digito verificador es obligatorio");
                    return UtilSql.sOutPutTransaccion("XX", "El digito verificador es obligatorio");
                }

                DistribuidorModel distribuidor = get_distribuidor(con_sql, model.codigo_distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El código de distribuidor " + model.codigo_distribuidor + " no existe");
                    return UtilSql.sOutPutTransaccion("05", "El código de distribuidor no existe");
                }

                ComercioModel comercio = get_comercio(con_sql, model.codigo_comercio, model.nombre_comercio, distribuidor.nu_id_distribuidor);

                ProductoModel producto = get_producto(con_sql, Convert.ToInt32(model.id_producto), distribuidor.nu_id_distribuidor);

                if (producto.nu_id_producto <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El producto " + model.id_producto + " no existe");
                    return UtilSql.sOutPutTransaccion("06", "El producto no existe");
                }


                TipoDocIdentidadModel tipodocidentidad_consultante = get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultante));
                if (tipodocidentidad_consultante.nu_id_tipo_doc_identidad <= 0)
                {
                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento de consultante no existe.");
                }

                TipoDocIdentidadModel tipodocidentidad_consultado = get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultado));
                if (tipodocidentidad_consultado.nu_id_tipo_doc_identidad <= 0)
                {
                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento de consultado no existe.");
                }

                if (string.IsNullOrEmpty(model.digito_verificador_consultante))
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + " El digito verificador es obligatorio");
                    return UtilSql.sOutPutTransaccion("XX", "El digito verificador es obligatorio");
                }

                con_sql.Close();

                con_sql.Open();

                TrxHubModel trx = new TrxHubModel();
                trx.codigo_distribuidor = model.codigo_distribuidor;
                trx.codigo_comercio = model.codigo_comercio;
                trx.nombre_comercio = model.nombre_comercio;
                trx.nro_telefono = "";
                trx.email = model.email_consultante;
                trx.id_producto = model.id_producto;

                cmd = insTrxhub(con_sql, trx);
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    tran_sql.Rollback();
                    _logger.Error(cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                    return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                }

                id_trx_hub = cmd.Parameters["@nu_tran_pkey"].Value.ToString();

                con_sql.Close();

                e_input.documentType = Convert.ToInt32(tipodocidentidad_consultante.vc_cod_tipo_doc_identidad);
                e_input.documentNumber = model.numero_documento_consultante;
                e_input.email = model.email_consultante;
                e_input.verifierDigit = model.digito_verificador_consultante;
                e_input.documentTypeThird = Convert.ToInt32(tipodocidentidad_consultado.vc_cod_tipo_doc_identidad);
                e_input.documentNumberThird = model.numero_documento_consultado;

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

                using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_equifax", con_sql, tran_sql))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nu_id_trx", id_trans_global);
                    cmd.Parameters.AddWithValue("@vc_cod_distribuidor", distribuidor.vc_cod_distribuidor);
                    cmd.Parameters.AddWithValue("@vc_cod_comercio", comercio.vc_cod_comercio);
                    cmd.Parameters.AddWithValue("@vc_cod_producto", producto.vc_cod_producto);
                    cmd.Parameters.AddWithValue("@vc_cod_tipo_doc_sol", model.tipo_documento_consultante);
                    cmd.Parameters.AddWithValue("@vc_nro_doc_sol", model.numero_documento_consultante);
                    cmd.Parameters.AddWithValue("@ch_dig_ver_sol", model.digito_verificador_consultante);
                    cmd.Parameters.AddWithValue("@vc_email_sol", model.email_consultante);
                    cmd.Parameters.AddWithValue("@vc_cod_tipo_doc_cpt", model.tipo_documento_consultado);
                    cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", model.numero_documento_consultado);
                    cmd.Parameters.AddWithValue("@vc_id_ref_trx", "");

                    UtilSql.iIns(cmd, model_sql);
                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                    {
                        tran_sql.Rollback();
                        _logger.Error("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                        return UtilSql.sOutPutTransaccion("99", "Error en base de datos");
                    }
                    model_sql.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();

                    EquifaxApi client = new EquifaxApi();
                    Generar_Reporte_Response response = client.Generar_Reporte(e_input).Result;

                    if (response.code == "201")
                    {
                        model_sql.vc_id_ref_trx = response.data.ToString();

                        using (var cmd_upd = new SqlCommand("tisi_trx.usp_upd_transacciones_trx_ref", con_sql, tran_sql))
                        {
                            cmd_upd.CommandType = CommandType.StoredProcedure;
                            cmd_upd.Parameters.AddWithValue("@nu_id_trx", model_sql.nu_id_trx_app);
                            cmd_upd.Parameters.AddWithValue("@vc_id_ref_trx", model_sql.vc_id_ref_trx);
                            UtilSql.iUpd(cmd_upd, model_sql);
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

                        object info = new object();

                        info = new
                        {
                            codigo = "00",
                            mensaje = "Operación exitosa, la información le llegará al correo electrónico registrado.",
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
                        tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                        tm.nu_id_comercio = comercio.nu_id_comercio;
                        tm.dt_fecha = DateTime.Now;
                        tm.nu_id_producto = producto.nu_id_producto;
                        tm.nu_precio = producto.nu_precio ?? 0;

                        if (response.code == null)
                            tm.vc_cod_error = "";
                        else
                            tm.vc_cod_error = response.code;

                        if (response.message == null)
                            tm.vc_desc_error = "";
                        else
                            tm.vc_desc_error = response.message + " - " + string.Join("; ", response.errors);
                                               
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
