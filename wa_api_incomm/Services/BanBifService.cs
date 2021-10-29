using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.BanBif;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.BanBif.Response;
using ConvenioModel = wa_api_incomm.Models.BanBif.ConvenioModel;

namespace wa_api_incomm.Services
{
    public class BanBifService : IBanBifService
    {
        public int nu_id_convenio = 3;
        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        public object get_datos_banbif(string conexion)
        {
            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;

            try
            {
                List<RubroModel> ls_rubro = sel_banbif_rubro_recaudador();
                //Guardar en BD

                con_sql.Open();
                tran_sql = con_sql.BeginTransaction();

                foreach (var rubro in ls_rubro)
                {

                    //ins categories

                    RubroModel e_rubro = new RubroModel();
                    e_rubro.nu_id_convenio = nu_id_convenio;
                    e_rubro.vc_cod_rubro = rubro.vc_cod_rubro;
                    e_rubro.vc_desc_rubro = rubro.vc_desc_rubro;
                    e_rubro.bi_estado = true;
                    //tran
                    e_rubro.vc_tran_usua_regi = "API";

                    cmd = Ins_rubro(con_sql, tran_sql, e_rubro);

                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                    {
                        tran_sql.Rollback();
                        return UtilSql.sOutPut(cmd);
                    }
                    RubroModel.Rubro_Input e_rubro_input = new RubroModel.Rubro_Input();
                    e_rubro_input.vc_cod_rubro = e_rubro.vc_cod_rubro;

                    List<EmpresaModel> ls_empresa = sel_banbif_empresa_rubros(e_rubro_input);

                    foreach (var empresa in ls_empresa)
                    {
                        EmpresaModel e_empresa = new EmpresaModel();
                        e_empresa.nu_id_convenio = nu_id_convenio;
                        e_empresa.vc_cod_rubro = e_rubro.vc_cod_rubro;
                        e_empresa.vc_desc_rubro = e_rubro.vc_desc_rubro;
                        e_empresa.vc_cod_empresa = empresa.vc_cod_empresa;
                        e_empresa.vc_nro_doc_identidad = empresa.vc_nro_doc_identidad;
                        e_empresa.vc_nombre = empresa.vc_nombre;
                        e_empresa.vc_razon_social = empresa.vc_razon_social;
                        e_empresa.bi_estado = true;
                        //tran
                        e_empresa.vc_tran_usua_regi = "API";

                        cmd = Ins_empresa_rubro(con_sql, tran_sql, e_empresa);

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                        {
                            tran_sql.Rollback();
                            return UtilSql.sOutPut(cmd);
                        }

                    }
                }
                tran_sql.Commit();
                return UtilSql.sOutPutSuccess("OK");
            }
            catch (Exception ex)
            {

                tran_sql.Rollback();
                return UtilSql.sOutPutCatch(ex.Message);
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }
        public List<RubroModel> sel_banbif_rubro_recaudador()
        {
            List<RubroModel> ls_rubro = new List<RubroModel>();
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.get_rubros_recaudador().Result;

                foreach (var item in (List<Response.E_datos>)result.datos)
                {
                    RubroModel e_rubro = new RubroModel();
                    e_rubro.vc_cod_rubro = item.codigo;
                    e_rubro.vc_desc_rubro = item.descripcion;
                    ls_rubro.Add(e_rubro);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ls_rubro;
        }
        public List<EmpresaModel> sel_banbif_empresa_rubros(RubroModel.Rubro_Input model)
        {
            List<EmpresaModel> ls_empresa = new List<EmpresaModel>();
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.get_empresa_rubros(model).Result;

                foreach (var item in (List<Response.E_datos>)result.datos)
                {
                    EmpresaModel e_empresa = new EmpresaModel();
                    e_empresa.vc_cod_empresa = item.codigo;
                    e_empresa.vc_nro_doc_identidad = item.documento;
                    e_empresa.vc_nombre = item.nombre;
                    e_empresa.vc_razon_social = item.razonSocial;
                    ls_empresa.Add(e_empresa);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ls_empresa;
        }
        public object sel_banbif_convenio(string conexion, EmpresaModel.Empresa_Input model)
        {
            List<ConvenioModel> ls_convenio = new List<ConvenioModel>();
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.get_lista_convenio(model).Result;

                foreach (var item in (List<Response.E_datos>)result.datos)
                {
                    ConvenioModel e_convenio = new ConvenioModel();
                    e_convenio.vc_cod_convenio = item.codigo;
                    e_convenio.vc_desc_convenio = item.descripcion;

                    e_convenio.vc_cod_empresa = item.empresa.codigo;
                    e_convenio.vc_nro_doc_empresa = item.empresa.documento;
                    e_convenio.vc_nombre_empresa = item.empresa.nombre;

                    e_convenio.vc_cod_producto = item.producto.codigo;
                    ls_convenio.Add(e_convenio);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ls_convenio;
        }
        public object get_banbif_convenio(string conexion, ConvenioModel.Convenio_Input model)
        {
            ConvenioModel e_convenio = new ConvenioModel();
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.get_convenio(model).Result;

                Response.E_datos e_datos = (Response.E_datos)result.datos;

                e_convenio.vc_cod_convenio = e_datos.codigo;
                e_convenio.vc_desc_convenio = e_datos.descripcion;

                e_convenio.vc_cod_empresa = e_datos.empresa.codigo;
                e_convenio.vc_nro_doc_empresa = e_datos.empresa.documento;
                e_convenio.vc_nombre_empresa = e_datos.empresa.nombre;

                e_convenio.vc_cod_producto = e_datos.producto.codigo;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return e_convenio;
        }

        public object get_deuda(string conexion, DeudaModel.Deuda_Input model)
        {
            List<DeudaModel> ls_deuda = new List<DeudaModel>();
            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.Consultar_Deuda(model).Result;

                Response.E_meta e_meta = (Response.E_meta)result.meta;
                List<Response.E_datos_trx> ls_datos = (List<Response.E_datos_trx>)result.datos;

                if (ls_datos == null && e_meta != null)
                {
                    string codigo = "";
                    string mensaje = "";
                    foreach (var item in e_meta.mensajes)
                    {
                        codigo = String.Concat(codigo, item.codigo, "; ");
                        mensaje = String.Concat(mensaje, item.mensaje, "; ");

                    }

                    if (codigo.Length > 0)
                        codigo = codigo.Substring(0, codigo.Length - 2);
                    if (mensaje.Length > 0)
                        mensaje = mensaje.Substring(0, mensaje.Length - 2);

                    return UtilSql.sOutPutTransaccion(codigo, mensaje);
                }
                foreach (var e_datos in ls_datos)
                {
                    DeudaModel e_deuda = new DeudaModel();

                    e_deuda.dt_fecha_vencimiento = e_datos.fechaVencimiento.ToString("yyyy-MM-dd");
                    e_deuda.vc_cliente = e_datos.cliente.id;

                    e_deuda.nu_monto_deuda = e_datos.montoRedondeo != e_datos.montoTotalDestino ? e_datos.montoRedondeo: e_datos.montoTotalDestino;
                    e_deuda.vc_numero_documento = e_datos.documento.numero;
                    e_deuda.vc_moneda = e_datos.moneda;
                    e_deuda.dt_fecha_factura = e_datos.fechaFactura.ToString("yyyy-MM-dd");

                    ls_deuda.Add(e_deuda);
                }

            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("500", ex.Message);
                //return UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion");
            }
            return ls_deuda;

        }
        public object post_pago(string conexion, PagoModel.Pago_Input model)
        {
            List<DeudaModel> ls_deuda = new List<DeudaModel>();
            DeudaModel.Deuda_Input Deuda_Model = new DeudaModel.Deuda_Input();
            PagoModel e_pago = new PagoModel();

            string idTransaccionOrigen = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmss"), new Random().Next(1, 9).ToString("D1"));

            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            try
            {

                if (string.IsNullOrEmpty(model.vc_nro_documento))
                {
                    return UtilSql.sOutPutTransaccion("500", "Debe indicar el número de documento de pago.");

                }

                Deuda_Model.nu_id_servicio = model.nu_id_servicio;
                Deuda_Model.vc_cod_convenio = model.vc_cod_convenio;


                BanBifApi client = new BanBifApi();

                var result = client.Consultar_Deuda(Deuda_Model).Result;

                Response.E_meta e_meta = (Response.E_meta)result.meta;
                List<Response.E_datos_trx> ls_datos = (List<Response.E_datos_trx>)result.datos;

                if (ls_datos == null && e_meta != null)
                {
                    string codigo = "";
                    string mensaje = "";
                    foreach (var item in e_meta.mensajes)
                    {
                        codigo = String.Concat(codigo, item.codigo, "; ");
                        mensaje = String.Concat(mensaje, item.mensaje, "; ");

                    }

                    if (codigo.Length > 0)
                        codigo = codigo.Substring(0, codigo.Length - 2);
                    if (mensaje.Length > 0)
                        mensaje = mensaje.Substring(0, mensaje.Length - 2);

                    return UtilSql.sOutPutTransaccion(codigo, mensaje);
                }
                String codigoRecaudador = "";
                foreach (var e_datos in ls_datos)
                {
                    if (e_datos.documento.numero == model.vc_nro_documento)
                    {
                        codigoRecaudador = config.GetSection("BanBifInfo:codigoRecaudador").Value;
                        e_pago.recaudador.codigo = codigoRecaudador;

                        e_pago.convenio.codigo = model.vc_cod_convenio;

                        e_pago.cliente.id = e_datos.cliente.id;
                        e_pago.moneda = e_datos.moneda;
                        e_pago.cantidadPagos = 1;
                        e_pago.agrupacion = false;
                        e_pago.montoTotalDeuda = e_datos.montoTotalDestino;
                        e_pago.montoTotalSaldo = e_datos.montoSaldoDestino;

                        //Agregar datos principales
                        PagoModel.E_datos e_datos_pago = new PagoModel.E_datos();
                        e_datos_pago.fechaVencimiento = e_datos.fechaVencimiento.ToString("yyyy-MM-dd");
                        e_datos_pago.cliente.id = e_datos.cliente.id;
                        e_datos_pago.idConsulta = e_datos.idConsulta.ToString();

                        foreach (var item_serv in e_datos.servicios)
                        {
                            PagoModel.E_servicios e_sv = new PagoModel.E_servicios();
                            e_sv.id = item_serv.id;
                            e_datos_pago.servicios.Add(e_sv);
                        }

                        e_datos_pago.montoSaldoDestino = e_datos.montoSaldoDestino;
                        e_datos_pago.montoDescuentoDestino = e_datos.montoDescuentoDestino;
                        e_datos_pago.montoMultaDestino = e_datos.montoMultaDestino;
                        e_datos_pago.montoVencidoDestino = e_datos.montoVencidoDestino;
                        e_datos_pago.montoInteresDestino = e_datos.montoInteresDestino;
                        e_datos_pago.montoReajusteDestino = e_datos.montoReajusteDestino;
                        e_datos_pago.montoTotalDestino = e_datos.montoTotalDestino;

                        e_datos_pago.documento.numero = e_datos.documento.numero;

                        PagoModel.E_pagos e_p = new PagoModel.E_pagos();

                        e_p.tipoOperacion = "PAGO_TOTAL_CUOTA";
                        e_p.medioPago = "EFECTIVO";
                        e_p.cuentaCargo.numero = "";
                        if (e_datos.montoRedondeo != e_datos.montoTotalDestino)
                        {
                            e_p.monto = e_datos.montoRedondeo;
                        }
                        else
                        {
                            e_p.monto = e_datos.montoTotalDestino;
                        }
                        e_p.deudaAPagar = e_datos.montoTotalDestino;

                        e_datos_pago.pagos.Add(e_p);


                        //

                        PagoModel.E_datosAdicionales e_datos_adicionales = new PagoModel.E_datosAdicionales();

                        foreach (var item_add in e_datos.datosAdicionales)
                        {
                            if (item_add.nombre == "NOMBRE")
                            {
                                e_datos_adicionales = new PagoModel.E_datosAdicionales();
                                e_datos_adicionales.nombre = "Nombre";
                                e_datos_adicionales.valor = item_add.valor;
                                e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                            }
                            if (item_add.nombre == "FECHAEMISION")
                            {
                                e_datos_adicionales = new PagoModel.E_datosAdicionales();
                                e_datos_adicionales.nombre = "FecEmision";
                                e_datos_adicionales.valor = item_add.valor;
                                e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                            }
                        }
                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "CODEBRANCH";
                        e_datos_adicionales.valor = CanalInfo.CODEBRANCH;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "TRNCODE";
                        e_datos_adicionales.valor = CanalInfo.TRNCODE;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "TELLUSERID";
                        e_datos_adicionales.valor = CanalInfo.TELLUSERID;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_RED";
                        e_datos_adicionales.valor = CanalInfo.Cod_RED;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_TRX";
                        e_datos_adicionales.valor = CanalInfo.Cod_TRX;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);


                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Des_TRX";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);




                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_Terminal";
                        e_datos_adicionales.valor = CanalInfo.Cod_Terminal;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_OperaTrace";
                        e_datos_adicionales.valor = idTransaccionOrigen;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);


                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_Autoriza";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Ubi_Terminal";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Nro_Cuenta";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Nro_Tarjeta";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Nro_Cuotas";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);



                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "ID_Servicio";
                        e_datos_adicionales.valor = e_datos.servicios[0].id;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "ID_Recibo";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Nom_EmpServ";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);


                        e_pago.deudas.Add(e_datos_pago);

                        var result_pago = client.Procesar_Pago(e_pago, idTransaccionOrigen).Result;

                        Response.E_meta e_meta_pago = (Response.E_meta)result_pago.meta;
                        Response.E_datos_trx e_datos_pago_result = (Response.E_datos_trx)result_pago.datos;
                        if (e_datos_pago_result == null && e_meta_pago != null)
                        {
                            string codigo = "";
                            string mensaje = "";
                            foreach (var item in e_meta_pago.mensajes)
                            {
                                codigo = String.Concat(codigo, item.codigo, "; ");
                                mensaje = String.Concat(mensaje, item.mensaje, "; ");

                            }

                            if (codigo.Length > 0)
                                codigo = codigo.Substring(0, codigo.Length - 2);
                            if (mensaje.Length > 0)
                                mensaje = mensaje.Substring(0, mensaje.Length - 2);

                            return UtilSql.sOutPutTransaccion(codigo, mensaje);
                        }
                        if (e_meta_pago.mensajes[0].codigo == "ESM00")
                        {
                            object info = new object();

                            info = new
                            {
                                codigo = "00",
                                mensaje = "Se realizó el pago correctamente.",
                                nro_transaccion = e_datos_pago_result.deudas[0].pagos[0].numeroPago
                            };


                            return info;

                        }


                    }

                }

                if (codigoRecaudador == "")
                {
                    return UtilSql.sOutPutTransaccion("500", "No se encontro deuda con el número de documento " + model.vc_nro_documento);
                }
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("500", ex.Message);
                //return UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion");
            }
            return ls_deuda;

        }

        private static SqlCommand Ins_rubro(SqlConnection cn, SqlTransaction tran, RubroModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_rubro", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@vc_cod_rubro", model.vc_cod_rubro);
                cmd.Parameters.AddWithValue("@vc_desc_rubro", model.vc_desc_rubro);
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                cmd.Parameters.AddWithValue("@bi_estado", model.bi_estado);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        private static SqlCommand Ins_empresa_rubro(SqlConnection cn, SqlTransaction tran, EmpresaModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_empresa_rubro", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@vc_cod_rubro", model.vc_cod_rubro);
                cmd.Parameters.AddWithValue("@vc_desc_rubro", model.vc_desc_rubro);
                cmd.Parameters.AddWithValue("@vc_cod_empresa", model.vc_cod_empresa);
                cmd.Parameters.AddWithValue("@vc_nro_doc_identidad", model.vc_nro_doc_identidad);
                cmd.Parameters.AddWithValue("@vc_nombre", model.vc_nombre);
                cmd.Parameters.AddWithValue("@vc_razon_social", model.vc_razon_social);
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                cmd.Parameters.AddWithValue("@bi_estado", model.bi_estado);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        private static SqlCommand Ins_producto_convenio(SqlConnection cn, SqlTransaction tran, ConvenioModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_producto_convenio_banbif", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@vc_cod_empresa", model.vc_cod_empresa);
                cmd.Parameters.AddWithValue("@vc_nombre_empresa", model.vc_nombre_empresa);
                cmd.Parameters.AddWithValue("@vc_cod_convenio", model.vc_cod_convenio);
                cmd.Parameters.AddWithValue("@vc_desc_convenio", model.vc_desc_convenio);
                cmd.Parameters.AddWithValue("@bi_estado", model.bi_estado);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
    }
}
