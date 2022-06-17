using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Services
{
    public class ConsultaService : IConsultaService
    {
        public object Transaccion(string conexion, Consulta.Transaccion_Input model)
        {

            SqlConnection con_sql = new SqlConnection(conexion);

            try
            {
                GlobalService global_service = new GlobalService();

                con_sql.Open();

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("01", "El código de distribuidor no existe");
                }
                ComercioModel comercio = new ComercioModel();
                comercio.vc_cod_comercio = model.codigo_comercio;
                comercio = global_service.get_comercio_busqueda(con_sql, model.codigo_comercio, distribuidor.nu_id_distribuidor);

                if (comercio.nu_id_comercio <= 0)
                {
                    return UtilSql.sOutPutTransaccion("07", "El código de comercio no existe");
                }

                TransaccionModel trx = new TransaccionModel();
                trx.nu_id_trx = model.nro_transaccion.ToInt64();
                trx.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                trx.nu_id_comercio = comercio.nu_id_comercio;

                trx = global_service.get_transaccion(con_sql, trx);

                con_sql.Close();

                object info = new object();
                if (trx.vc_fecha_reg != null)
                {
                    Consulta.Transaccion_Result trx_result = new Consulta.Transaccion_Result();
                    trx_result.nro_transaccion = trx.nu_id_trx.ToString();
                    trx_result.codigo_distribuidor = trx.vc_cod_distribuidor.ToString();
                    trx_result.nombre_distribuidor = trx.vc_desc_distribuidor.ToString();
                    trx_result.codigo_comercio = trx.vc_cod_comercio.ToString();
                    trx_result.nombre_comercio = trx.vc_nombre_comercio.ToString();
                    trx_result.fecha = ((DateTime)trx.dt_fecha).ToString("dd/MM/yyyy");
                    trx_result.nombre_producto = trx.vc_desc_producto.ToString();
                    trx_result.precio_venta = Decimal.Round(Convert.ToDecimal((trx.nu_precio_vta ?? 0).ToString("N")), 2).ToString();
                    trx_result.valor_comision = Decimal.Round(Convert.ToDecimal((trx.nu_valor_comision ?? 0).ToString("N")), 2).ToString();
                    trx_result.importe = Decimal.Round(Convert.ToDecimal((trx.nu_imp_trx ?? 0).ToString("N")), 2).ToString();
                    trx_result.nro_referencia = trx.vc_id_ref_trx.ToString();
                    trx_result.nro_autorizacion = trx.vc_cod_autorizacion.ToString();

                    trx_result.tipo_documento_consultante = trx.vc_tipo_doc_sol.ToString();
                    trx_result.nro_documento_consultante = trx.vc_nro_doc_sol.ToString();
                    trx_result.telefono_consultante = trx.vc_telefono_sol.ToString();
                    trx_result.email_consultante = trx.vc_email_sol.ToString();

                    trx_result.tipo_documento_consultado = trx.vc_tipo_doc_cpt.ToString();
                    trx_result.nro_documento_consultado = trx.vc_nro_doc_cpt.ToString();

                    trx_result.tipo_documento_facturacion = trx.vc_tipo_comprobante.ToString();

                    trx_result.nro_ruc = trx.vc_ruc.ToString();
                    trx_result.nro_servicio = trx.vc_numero_servicio.ToString();
                    trx_result.nro_documento_pago = trx.vc_nro_doc_pago.ToString();
                    trx_result.fecha_registro = trx.vc_fecha_reg.ToString();

                    info = new
                    {
                        codigo = "00",
                        mensaje = "Transacción consultada correctamente.",
                        datos = trx_result
                    };
                }
                else
                {
                    info = new
                    {
                        codigo = "99",
                        mensaje = "No se encontró transacción."
                    };

                }


                return info;
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutCatch(ex.Message);
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }

        public object Estado(string conexion, Consulta.Transaccion_Input_Estado model)
        {

            SqlConnection con_sql = new SqlConnection(conexion);

            try
            {
                GlobalService global_service = new GlobalService();

                con_sql.Open();

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("01", "El código de distribuidor no existe");
                }
                ComercioModel comercio = new ComercioModel();
                comercio.vc_cod_comercio = model.codigo_comercio;
                comercio = global_service.get_comercio_busqueda(con_sql, model.codigo_comercio, distribuidor.nu_id_distribuidor);

                if (comercio.nu_id_comercio <= 0)
                {
                    return UtilSql.sOutPutTransaccion("07", "El código de comercio no existe");
                }

                TransaccionModel trx = new TransaccionModel();
                trx.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;
                trx.nu_id_distribuidor = distribuidor.nu_id_distribuidor;
                trx.nu_id_comercio = comercio.nu_id_comercio;

                trx = global_service.get_transaccion_distribuidor(con_sql, trx);

                con_sql.Close();

                object info = new object();
                if (trx.vc_fecha_reg != null)
                {
                    switch (trx.nu_id_estado)
                    {
                        case -1:
                            info = new
                            {
                                codigo = "39",
                                mensaje = "No se encontró transacción con los datos enviados."
                            };
                            break;
                        case 0:
                            info = new
                            {
                                codigo = "36",
                                mensaje = "La transacción está siendo procesada."
                            };
                            break;
                        case 1:
                            info = new
                            {
                                codigo = "00",
                                mensaje = "Transacción procesada correctamente.",
                                nro_transaccion = trx.nu_id_trx.ToString(),
                                confirmado = (trx.bi_confirmado == true ? "1" : "0")
                            };
                            break;
                        case 2:
                            info = new
                            {
                                codigo = "37",
                                mensaje = "La transacción no fue procesada: " + trx.vc_error + "."
                            };
                            break;
                        case 3:
                            info = new
                            {
                                codigo = "00",
                                mensaje = "Transacción procesada correctamente.",
                                nro_transaccion = trx.nu_id_trx.ToString(),
                                confirmado = (trx.bi_confirmado == true ? "1" : "0")
                            };
                            break;
                        case 4:
                            info = new
                            {
                                codigo = "00",
                                mensaje = "Transacción procesada correctamente.",
                                nro_transaccion = trx.nu_id_trx.ToString(),
                                confirmado = (trx.bi_confirmado == true ? "1" : "0")
                            };
                            break;
                        case 5:
                            info = new
                            {
                                codigo = "38",
                                mensaje = "Transacción ha sido extornada."
                            };
                            break;
                    }
                }
                else
                {
                    info = new
                    {
                        codigo = "39",
                        mensaje = "No se encontró transacción con los datos enviados."
                    };

                }


                return info;
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutCatch(ex.Message);
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }
        public object Saldo(string conexion, Consulta.Distribuidor_Saldo_Input model)
        {

            SqlConnection con_sql = new SqlConnection(conexion);

            try
            {
                GlobalService global_service = new GlobalService();

                con_sql.Open();

                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("01", "El código de distribuidor no existe");
                }

                distribuidor = global_service.get_distrbuidor_saldo(con_sql, distribuidor);

                con_sql.Close();

                object info = new object();

                info = new
                {
                    saldo_disponible = distribuidor.nu_saldo.ToString()
                };

                return info;
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutCatch(ex.Message);
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }
    }
}
