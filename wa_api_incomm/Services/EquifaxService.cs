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
                GlobalService global_service = new GlobalService();

                con_sql = new SqlConnection(conexion);

                con_sql.Open();

                TrxHubModel trx = new TrxHubModel();
                trx.codigo_distribuidor = model.codigo_distribuidor;
                trx.codigo_comercio = model.codigo_comercio;
                trx.nombre_comercio = model.nombre_comercio;
                trx.nro_telefono = "";
                trx.email = model.email_consultante;
                trx.id_producto = model.id_producto;

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

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Inicio de transaccion");

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

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El código de distribuidor " + model.codigo_distribuidor + " no existe");
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
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El producto " + model.id_producto + " no existe");
                    return UtilSql.sOutPutTransaccion("06", "El producto no existe");
                }


                TipoDocIdentidadModel tipodocidentidad_consultante = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultante), nu_id_convenio);
                if (tipodocidentidad_consultante.nu_id_tipo_doc_identidad <= 0)
                {
                    return UtilSql.sOutPutTransaccion("XX", "El tipo de documento de consultante no existe.");
                }

                TipoDocIdentidadModel tipodocidentidad_consultado = global_service.get_tipo_documento(con_sql, Convert.ToInt32(model.tipo_documento_consultado), nu_id_convenio);
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


                e_input.documentType = Convert.ToInt32(tipodocidentidad_consultante.vc_cod_tipo_doc_identidad);
                e_input.documentNumber = model.numero_documento_consultante;
                e_input.email = model.email_consultante;
                e_input.verifierDigit = model.digito_verificador_consultante;
                e_input.documentTypeThird = Convert.ToInt32(tipodocidentidad_consultado.vc_cod_tipo_doc_identidad);
                e_input.documentNumberThird = model.numero_documento_consultado;

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
                
                model_sql.vc_tran_usua_regi = "API";

                using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_equifax", con_sql, tran_sql))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nu_id_trx", id_trans_global);
                    cmd.Parameters.AddWithValue("@nu_id_trx_hub", id_trx_hub);
                    cmd.Parameters.AddWithValue("@nu_id_distribuidor", distribuidor.nu_id_distribuidor);
                    cmd.Parameters.AddWithValue("@nu_id_comercio", comercio.nu_id_comercio);
                    cmd.Parameters.AddWithValue("@nu_id_producto", producto.nu_id_producto);
                    cmd.Parameters.AddWithValue("@nu_id_tipo_doc_sol", tipodocidentidad_consultante.nu_id_tipo_doc_identidad);
                    cmd.Parameters.AddWithValue("@vc_nro_doc_sol", model.numero_documento_consultante);
                    cmd.Parameters.AddWithValue("@ch_dig_ver_sol", model.digito_verificador_consultante);
                    cmd.Parameters.AddWithValue("@vc_email_sol", model.email_consultante);
                    cmd.Parameters.AddWithValue("@nu_id_tipo_doc_cpt", tipodocidentidad_consultado.nu_id_tipo_doc_identidad);
                    cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", model.numero_documento_consultado);

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
                    Generar_Reporte_Response response = client.Generar_Reporte(e_input, _logger, id_trx_hub).Result;

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
                        _logger.Information("idtrx: " + id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());

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
                        tm.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                        tm.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                        tm.nu_id_comercio = comercio.nu_id_comercio;
                        tm.dt_fecha = DateTime.Now;
                        tm.nu_id_producto = producto.nu_id_producto;
                        tm.nu_precio = producto.nu_precio ?? 0;
                        tm.nu_id_tipo_moneda_vta = 1;
                        tm.vc_numero_servicio = "";
                        tm.vc_tran_usua_regi = "API";

                        if (response.code == null)
                            tm.vc_cod_error = "";
                        else
                            tm.vc_cod_error = response.code;

                        if (response.message == null)
                            tm.vc_desc_error = "";
                        else
                            tm.vc_desc_error = response.message + " - " + string.Join("; ", response.errors);

                        tm.vc_desc_tipo_error = "CONVENIO";


                        SqlTransaction tran_sql_error = null;
                        con_sql.Open();

                        tran_sql_error = con_sql.BeginTransaction();
                        ins_bd = true;

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
