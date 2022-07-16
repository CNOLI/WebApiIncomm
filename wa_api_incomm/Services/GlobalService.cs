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
    public class GlobalService
    {
        public DistribuidorModel get_distribuidor(SqlConnection cn, DistribuidorModel model)
        {
            using (var cmd = new SqlCommand("tisi_global.usp_get_distribuidor", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 2;
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
                    if (UtilSql.Ec(dr, "vc_ruc"))
                        model.vc_ruc = dr["vc_ruc"].ToString();
                    if (UtilSql.Ec(dr, "vc_nombre_contacto"))
                        model.vc_nombre_contacto = dr["vc_nombre_contacto"].ToString();
                    if (UtilSql.Ec(dr, "vc_email_contacto"))
                        model.vc_email_contacto = dr["vc_email_contacto"].ToString();
                    if (UtilSql.Ec(dr, "vc_celular_contacto"))
                        model.vc_celular_contacto = dr["vc_celular_contacto"].ToString();
                    if (UtilSql.Ec(dr, "nu_id_comercio"))
                        model.nu_id_comercio = dr["nu_id_comercio"].ToInt();
                    if (UtilSql.Ec(dr, "nu_seg_encolamiento"))
                        model.nu_seg_encolamiento = dr["nu_seg_encolamiento"].ToInt();
                }
            }
            return model;
        }
        public ComercioModel get_comercio(SqlConnection cn, string vc_cod_comercio, string vc_nombre_comercio, int nu_id_distribuidor)
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
        public ComercioModel get_comercio_busqueda(SqlConnection cn, string vc_cod_comercio, int nu_id_distribuidor)
        {
            ComercioModel model = new ComercioModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_comercio", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 2;
                model.nu_id_distribuidor = nu_id_distribuidor;
                model.vc_cod_comercio = vc_cod_comercio;
                model.vc_tran_usua_regi = "API";
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@vc_cod_comercio", model.vc_cod_comercio);
                cmd.Parameters.AddWithValue("@vc_nombre_comercio", "");
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
        public ProductoModel get_producto(SqlConnection cn, ProductoModel model)
        {
            ProductoModel _result = new ProductoModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_distribuidor_producto", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 1;
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
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
                    if (UtilSql.Ec(dr, "nu_id_convenio"))
                        _result.nu_id_convenio = dr["nu_id_convenio"].ToInt();
                    if (UtilSql.Ec(dr, "bi_envio_sms"))
                        _result.bi_envio_sms = dr["bi_envio_sms"].ToBool();
                    if (UtilSql.Ec(dr, "bi_envio_email"))
                        _result.bi_envio_email = dr["bi_envio_email"].ToBool();
                    if (UtilSql.Ec(dr, "bi_doc_dinamico"))
                        _result.bi_doc_dinamico = dr["bi_doc_dinamico"].ToBool();

                }
            }
            return _result;
        }
        public TipoDocIdentidadModel get_tipo_documento(SqlConnection cn, int nu_id_tipo_doc_identidad, int nu_id_convenio)
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
        public TipoDocIdentidadModel get_tipo_documento_codigo(SqlConnection cn, string vc_cod_tipo_doc_identidad, int nu_id_convenio)
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
        public Int64 get_id_transaccion(SqlConnection cn)
        {
            Int64 r = 0;
            ConvenioModel model = new ConvenioModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_id_transaccion", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (UtilSql.Ec(dr, "id_trans"))
                        r = Convert.ToInt64(dr["id_trans"].ToString());
                }
            }
            return r;
        }
        public string get_mensaje_error(int nu_id_convenio, string codigo)
        {
            string mensaje_error = "";
            switch (nu_id_convenio)
            {
                case 1:
                    break;
                case 2:
                    switch (codigo)
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
                        case "61":
                            mensaje_error = "El reporte Flash sólo está disponible para consultas personales";
                            break;
                        case "62":
                            mensaje_error = "El tipo de reporte ingresado no es válido";
                            break;
                        case "63":
                            mensaje_error = "Ha superado la cantidad máxima de consultas gratuitas en el Mes del Servicio";
                            break;
                        case "64":
                            mensaje_error = "Número de documento vacío";
                            break;
                        case "65":
                            mensaje_error = "Número de documento inválido";
                            break;
                        case "67":
                            mensaje_error = "Tipo de documento inválido";
                            break;
                        case "68":
                            mensaje_error = "Punto de venta no encontrado en nuestras fuentes";
                            break;
                        case "69":
                            mensaje_error = "Razón Social del punto de venta vacío";
                            break;
                        case "96":
                            mensaje_error = "No se completó la operación. Revise el detalle de validación";
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
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:

                    switch (codigo)
                    {
                        case "99":
                            mensaje_error = "No hubo respuesta por parte del operador.";
                            break;
                        case "404":
                            mensaje_error = "Transacción no encontrada";
                            break;
                        case "460":
                            mensaje_error = "Error desconocido";
                            break;
                        case "461":
                            mensaje_error = "Número incorrecto (por formato)";
                            break;
                        case "462":
                            mensaje_error = "Monto incorrecto (no válido o no autorizado)";
                            break;
                        case "463":
                            mensaje_error = "Producto incorrecto (id no válido)";
                            break;
                        case "465":
                            mensaje_error = "Saldo insuficiente para ejecutar la operación";
                            break;
                        case "500":
                            mensaje_error = "Error interno en el servidor";
                            break;
                    }
                    break;
            }

            return mensaje_error;
        }
        public SqlCommand insTransaccionError(SqlConnection cn, SqlTransaction tran, TransaccionModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_transaccion_error", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_trx_hub", model.nu_id_trx_hub);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                cmd.Parameters.AddWithValue("@dt_fecha", model.dt_fecha);
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@nu_precio", model.nu_precio);
                cmd.Parameters.AddWithValue("@vc_cod_error", model.vc_cod_error);
                cmd.Parameters.AddWithValue("@vc_desc_error", model.vc_desc_error);
                cmd.Parameters.AddWithValue("@vc_desc_tipo_error", model.vc_desc_tipo_error);
                cmd.Parameters.AddWithValue("@nu_id_tipo_moneda", model.nu_id_tipo_moneda_vta);
                cmd.Parameters.AddWithValue("@vc_numero_servicio", model.vc_numero_servicio);
                cmd.Parameters.AddWithValue("@ti_respuesta_api", model.ti_respuesta_api);

                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        public SqlCommand insTrxhub(SqlConnection cn, TrxHubModel model)
        {

            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_trxhub", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                cmd.Parameters.AddWithValue("@vc_cod_comercio", model.vc_cod_comercio);
                cmd.Parameters.AddWithValue("@vc_nombre_comercio", model.vc_nombre_comercio);
                cmd.Parameters.AddWithValue("@vc_nro_telefono", model.vc_nro_telefono);
                cmd.Parameters.AddWithValue("@vc_email", model.vc_email);
                cmd.Parameters.AddWithValue("@vc_id_producto", model.vc_id_producto);
                cmd.Parameters.AddWithValue("@nu_precio_vta", model.nu_precio_vta);
                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_sol", model.nu_id_tipo_doc_sol);
                cmd.Parameters.AddWithValue("@vc_nro_doc_sol", model.vc_nro_doc_sol);
                cmd.Parameters.AddWithValue("@ch_dig_ver_sol", model.ch_dig_ver_sol);
                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_cpt", model.nu_id_tipo_doc_cpt);
                cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", model.vc_nro_doc_cpt);
                cmd.Parameters.AddWithValue("@nu_id_tipo_documento", model.nu_id_tipo_comprobante);
                cmd.Parameters.AddWithValue("@vc_ruc", model.vc_ruc);
                cmd.Parameters.AddWithValue("@vc_numero_servicio", model.vc_numero_servicio);
                cmd.Parameters.AddWithValue("@vc_nro_doc_pago", model.vc_nro_doc_pago);
                cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", model.vc_id_ref_trx_distribuidor);
                cmd.Parameters.AddWithValue("@bi_extorno", model.bi_extorno);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }

        }
        public SqlCommand updTrxhubSaldo(SqlConnection cn, TrxHubModel model)
        {

            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_upd_trxhub_saldo", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx_hub", model.nu_id_trx_hub);
                cmd.Parameters.AddWithValue("@bi_extorno", model.bi_extorno);
                cmd.Parameters.AddWithValue("@vc_mensaje_error", model.vc_mensaje_error);
                cmd.Parameters.AddWithValue("@bi_error", model.bi_error);
                UtilSql.iUpd(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }

        }
        public SqlCommand updTrxhubError(SqlConnection cn, TrxHubModel model)
        {

            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_upd_trxhub_error", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx_hub", model.nu_id_trx_hub);
                cmd.Parameters.AddWithValue("@vc_mensaje_error", model.vc_mensaje_error);
                UtilSql.iUpd(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }

        }
        public SqlCommand insTransaccionExtorno(SqlConnection cn, SqlTransaction tran, TransaccionModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_trx.usp_ins_transaccion_extorno", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_trx_hub", model.nu_id_trx_hub);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@dt_fecha", model.dt_fecha);
                cmd.Parameters.AddWithValue("@nu_id_tipo_moneda_vta", model.nu_id_tipo_moneda_vta);
                cmd.Parameters.AddWithValue("@nu_precio_vta", model.nu_precio);

                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_sol", model.nu_id_tipo_doc_sol);
                cmd.Parameters.AddWithValue("@vc_nro_doc_sol", model.vc_nro_doc_sol);
                cmd.Parameters.AddWithValue("@ch_dig_ver_sol", model.ch_dig_ver_sol);
                cmd.Parameters.AddWithValue("@vc_email_sol", model.vc_email_sol);
                cmd.Parameters.AddWithValue("@vc_telefono_sol", model.vc_telefono_sol);
                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_cpt", model.nu_id_tipo_doc_cpt);
                cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", model.vc_nro_doc_cpt);
                cmd.Parameters.AddWithValue("@nu_id_tipo_documento", model.nu_id_tipo_comprobante);
                cmd.Parameters.AddWithValue("@vc_ruc", model.vc_ruc);

                cmd.Parameters.AddWithValue("@vc_numero_servicio", model.vc_numero_servicio);
                cmd.Parameters.AddWithValue("@vc_nro_doc_pago", model.vc_nro_doc_pago);
                cmd.Parameters.AddWithValue("@vc_cod_autorizacion", model.vc_cod_autorizacion);
                cmd.Parameters.AddWithValue("@vc_nro_pin", model.vc_nro_pin);
                cmd.Parameters.AddWithValue("@vc_id_ref_trx", model.vc_id_ref_trx);
                cmd.Parameters.AddWithValue("@nu_saldo_izipay", model.nu_saldo_izipay);

                cmd.Parameters.AddWithValue("@nu_id_trx_ref", model.nu_id_trx_ref);
                cmd.Parameters.AddWithValue("@bi_confirmado", model.bi_confirmado);
                cmd.Parameters.AddWithValue("@ti_respuesta_api", model.ti_respuesta_api);

                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        public TransaccionModel get_transaccion(SqlConnection cn, TransaccionModel model)
        {
            using (var cmd = new SqlCommand("tisi_trx.usp_get_transaccion", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 1;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (UtilSql.Ec(dr, "nu_id_trx"))
                        model.nu_id_trx = Convert.ToInt32(dr["nu_id_trx"].ToString());
                    if (UtilSql.Ec(dr, "nu_id_trx_hub"))
                        model.nu_id_trx_hub = Convert.ToInt64(dr["nu_id_trx_hub"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_distribuidor"))
                        model.vc_cod_distribuidor = dr["vc_cod_distribuidor"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_distribuidor"))
                        model.vc_desc_distribuidor = dr["vc_desc_distribuidor"].ToString();
                    if (UtilSql.Ec(dr, "vc_cod_comercio"))
                        model.vc_cod_comercio = dr["vc_cod_comercio"].ToString();
                    if (UtilSql.Ec(dr, "vc_nombre_comercio"))
                        model.vc_nombre_comercio = dr["vc_nombre_comercio"].ToString();
                    if (UtilSql.Ec(dr, "dt_fecha"))
                        model.dt_fecha = dr["dt_fecha"].ToDateTime();
                    if (UtilSql.Ec(dr, "nu_id_producto"))
                        model.nu_id_producto = dr["nu_id_producto"].ToInt();
                    if (UtilSql.Ec(dr, "vc_desc_producto"))
                        model.vc_desc_producto = dr["vc_desc_producto"].ToString();
                    if (UtilSql.Ec(dr, "nu_precio_vta"))
                        model.nu_precio_vta = dr["nu_precio_vta"].ToDecimal();
                    if (UtilSql.Ec(dr, "nu_valor_comision"))
                        model.nu_valor_comision = dr["nu_valor_comision"].ToDecimal();
                    if (UtilSql.Ec(dr, "nu_imp_trx"))
                        model.nu_imp_trx = dr["nu_imp_trx"].ToDecimal();
                    if (UtilSql.Ec(dr, "vc_id_ref_trx"))
                        model.vc_id_ref_trx = dr["vc_id_ref_trx"].ToString();
                    if (UtilSql.Ec(dr, "vc_cod_autorizacion"))
                        model.vc_cod_autorizacion = dr["vc_cod_autorizacion"].ToString();
                    if (UtilSql.Ec(dr, "vc_tipo_doc_sol"))
                        model.vc_tipo_doc_sol = dr["vc_tipo_doc_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_doc_sol"))
                        model.vc_nro_doc_sol = dr["vc_nro_doc_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_email_sol"))
                        model.vc_email_sol = dr["vc_email_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_telefono_sol"))
                        model.vc_telefono_sol = dr["vc_telefono_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_tipo_doc_cpt"))
                        model.vc_tipo_doc_cpt = dr["vc_tipo_doc_cpt"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_doc_cpt"))
                        model.vc_nro_doc_cpt = dr["vc_nro_doc_cpt"].ToString();
                    if (UtilSql.Ec(dr, "vc_tipo_comprobante"))
                        model.vc_tipo_comprobante = dr["vc_tipo_comprobante"].ToString();
                    if (UtilSql.Ec(dr, "vc_ruc"))
                        model.vc_ruc = dr["vc_ruc"].ToString();
                    if (UtilSql.Ec(dr, "vc_numero_servicio"))
                        model.vc_numero_servicio = dr["vc_numero_servicio"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_doc_pago"))
                        model.vc_nro_doc_pago = dr["vc_nro_doc_pago"].ToString();
                    if (UtilSql.Ec(dr, "vc_fecha_reg"))
                        model.vc_fecha_reg = dr["vc_fecha_reg"].ToString();
                    if (UtilSql.Ec(dr, "bi_confirmado"))
                        model.bi_confirmado = dr["bi_confirmado"].ToBool();
                    if (UtilSql.Ec(dr, "bi_informado"))
                        model.bi_informado = dr["bi_informado"].ToBool();

                    if (UtilSql.Ec(dr, "nu_id_convenio"))
                        model.nu_id_convenio = Convert.ToDecimal(dr["nu_id_convenio"].ToString());

                    if (UtilSql.Ec(dr, "vc_desc_categoria"))
                        model.vc_desc_categoria = dr["vc_desc_categoria"].ToString();


                    if (UtilSql.Ec(dr, "vc_nro_pin"))
                        model.vc_nro_pin = dr["vc_nro_pin"].ToString();

                    if (UtilSql.Ec(dr, "nu_id_estado"))
                        model.nu_id_estado = dr["nu_id_estado"].ToDecimal();
                    if (UtilSql.Ec(dr, "vc_error"))
                        model.vc_error = dr["vc_error"].ToString();
                }
            }
            return model;
        }
        public TransaccionModel get_transaccion_distribuidor(SqlConnection cn, TransaccionModel model)
        {
            using (var cmd = new SqlCommand("tisi_trx.usp_get_transaccion", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 2;
                cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", model.vc_id_ref_trx_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (UtilSql.Ec(dr, "nu_id_trx"))
                        model.nu_id_trx = Convert.ToInt32(dr["nu_id_trx"].ToString());
                    if (UtilSql.Ec(dr, "nu_id_trx_hub"))
                        model.nu_id_trx_hub = Convert.ToInt64(dr["nu_id_trx_hub"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_distribuidor"))
                        model.vc_cod_distribuidor = dr["vc_cod_distribuidor"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_distribuidor"))
                        model.vc_desc_distribuidor = dr["vc_desc_distribuidor"].ToString();
                    if (UtilSql.Ec(dr, "vc_cod_comercio"))
                        model.vc_cod_comercio = dr["vc_cod_comercio"].ToString();
                    if (UtilSql.Ec(dr, "vc_nombre_comercio"))
                        model.vc_nombre_comercio = dr["vc_nombre_comercio"].ToString();
                    if (UtilSql.Ec(dr, "dt_fecha"))
                        model.dt_fecha = dr["dt_fecha"].ToDateTime();
                    if (UtilSql.Ec(dr, "nu_id_producto"))
                        model.nu_id_producto = dr["nu_id_producto"].ToInt();
                    if (UtilSql.Ec(dr, "vc_desc_producto"))
                        model.vc_desc_producto = dr["vc_desc_producto"].ToString();
                    if (UtilSql.Ec(dr, "nu_precio_vta"))
                        model.nu_precio_vta = dr["nu_precio_vta"].ToDecimal();
                    if (UtilSql.Ec(dr, "nu_valor_comision"))
                        model.nu_valor_comision = dr["nu_valor_comision"].ToDecimal();
                    if (UtilSql.Ec(dr, "nu_imp_trx"))
                        model.nu_imp_trx = dr["nu_imp_trx"].ToDecimal();
                    if (UtilSql.Ec(dr, "vc_id_ref_trx"))
                        model.vc_id_ref_trx = dr["vc_id_ref_trx"].ToString();
                    if (UtilSql.Ec(dr, "vc_cod_autorizacion"))
                        model.vc_cod_autorizacion = dr["vc_cod_autorizacion"].ToString();
                    if (UtilSql.Ec(dr, "vc_tipo_doc_sol"))
                        model.vc_tipo_doc_sol = dr["vc_tipo_doc_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_doc_sol"))
                        model.vc_nro_doc_sol = dr["vc_nro_doc_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_email_sol"))
                        model.vc_email_sol = dr["vc_email_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_telefono_sol"))
                        model.vc_telefono_sol = dr["vc_telefono_sol"].ToString();
                    if (UtilSql.Ec(dr, "vc_tipo_doc_cpt"))
                        model.vc_tipo_doc_cpt = dr["vc_tipo_doc_cpt"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_doc_cpt"))
                        model.vc_nro_doc_cpt = dr["vc_nro_doc_cpt"].ToString();
                    if (UtilSql.Ec(dr, "vc_tipo_comprobante"))
                        model.vc_tipo_comprobante = dr["vc_tipo_comprobante"].ToString();
                    if (UtilSql.Ec(dr, "vc_ruc"))
                        model.vc_ruc = dr["vc_ruc"].ToString();
                    if (UtilSql.Ec(dr, "vc_numero_servicio"))
                        model.vc_numero_servicio = dr["vc_numero_servicio"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_doc_pago"))
                        model.vc_nro_doc_pago = dr["vc_nro_doc_pago"].ToString();
                    if (UtilSql.Ec(dr, "vc_fecha_reg"))
                        model.vc_fecha_reg = dr["vc_fecha_reg"].ToString();
                    if (UtilSql.Ec(dr, "bi_confirmado"))
                        model.bi_confirmado = dr["bi_confirmado"].ToBool();
                    if (UtilSql.Ec(dr, "bi_informado"))
                        model.bi_informado = dr["bi_informado"].ToBool();

                    if (UtilSql.Ec(dr, "nu_id_convenio"))
                        model.nu_id_convenio = Convert.ToDecimal(dr["nu_id_convenio"].ToString());

                    if (UtilSql.Ec(dr, "vc_desc_categoria"))
                        model.vc_desc_categoria = dr["vc_desc_categoria"].ToString();


                    if (UtilSql.Ec(dr, "vc_nro_pin"))
                        model.vc_nro_pin = dr["vc_nro_pin"].ToString();

                    if (UtilSql.Ec(dr, "nu_id_estado"))
                        model.nu_id_estado = dr["nu_id_estado"].ToDecimal();
                    if (UtilSql.Ec(dr, "vc_error"))
                        model.vc_error = dr["vc_error"].ToString();

                }
            }
            return model;
        }
        public TransaccionModel get_transaccion_validar_datos(SqlConnection cn, TransaccionModel model)
        {
            TransaccionModel model_result = new TransaccionModel();
            using (var cmd = new SqlCommand("TISI_TRX.USP_GET_TRANSACCION_VALIDAR_DATOS", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 1;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@dt_fecha", model.dt_fecha);
                cmd.Parameters.AddWithValue("@nu_precio", model.nu_precio);
                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_sol", model.nu_id_tipo_doc_sol);
                cmd.Parameters.AddWithValue("@vc_nro_doc_sol", model.vc_nro_doc_sol);
                cmd.Parameters.AddWithValue("@ch_dig_ver_sol", model.ch_dig_ver_sol);
                cmd.Parameters.AddWithValue("@vc_email_sol", model.vc_email_sol);
                cmd.Parameters.AddWithValue("@vc_telefono_sol", model.vc_telefono_sol);
                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_cpt", model.nu_id_tipo_doc_cpt);
                cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", model.vc_nro_doc_cpt);
                cmd.Parameters.AddWithValue("@nu_id_tipo_documento", model.nu_id_tipo_comprobante);
                cmd.Parameters.AddWithValue("@vc_ruc", model.vc_ruc);
                cmd.Parameters.AddWithValue("@vc_numero_servicio", model.vc_numero_servicio);
                cmd.Parameters.AddWithValue("@vc_nro_doc_pago", model.vc_nro_doc_pago);
                cmd.Parameters.AddWithValue("@bi_extorno", model.bi_extorno);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    if (UtilSql.Ec(dr, "nu_id_trx"))
                        model_result.nu_id_trx = Convert.ToInt64(dr["nu_id_trx"].ToString());
                }
                else
                {
                    model_result.nu_id_trx = null;
                    model_result.vc_observacion = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();

                }
            }
            return model_result;
        }
        public TransaccionModel get_transaccion_validar_datos_extorno(SqlConnection cn, TransaccionModel model)
        {
            TransaccionModel model_result = new TransaccionModel();
            using (var cmd = new SqlCommand("TISI_TRX.USP_GET_TRANSACCION_VALIDAR_DATOS", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 2;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", model.vc_id_ref_trx_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
                cmd.Parameters.AddWithValue("@dt_fecha", model.dt_fecha);
                cmd.Parameters.AddWithValue("@nu_precio", model.nu_precio);
                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_sol", model.nu_id_tipo_doc_sol);
                cmd.Parameters.AddWithValue("@vc_nro_doc_sol", model.vc_nro_doc_sol);
                cmd.Parameters.AddWithValue("@ch_dig_ver_sol", model.ch_dig_ver_sol);
                cmd.Parameters.AddWithValue("@vc_email_sol", model.vc_email_sol);
                cmd.Parameters.AddWithValue("@vc_telefono_sol", model.vc_telefono_sol);
                cmd.Parameters.AddWithValue("@nu_id_tipo_doc_cpt", model.nu_id_tipo_doc_cpt);
                cmd.Parameters.AddWithValue("@vc_nro_doc_cpt", model.vc_nro_doc_cpt);
                cmd.Parameters.AddWithValue("@nu_id_tipo_documento", model.nu_id_tipo_comprobante);
                cmd.Parameters.AddWithValue("@vc_ruc", model.vc_ruc);
                cmd.Parameters.AddWithValue("@vc_numero_servicio", model.vc_numero_servicio);
                cmd.Parameters.AddWithValue("@vc_nro_doc_pago", model.vc_nro_doc_pago);
                cmd.Parameters.AddWithValue("@bi_extorno", model.bi_extorno);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    if (UtilSql.Ec(dr, "nu_id_trx"))
                        model_result.nu_id_trx = Convert.ToInt64(dr["nu_id_trx"].ToString());
                }
                else
                {
                    model_result.nu_id_trx = null;
                    model_result.vc_observacion = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();

                }
            }
            return model_result;
        }
        public SqlCommand upd_confirmar(SqlConnection cn, TransaccionModel model)
        {
            using (var cmd = new SqlCommand("tisi_trx.usp_upd_transaccion_confirmar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                cmd.Parameters.AddWithValue("@bi_confirmado", true);
                UtilSql.iUpd(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }

        }
        public SqlCommand upd_informar(SqlConnection cn, TransaccionModel model)
        {
            using (var cmd = new SqlCommand("tisi_trx.usp_upd_transaccion_informar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_trx", model.nu_id_trx);
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                cmd.Parameters.AddWithValue("@nu_id_comercio", model.nu_id_comercio);
                cmd.Parameters.AddWithValue("@bi_informado", true);
                UtilSql.iUpd(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }

        }


        public ConvenioModel get_convenio(SqlConnection cn, decimal? nu_id_convenio)
        {
            ConvenioModel _result = new ConvenioModel();
            ConvenioModel model = new ConvenioModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_convenio", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_id_convenio = nu_id_convenio;
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (UtilSql.Ec(dr, "nu_id_convenio"))
                        _result.nu_id_convenio = dr["nu_id_convenio"].ToDecimal();
                    if (UtilSql.Ec(dr, "vc_cod_convenio"))
                        _result.vc_cod_convenio = dr["vc_cod_convenio"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_convenio"))
                        _result.vc_desc_convenio = dr["vc_desc_convenio"].ToString();
                    if (UtilSql.Ec(dr, "nu_id_tipo_moneda_def"))
                        _result.nu_id_tipo_moneda_def = dr["nu_id_tipo_moneda_def"].ToInt();

                    if (UtilSql.Ec(dr, "vc_url_api_token"))
                        _result.vc_url_api_token = dr["vc_url_api_token"].ToString();
                    if (UtilSql.Ec(dr, "vc_url_api_1"))
                        _result.vc_url_api_1 = dr["vc_url_api_1"].ToString();
                    if (UtilSql.Ec(dr, "vc_url_api_2"))
                        _result.vc_url_api_2 = dr["vc_url_api_2"].ToString();
                    if (UtilSql.Ec(dr, "nu_seg_timeout"))
                        _result.nu_seg_timeout = dr["nu_seg_timeout"].ToInt();


                    if (UtilSql.Ec(dr, "vc_nro_celular_aut"))
                        _result.vc_nro_celular_aut = dr["vc_nro_celular_aut"].ToString();
                    if (UtilSql.Ec(dr, "vc_clave_aut"))
                        _result.vc_clave_aut = dr["vc_clave_aut"].ToString();

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
                        _result.nu_puerto_smtp_email = dr["nu_puerto_smtp_email"].ToInt();
                    if (UtilSql.Ec(dr, "bi_ssl_email"))
                        _result.bi_ssl_email = dr["bi_ssl_email"].ToBool();
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
        public DistribuidorModel get_distrbuidor_saldo(SqlConnection cn, DistribuidorModel model)
        {
            using (var cmd = new SqlCommand("TISI_GLOBAL.USP_SEL_DISTRIBUIDOR_SALDO", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 1;
                cmd.Parameters.AddWithValue("@nu_id_distribuidor", model.nu_id_distribuidor);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if (UtilSql.Ec(dr, "nu_saldo"))
                        model.nu_saldo = dr["nu_saldo"].ToDecimal();
                }
            }
            return model;
        }
    }
}
