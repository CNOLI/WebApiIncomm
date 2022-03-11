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
        public int get_id_transaccion(SqlConnection cn)
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
        public SqlCommand insTransaccionExtorno(SqlConnection cn, SqlTransaction tran, TransaccionModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_transaccion_extorno", cn, tran))
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

                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
    }
}
