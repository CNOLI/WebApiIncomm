using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.BanBif;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Models.Servicios;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.BanBif.Response;
using ConvenioModel = wa_api_incomm.Models.BanBif.ConvenioModel;

namespace wa_api_incomm.Services
{
    public class BanBifService : IBanBifService
    {
        public int nu_id_convenio = 3;

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        private readonly Serilog.ILogger _logger;

        Models.Hub.ConvenioModel hub_convenio;
        public BanBifService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object get_datos_banbif(string conexion)
        {
            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;

            try
            {
                con_sql.Open();

                List<RubroModel> ls_rubro = sel_rubro(con_sql);

                con_sql.Close();
                //Guardar en BD
                con_sql.Open();

                tran_sql = con_sql.BeginTransaction();

                foreach (var e_rubro in ls_rubro)
                {
                    RubroModel.Rubro_Input e_rubro_input = new RubroModel.Rubro_Input();
                    e_rubro_input.vc_cod_rubro = e_rubro.vc_cod_rubro;

                    List<EmpresaModel> ls_empresa = sel_banbif_empresa_rubros(e_rubro_input, conexion);

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
        public List<RubroModel> sel_banbif_rubro_recaudador(string conexion)
        {
            GlobalService global_service = new GlobalService();
            List<RubroModel> ls_rubro = new List<RubroModel>();
            try
            {
                SqlConnection con_sql = new SqlConnection(conexion);
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                BanBifApi client = new BanBifApi(hub_convenio);

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
        private List<RubroModel> sel_rubro(SqlConnection cn)
        {
            List<RubroModel> ls = new List<RubroModel>();
            RubroModel model = new RubroModel();
            using (var cmd = new SqlCommand("tisi_global.usp_sel_rubro_convenios", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_tran_ruta = 1;
                model.nu_id_convenio = nu_id_convenio;
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    model = new RubroModel();
                    if (UtilSql.Ec(dr, "nu_id_rubro"))
                        model.nu_id_rubro = Convert.ToInt32(dr["nu_id_rubro"].ToString());
                    if (UtilSql.Ec(dr, "vc_cod_rubro"))
                        model.vc_cod_rubro = dr["vc_cod_rubro"].ToString();
                    if (UtilSql.Ec(dr, "vc_desc_rubro"))
                        model.vc_desc_rubro = dr["vc_desc_rubro"].ToString();
                    ls.Add(model);
                }
            }
            return ls;
        }
        public List<EmpresaModel> sel_banbif_empresa_rubros(RubroModel.Rubro_Input model, string conexion)
        {
            GlobalService global_service = new GlobalService();
            List<EmpresaModel> ls_empresa = new List<EmpresaModel>();
            try
            {
                SqlConnection con_sql = new SqlConnection(conexion);
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                BanBifApi client = new BanBifApi(hub_convenio);

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
            GlobalService global_service = new GlobalService();
            List<ConvenioModel> ls_convenio = new List<ConvenioModel>();
            try
            {
                SqlConnection con_sql = new SqlConnection(conexion);
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                BanBifApi client = new BanBifApi(hub_convenio);

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
            GlobalService global_service = new GlobalService();
            ConvenioModel e_convenio = new ConvenioModel();
            try
            {
                SqlConnection con_sql = new SqlConnection(conexion);
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                BanBifApi client = new BanBifApi(hub_convenio);

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
            GlobalService global_service = new GlobalService();
            List<DeudaModel> ls_deuda = new List<DeudaModel>();
            try
            {
                SqlConnection con_sql = new SqlConnection(conexion);
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                BanBifApi client = new BanBifApi(hub_convenio);

                var result = client.Consultar_Deuda(model, null).Result;

                Response.E_meta e_meta = (Response.E_meta)result.meta;
                List<Response.E_datos_trx> ls_datos = (List<Response.E_datos_trx>)result.datos;

                string codigo = "";
                string mensaje = "";

                string vc_cod_error_hub = "99";
                string vc_desc_error_hub = "Error desconocido.";

                if (result.timeout == true)
                {
                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, "-1", ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);
                }

                if (ls_datos == null && e_meta != null)
                {

                    foreach (var item in e_meta.mensajes)
                    {
                        con_sql.Open();
                        global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, item.codigo, ref vc_cod_error_hub, ref vc_desc_error_hub);
                        con_sql.Close();

                        codigo = String.Concat(codigo, vc_cod_error_hub, " - ");
                        mensaje = String.Concat(mensaje, vc_desc_error_hub, " - ");
                    }

                    if (codigo.Length > 0)
                        codigo = codigo.Substring(0, codigo.Length - 3);
                    if (mensaje.Length > 0)
                        mensaje = mensaje.Substring(0, mensaje.Length - 3);
                                   
                    return UtilSql.sOutPutTransaccion(codigo, mensaje);
                }
                foreach (var e_datos in ls_datos)
                {
                    DeudaModel e_deuda = new DeudaModel();

                    e_deuda.fecha_vencimento = e_datos.fechaVencimiento.ToString("yyyy-MM-dd");
                    e_deuda.cliente = e_datos.cliente.id;
                    if (e_deuda.cliente == "")
                    {
                        foreach (var item in e_datos.datosAdicionales)
                        {
                            if (item.nombre == "Nombre")
                            {
                                e_deuda.cliente = item.valor;

                            }
                        }
                    }
                    if (e_deuda.cliente.Length > 20)
                    {
                        e_deuda.cliente = e_deuda.cliente.Substring(0, 20);
                    }

                    e_deuda.monto_documento = (e_datos.montoRedondeo != e_datos.montoTotalDestino ? e_datos.montoRedondeo : e_datos.montoTotalDestino) - e_datos.comisionCliente;
                    e_deuda.monto_documento = Decimal.Round(Convert.ToDecimal((e_deuda.monto_documento ?? 0).ToString("N")), 2);
                    e_deuda.comision_cliente = Decimal.Round(Convert.ToDecimal((e_datos.comisionCliente ?? 0).ToString("N")), 2) + Decimal.Round(Convert.ToDecimal(model.comision_usuario_final), 2); 
                    e_deuda.monto_deuda = e_datos.montoRedondeo != e_datos.montoTotalDestino ? e_datos.montoRedondeo : e_datos.montoTotalDestino;
                    e_deuda.monto_deuda = Decimal.Round(Convert.ToDecimal((e_deuda.monto_deuda ?? 0).ToString("N")), 2) + Decimal.Round(Convert.ToDecimal(model.comision_usuario_final), 2);
                    e_deuda.numero_documento = e_datos.documento.numero;
                    e_deuda.moneda = e_datos.moneda;

                    string monedaISO = "";
                    switch (e_deuda.moneda)
                    {
                        case "SOL":
                            monedaISO = "PEN";
                            break;
                        case "DOLAR":
                            monedaISO = "USD";
                            break;
                        case "USD":
                            monedaISO = "USD";
                            break;
                        default:
                            monedaISO = "";
                            break;
                    }
                    RegionInfo region = CultureInfo
                     .GetCultures(CultureTypes.SpecificCultures)
                     .Select(ct => new RegionInfo(ct.LCID))
                     .Where(ri => ri.ISOCurrencySymbol == monedaISO).FirstOrDefault();

                    e_deuda.simbolo_moneda = region != null ? region.CurrencySymbol : "";

                    e_deuda.fecha_factura = e_datos.fechaFactura.ToString("yyyy-MM-dd");

                    ls_deuda.Add(e_deuda);
                }
                #region Pruebas temporales
                //DeudaModel e_deuda = new DeudaModel();

                //e_deuda.fecha_vencimento = "2022-02-19";
                //e_deuda.cliente = "PANTA PAZO VICTOR M";
                //e_deuda.monto_deuda = (decimal)341.00;
                //e_deuda.monto_deuda = Decimal.Round(Convert.ToDecimal((e_deuda.monto_deuda ?? 0).ToString("N")), 2);
                //e_deuda.numero_documento = "2048010328";
                //e_deuda.moneda = "SOL";
                //e_deuda.simbolo_moneda = "S/";
                //e_deuda.fecha_factura = "2020-02-19";

                //ls_deuda.Add(e_deuda);
                //e_deuda = new DeudaModel();
                //e_deuda.fecha_vencimento = "2020-03-20";
                //e_deuda.cliente = "PANTA PAZO VICTOR M";
                //e_deuda.monto_deuda = (decimal)282.60;
                //e_deuda.monto_deuda = Decimal.Round(Convert.ToDecimal((e_deuda.monto_deuda ?? 0).ToString("N")), 2);
                //e_deuda.numero_documento = "2048010330";
                //e_deuda.moneda = "SOL";
                //e_deuda.simbolo_moneda = "S/";
                //e_deuda.fecha_factura = "2020-03-20";

                //ls_deuda.Add(e_deuda);
                //e_deuda = new DeudaModel();
                //e_deuda.fecha_vencimento = "2020-04-22";
                //e_deuda.cliente = "PANTA PAZO VICTOR M";
                //e_deuda.monto_deuda = (decimal)276.10;
                //e_deuda.monto_deuda = Decimal.Round(Convert.ToDecimal((e_deuda.monto_deuda ?? 0).ToString("N")), 2);
                //e_deuda.numero_documento = "2048010331";
                //e_deuda.moneda = "SOL";
                //e_deuda.simbolo_moneda = "S/";
                //e_deuda.fecha_factura = "2020-04-22";

                //ls_deuda.Add(e_deuda);
                //e_deuda = new DeudaModel();
                //e_deuda.fecha_vencimento = "2020-05-21";
                //e_deuda.cliente = "PANTA PAZO VICTOR M";
                //e_deuda.monto_deuda = (decimal)277;
                //e_deuda.monto_deuda = Decimal.Round(Convert.ToDecimal((e_deuda.monto_deuda ?? 0).ToString("N")), 2);
                //e_deuda.numero_documento = "2048010332";
                //e_deuda.moneda = "SOL";
                //e_deuda.simbolo_moneda = "S/";
                //e_deuda.fecha_factura = "2020-05-21";

                //ls_deuda.Add(e_deuda);
                //e_deuda = new DeudaModel();
                //e_deuda.fecha_vencimento = "2020-07-04";
                //e_deuda.cliente = "PANTA PAZO VICTOR M";
                //e_deuda.monto_deuda = (decimal)298.20;
                //e_deuda.monto_deuda = Decimal.Round(Convert.ToDecimal((e_deuda.monto_deuda ?? 0).ToString("N")), 2);
                //e_deuda.numero_documento = "2048010334";
                //e_deuda.moneda = "SOL";
                //e_deuda.simbolo_moneda = "S/";
                //e_deuda.fecha_factura = "2020-07-04";

                //ls_deuda.Add(e_deuda);
                #endregion
            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
            }
            return ls_deuda;

        }
        public object post_pago(string conexion, PagoModel.Pago_Input model)
        {
            bool ins_bd = false;
            bool saldo_comprometido = false;
            bool transaccion_completada = false;
            string id_trans_global = "";

            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;

            string mensaje_error = "";
            GlobalService global_service = new GlobalService();

            //object info_prueba = new object();

            //info_prueba = new
            //{
            //    codigo = "00",
            //    mensaje = "Se realizó el pago correctamente.",
            //    nro_transaccion = "1339"
            //};
            //return info_prueba;

            try
            {
                con_sql.Open();
                hub_convenio = global_service.get_convenio(con_sql, nu_id_convenio);
                con_sql.Close();

                // 3) Obtener ID Transacción y comprometer saldo.
                con_sql.Open();
                var idtran = global_service.get_id_transaccion(con_sql);
                var fechatran = DateTime.Now;

                id_trans_global = idtran.ToString();

                TrxHubModel model_saldo = new TrxHubModel();

                model_saldo.nu_id_trx_hub = Convert.ToInt64(model.id_trx_hub);

                var cmd_saldo = global_service.updTrxhubSaldo(con_sql, model_saldo);

                if (cmd_saldo.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    mensaje_error = cmd_saldo.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                }
                saldo_comprometido = true;

                con_sql.Close();

                DeudaModel.Deuda_Input Deuda_Model = new DeudaModel.Deuda_Input();

                Deuda_Model.numero_servicio = model.numero_servicio;
                Deuda_Model.vc_cod_convenio = model.vc_cod_convenio;

                BanBifApi client = new BanBifApi(hub_convenio);

                var result = client.Consultar_Deuda(Deuda_Model, _logger, model.id_trx_hub).Result;

                Response.E_meta e_meta = (Response.E_meta)result.meta;
                List<Response.E_datos_trx> ls_datos = (List<Response.E_datos_trx>)result.datos;

                string codigo = "";
                string mensaje = "";

                string vc_cod_error_hub = "99";
                string vc_desc_error_hub = "Error desconocido.";

                if (result.timeout == true)
                {
                    con_sql.Open();
                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, "-1", ref vc_cod_error_hub, ref vc_desc_error_hub);
                    con_sql.Close();

                    mensaje_error = vc_desc_error_hub;

                    return UtilSql.sOutPutTransaccion(vc_cod_error_hub, vc_desc_error_hub);
                }

                if (ls_datos == null && e_meta != null)
                {
                    foreach (var item in e_meta.mensajes)
                    {
                        con_sql.Open();
                        global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, item.codigo, ref vc_cod_error_hub, ref vc_desc_error_hub);
                        con_sql.Close();

                        codigo = String.Concat(codigo, vc_cod_error_hub, " - ");
                        mensaje = String.Concat(mensaje, vc_desc_error_hub, " - ");
                    }

                    if (codigo.Length > 0)
                        codigo = codigo.Substring(0, codigo.Length - 3);
                    if (mensaje.Length > 0)
                        mensaje = mensaje.Substring(0, mensaje.Length - 3);

                    mensaje_error = mensaje;

                    return UtilSql.sOutPutTransaccion(codigo, mensaje);
                }

                // 4) Enviar Solicitud al proveedor
                Banbif_PagoModel e_pago = new Banbif_PagoModel();
                foreach (var e_datos in ls_datos)
                {
                    if (model.bi_doc_dinamico == true)
                    {
                        model.numero_documento = e_datos.documento.numero;
                    }

                    if (e_datos.documento.numero == model.numero_documento)
                    {

                        #region Datos de deuda
                        e_pago.recaudador.codigo = config.GetSection("BanBifInfo:codigoRecaudador").Value;

                        e_pago.convenio.codigo = model.vc_cod_convenio;

                        e_pago.cliente.id = e_datos.cliente.id;
                        e_pago.moneda = e_datos.moneda;
                        e_pago.cantidadPagos = 1;
                        e_pago.agrupacion = false;
                        e_pago.montoTotalDeuda = e_datos.montoTotalDestino;
                        e_pago.montoTotalSaldo = e_datos.montoSaldoDestino;

                        //Agregar datos principales
                        Banbif_PagoModel.E_datos e_datos_pago = new Banbif_PagoModel.E_datos();
                        e_datos_pago.fechaVencimiento = e_datos.fechaVencimiento.ToString("yyyy-MM-dd");
                        e_datos_pago.cliente.id = e_datos.cliente.id;
                        e_datos_pago.idConsulta = e_datos.idConsulta.ToString();

                        foreach (var item_serv in e_datos.servicios)
                        {
                            Banbif_PagoModel.E_servicios e_sv = new Banbif_PagoModel.E_servicios();
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

                        Banbif_PagoModel.E_pagos e_p = new Banbif_PagoModel.E_pagos();

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

                        if (e_p.monto != (Convert.ToDecimal(model.importe_pago) - Convert.ToDecimal(model.comision_usuario_final)))
                        {
                            _logger.Error("idtrx: " + model.id_trx_hub + " / " + "La suma de importes a pagar no coincide con los datos de la deuda.");
                            mensaje_error = "La suma de importes a pagar no coincide con los datos de la deuda.";
                            return UtilSql.sOutPutTransaccion("65", "La suma de importes a pagar no coincide con los datos de la deuda.");
                        }

                        e_p.deudaAPagar = e_datos.montoTotalDestino;

                        e_datos_pago.pagos.Add(e_p);

                        #region DatosAdicionales
                        Banbif_PagoModel.E_datosAdicionales e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();

                        foreach (var item_add in e_datos.datosAdicionales)
                        {
                            if (item_add.nombre.ToUpper() == "NOMBRE")
                            {
                                e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                                e_datos_adicionales.nombre = "Nombre";
                                e_datos_adicionales.valor = item_add.valor;
                                e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                            }
                            if (item_add.nombre.ToUpper() == "FECHAEMISION")
                            {
                                e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                                e_datos_adicionales.nombre = "FecEmision";
                                e_datos_adicionales.valor = item_add.valor;
                                e_datos_pago.datosAdicionales.Add(e_datos_adicionales);
                            }
                        }
                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "CODEBRANCH";
                        e_datos_adicionales.valor = config.GetSection("BanBifInfo:CODEBRANCH").Value;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "TRNCODE";
                        e_datos_adicionales.valor = config.GetSection("BanBifInfo:TRNCODE").Value;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "TELLUSERID";
                        e_datos_adicionales.valor = config.GetSection("BanBifInfo:TELLUSERID").Value;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_RED";
                        e_datos_adicionales.valor = config.GetSection("BanBifInfo:Cod_RED").Value;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_TRX";
                        e_datos_adicionales.valor = config.GetSection("BanBifInfo:Cod_TRX").Value;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Des_TRX";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_Terminal";
                        e_datos_adicionales.valor = config.GetSection("BanBifInfo:Cod_Terminal").Value;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_OperaTrace";
                        e_datos_adicionales.valor = idtran.ToString();
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Cod_Autoriza";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Ubi_Terminal";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Nro_Cuenta";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Nro_Tarjeta";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Nro_Cuotas";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "ID_Servicio";
                        e_datos_adicionales.valor = e_datos.servicios[0].id;
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "ID_Recibo";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        e_datos_adicionales = new Banbif_PagoModel.E_datosAdicionales();
                        e_datos_adicionales.nombre = "Nom_EmpServ";
                        e_datos_adicionales.valor = "";
                        e_datos_pago.datosAdicionales.Add(e_datos_adicionales);

                        #endregion

                        e_pago.deudas.Add(e_datos_pago);

                        #endregion
                        BanBifApi client_pago = new BanBifApi(hub_convenio, true);

                        var result_pago = client_pago.Procesar_Pago(e_pago, idtran, _logger, model.id_trx_hub).Result;
                                                                     
                        Response.E_meta e_meta_pago = (Response.E_meta)result_pago.meta;
                        Response.E_datos_trx e_datos_pago_result = (Response.E_datos_trx)result_pago.datos;

                        //Solo para provocar el extorno
                        //result_pago.timeout = true;
                        //e_meta_pago.mensajes[0].codigo = "EEG02";


                        TransaccionModel trx = new TransaccionModel();
                        trx.nu_id_trx = idtran;
                        trx.nu_id_trx_hub = Int64.Parse(model.id_trx_hub);
                        trx.nu_id_distribuidor = int.Parse(model.id_distribuidor);
                        trx.nu_id_comercio = int.Parse(model.id_comercio);
                        trx.dt_fecha = fechatran;
                        trx.nu_id_producto = int.Parse(model.id_servicio);
                        trx.nu_precio = Convert.ToDecimal(model.importe_pago);
                        trx.vc_tran_usua_regi = "API";
                        if (e_datos.moneda == "SOL")
                        {
                            trx.nu_id_tipo_moneda_vta = 1; //SOLES
                        }
                        else
                        {
                            trx.nu_id_tipo_moneda_vta = 2; //DOLARES
                        }
                        trx.vc_numero_servicio = model.numero_servicio;
                        trx.vc_nro_doc_pago = model.numero_documento;
                        trx.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;
                        try { trx.ti_respuesta_api = (result_pago.dt_fin - result_pago.dt_inicio); } catch (Exception ti) { }
                        trx.nu_imp_com_usuario_final = model.comision_usuario_final.ToDecimal();

                        if (e_meta_pago != null &&
                            e_datos_pago_result != null &&
                            e_meta_pago.mensajes[0].codigo == "ESM00")
                        {
                            trx.vc_id_ref_trx = e_meta.idTransaccion;
                            trx.vc_cod_autorizacion = e_datos_pago_result.deudas[0].pagos[0].numeroPago;

                            //Graba BD
                            con_sql.Open();
                            tran_sql = con_sql.BeginTransaction();
                            ins_bd = true;

                            using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_servicios", con_sql, tran_sql))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx);
                                cmd.Parameters.AddWithValue("@nu_id_trx_hub", trx.nu_id_trx_hub);
                                cmd.Parameters.AddWithValue("@nu_id_distribuidor", trx.nu_id_distribuidor);
                                cmd.Parameters.AddWithValue("@nu_id_comercio", trx.nu_id_comercio);
                                cmd.Parameters.AddWithValue("@nu_id_producto", trx.nu_id_producto);
                                cmd.Parameters.AddWithValue("@vc_numero_servicio", trx.vc_numero_servicio);
                                cmd.Parameters.AddWithValue("@vc_nro_doc_pago", trx.vc_nro_doc_pago);
                                cmd.Parameters.AddWithValue("@nu_id_tipo_moneda_vta", trx.nu_id_tipo_moneda_vta);
                                cmd.Parameters.AddWithValue("@nu_precio_vta", trx.nu_precio);
                                cmd.Parameters.AddWithValue("@vc_id_ref_trx_distribuidor", trx.vc_id_ref_trx_distribuidor);
                                cmd.Parameters.AddWithValue("@ti_respuesta_api", trx.ti_respuesta_api);
                                cmd.Parameters.AddWithValue("@nu_imp_com_usuario_final", trx.nu_imp_com_usuario_final);

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
                                cmd_upd.Parameters.AddWithValue("@vc_cod_autorizacion", trx.vc_cod_autorizacion);
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
                            
                            object info = new object();

                            info = new
                            {
                                codigo = "00",
                                mensaje = "Se realizó el pago correctamente.",
                                nro_transaccion = id_trans_global
                            };

                            return info;
                        }
                        else
                        {

                            TransaccionModel tm = new TransaccionModel();
                            tm.nu_id_trx = trx.nu_id_trx;
                            tm.nu_id_trx_hub = trx.nu_id_trx_hub;
                            tm.nu_id_distribuidor = trx.nu_id_distribuidor;
                            tm.nu_id_comercio = trx.nu_id_comercio;
                            tm.dt_fecha = DateTime.Now;
                            tm.nu_id_producto = trx.nu_id_producto;
                            tm.nu_precio = trx.nu_precio;
                            tm.nu_id_tipo_moneda_vta = trx.nu_id_tipo_moneda_vta;
                            tm.vc_numero_servicio = trx.vc_numero_servicio;
                            tm.vc_tran_usua_regi = "API";
                            tm.ti_respuesta_api = trx.ti_respuesta_api;

                            //Solo para extorno Automatico guardar el registro principal
                            if (result_pago.timeout == true || e_meta_pago.mensajes[0].codigo == "EEG02")
                            {
                                con_sql.Open();
                                tran_sql = con_sql.BeginTransaction();
                                ins_bd = true;
                                using (cmd = new SqlCommand("tisi_global.usp_ins_transaccion_servicios", con_sql, tran_sql))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@nu_id_trx", trx.nu_id_trx);
                                    cmd.Parameters.AddWithValue("@nu_id_trx_hub", trx.nu_id_trx_hub);
                                    cmd.Parameters.AddWithValue("@nu_id_distribuidor", trx.nu_id_distribuidor);
                                    cmd.Parameters.AddWithValue("@nu_id_comercio", trx.nu_id_comercio);
                                    cmd.Parameters.AddWithValue("@nu_id_producto", trx.nu_id_producto);
                                    cmd.Parameters.AddWithValue("@vc_numero_servicio", trx.vc_numero_servicio);
                                    cmd.Parameters.AddWithValue("@vc_nro_doc_pago", trx.vc_nro_doc_pago);
                                    cmd.Parameters.AddWithValue("@nu_id_tipo_moneda_vta", trx.nu_id_tipo_moneda_vta);
                                    cmd.Parameters.AddWithValue("@nu_precio_vta", trx.nu_precio);
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
                                tran_sql.Commit();
                                ins_bd = false;
                                con_sql.Close();

                                //Variables BD
                                con_sql.Open();
                                var idtran_reverso = global_service.get_id_transaccion(con_sql);
                                var id_trans_global_reverso = idtran_reverso.ToString();
                                con_sql.Close();

                                ReversarPagoParamModel e_reversar_pago_param = new ReversarPagoParamModel();
                                e_reversar_pago_param.codigoConvenio = e_pago.convenio.codigo;
                                e_reversar_pago_param.codigoMoneda = e_pago.moneda;
                                e_reversar_pago_param.cantidadPagos = e_pago.cantidadPagos;
                                e_reversar_pago_param.agrupacion = e_pago.agrupacion;
                                e_reversar_pago_param.montoTotalDeuda = e_pago.montoTotalDeuda;
                                e_reversar_pago_param.montoTotalSaldo = e_pago.montoTotalSaldo;

                                ReversarPagoModel e_reversar_pago = new ReversarPagoModel();
                                e_reversar_pago.numeroPago = "0";
                                e_reversar_pago.tipoOperacion = e_p.tipoOperacion;
                                e_reversar_pago.medioPago = e_p.medioPago;
                                e_reversar_pago.monto = e_p.monto;


                                //Agregar datos principales
                                ReversarPagoModel.E_datos e_datos_pago_reversar = new ReversarPagoModel.E_datos();
                                e_datos_pago_reversar.fechaVencimiento = e_datos.fechaVencimiento.ToString("yyyy-MM-dd");
                                e_datos_pago_reversar.cliente.id = e_datos.cliente.id;
                                e_datos_pago_reversar.idConsulta = e_datos.idConsulta.ToString();

                                foreach (var item_serv in e_datos.servicios)
                                {
                                    ReversarPagoModel.E_servicios e_sv = new ReversarPagoModel.E_servicios();
                                    e_sv.id = item_serv.id;
                                    e_datos_pago_reversar.servicios.Add(e_sv);
                                }

                                e_datos_pago_reversar.montoSaldoOrigen = e_datos.montoSaldoDestino;
                                e_datos_pago_reversar.montoDescuentoOrigen = e_datos.montoDescuentoDestino;
                                e_datos_pago_reversar.montoMultaOrigen = e_datos.montoMultaDestino;
                                e_datos_pago_reversar.montoVencidoOrigen = e_datos.montoVencidoDestino;
                                e_datos_pago_reversar.montoInteresOrigen = e_datos.montoInteresDestino;
                                e_datos_pago_reversar.montoReajusteOrigen = e_datos.montoReajusteDestino;
                                e_datos_pago_reversar.montoTotalOrigen = e_datos.montoTotalDestino;

                                e_datos_pago_reversar.documento.numero = e_datos.documento.numero;

                                #region DatosAdicionales
                                ReversarPagoModel.E_datosAdicionales e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();

                                e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                e_datos_adicionales_reversar.nombre = "CODEBRANCH";
                                e_datos_adicionales_reversar.valor = config.GetSection("BanBifInfo:CODEBRANCH").Value;
                                e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);

                                e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                e_datos_adicionales_reversar.nombre = "TRNCODE";
                                e_datos_adicionales_reversar.valor = config.GetSection("BanBifInfo:TRNCODE").Value;
                                e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);

                                e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                e_datos_adicionales_reversar.nombre = "TELLUSERID";
                                e_datos_adicionales_reversar.valor = config.GetSection("BanBifInfo:TELLUSERID").Value;
                                e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);

                                e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                e_datos_adicionales_reversar.nombre = "Cod_RED";
                                e_datos_adicionales_reversar.valor = config.GetSection("BanBifInfo:Cod_RED").Value;
                                e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);

                                e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                e_datos_adicionales_reversar.nombre = "Cod_TRX";
                                e_datos_adicionales_reversar.valor = config.GetSection("BanBifInfo:Cod_TRX").Value;
                                e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);

                                e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                e_datos_adicionales_reversar.nombre = "Des_TRX";
                                e_datos_adicionales_reversar.valor = "";
                                e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);

                                foreach (var item_add in e_datos.datosAdicionales)
                                {
                                    if (item_add.nombre.ToUpper() == "NOMBRE")
                                    {
                                        e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                        e_datos_adicionales_reversar.nombre = "NOMBRE";
                                        e_datos_adicionales_reversar.valor = item_add.valor;
                                        e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);
                                    }
                                    if (item_add.nombre.ToUpper() == "OLCAE")
                                    {
                                        e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                        e_datos_adicionales_reversar.nombre = "OLCAE";
                                        e_datos_adicionales_reversar.valor = item_add.valor;
                                        e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);
                                    }
                                }

                                e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                e_datos_adicionales_reversar.nombre = "ESTADODEUDOR";
                                e_datos_adicionales_reversar.valor = "01";
                                e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);

                                foreach (var item_add in e_datos.datosAdicionales)
                                {
                                    if (item_add.nombre.ToUpper() == "DESCRIPCIONDOCPAGO")
                                    {
                                        e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                        e_datos_adicionales_reversar.nombre = "DESCRIPCIONDOCPAGO";
                                        e_datos_adicionales_reversar.valor = item_add.valor;
                                        e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);
                                    }
                                    if (item_add.nombre.ToUpper() == "IMPORTEORIGINALDEUDA")
                                    {
                                        e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                        e_datos_adicionales_reversar.nombre = "IMPORTEORIGINALDEUDA";
                                        e_datos_adicionales_reversar.valor = item_add.valor;
                                        e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);
                                    }
                                    if (item_add.nombre.ToUpper() == "FECHAEMISION")
                                    {
                                        e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                        e_datos_adicionales_reversar.nombre = "FECHAEMISION";
                                        e_datos_adicionales_reversar.valor = item_add.valor;
                                        e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);
                                    }
                                    if (item_add.nombre.ToUpper() == "CANAL")
                                    {
                                        e_datos_adicionales_reversar = new ReversarPagoModel.E_datosAdicionales();
                                        e_datos_adicionales_reversar.nombre = "CANAL";
                                        e_datos_adicionales_reversar.valor = item_add.valor;
                                        e_datos_pago_reversar.datosAdicionales.Add(e_datos_adicionales_reversar);
                                    }
                                }
                                #endregion

                                e_reversar_pago.deudas.Add(e_datos_pago_reversar);

                                TransaccionModel trx_reverso = new TransaccionModel();
                                trx_reverso.nu_id_trx = Convert.ToInt32(id_trans_global_reverso);

                                trx_reverso.nu_id_trx_hub = Convert.ToInt32(model.id_trx_hub);
                                trx_reverso.nu_id_distribuidor = trx.nu_id_distribuidor;
                                trx_reverso.nu_id_comercio = trx.nu_id_comercio;
                                trx_reverso.nu_id_producto = trx.nu_id_producto;
                                trx_reverso.dt_fecha = DateTime.Now.Date;
                                trx_reverso.nu_id_tipo_moneda_vta = trx.nu_id_tipo_moneda_vta;
                                trx_reverso.nu_precio = trx.nu_precio;

                                trx_reverso.vc_numero_servicio = trx.vc_numero_servicio;
                                trx_reverso.vc_nro_doc_pago = trx.vc_nro_doc_pago;
                                trx_reverso.nu_id_trx_ref = trx.nu_id_trx;
                                
                                client = new BanBifApi(hub_convenio);

                                var result_reversion_pago = client.Reversar_Pago(e_reversar_pago_param, e_reversar_pago, trx.nu_id_trx, _logger, model.id_trx_hub).Result;

                                trx_reverso.ti_respuesta_api = (result_reversion_pago.dt_fin - result_reversion_pago.dt_inicio);

                                Response.E_meta e_meta_pago_reversion = (Response.E_meta)result_reversion_pago.meta;
                                Response.E_datos_trx e_datos_pago_result_reversion = (Response.E_datos_trx)result_reversion_pago.datos;
                                if (e_meta_pago_reversion != null &&
                                    e_meta_pago_reversion.mensajes[0].codigo == "ESM00")
                                {
                                    trx_reverso.vc_id_ref_trx = e_meta_pago_reversion.idTransaccion;
                                    trx_reverso.vc_cod_autorizacion = "";
                                    trx_reverso.bi_confirmado = true;
                                    trx_reverso.vc_tran_usua_regi = "API";

                                    con_sql.Open();
                                    var tran_sql_rev = con_sql.BeginTransaction();

                                    cmd = global_service.insTransaccionExtorno(con_sql, tran_sql_rev, trx_reverso);

                                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                                    {
                                        tran_sql_rev.Rollback();
                                        ins_bd = false;
                                        _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                                        mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                                        return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                                    }
                                    trx_reverso.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();

                                    tran_sql_rev.Commit();
                                    con_sql.Close();
                                    ins_bd = false;

                                    tm.nu_id_trx_extorno = trx_reverso.nu_id_trx_app;

                                    tm.vc_cod_error = "-1";
                                    tm.vc_desc_error = "No hubo respuesta del convenio. (Confirmado)";

                                    codigo = "99";
                                    mensaje = tm.vc_desc_error;
                                }
                                else
                                {
                                    trx_reverso.vc_id_ref_trx = "";
                                    trx_reverso.vc_cod_autorizacion = "";
                                    trx_reverso.bi_confirmado = false;
                                    trx_reverso.vc_tran_usua_regi = "API";

                                    con_sql.Open();
                                    var tran_sql_rev = con_sql.BeginTransaction();

                                    cmd = global_service.insTransaccionExtorno(con_sql, tran_sql_rev, trx_reverso);

                                    if (cmd.Parameters["@nu_tran_stdo"].Value.ToString() == "0")
                                    {
                                        tran_sql_rev.Rollback();
                                        ins_bd = false;
                                        _logger.Error("idtrx: " + trx.nu_id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToText());
                                        mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                                        return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                                    }
                                    trx_reverso.nu_id_trx_app = cmd.Parameters["@nu_tran_pkey"].Value.ToDecimal();

                                    tran_sql_rev.Commit();
                                    con_sql.Close();
                                    ins_bd = false;

                                    tm.nu_id_trx_extorno = trx_reverso.nu_id_trx_app;

                                    tm.vc_cod_error = "-1";
                                    tm.vc_desc_error = "No hubo respuesta del convenio. (No confirmado)";

                                    codigo = "99";
                                    mensaje = tm.vc_desc_error;
                                }


                            }

                            
                            if (e_datos_pago_result == null && e_meta_pago != null)
                            {
                                foreach (var item in e_meta_pago.mensajes)
                                {
                                    con_sql.Open();
                                    global_service.get_mensaje_error_hub(con_sql, nu_id_convenio, item.codigo, ref vc_cod_error_hub, ref vc_desc_error_hub);
                                    con_sql.Close();

                                    codigo = String.Concat(tm.vc_cod_error, vc_cod_error_hub, " - ");
                                    mensaje = String.Concat(tm.vc_desc_error, vc_desc_error_hub, " - ");

                                    tm.vc_cod_error = String.Concat(tm.vc_cod_error, item.codigo, " - ");
                                    tm.vc_desc_error = String.Concat(tm.vc_desc_error, item.mensaje, " - ");
                                }

                                if (codigo.Length > 0)
                                    codigo = codigo.Substring(0, codigo.Length - 3);
                                if (mensaje.Length > 0)
                                    mensaje = mensaje.Substring(0, mensaje.Length - 3);

                                if (tm.vc_cod_error.Length > 0)
                                    tm.vc_cod_error = tm.vc_cod_error.Substring(0, tm.vc_cod_error.Length - 3);
                                if (tm.vc_desc_error.Length > 0)
                                    tm.vc_desc_error = tm.vc_desc_error.Substring(0, tm.vc_desc_error.Length - 3);

                            }

                            tm.vc_desc_tipo_error = "CONVENIO";

                            SqlTransaction tran_sql_error = null;
                            con_sql.Open();

                            tran_sql_error = con_sql.BeginTransaction();

                            cmd = global_service.insTransaccionError(con_sql, tran_sql_error, tm);

                            if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                            {
                                tran_sql_error.Rollback();
                                _logger.Error("idtrx: " + model.id_trx_hub + " / " + cmd.Parameters["@tx_tran_mnsg"].Value.ToString());
                                mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                            }

                            tran_sql_error.Commit();
                            con_sql.Close();
                            mensaje_error = tm.vc_desc_error;
                            ins_bd = false;
                            _logger.Error("idtrx: " + model.id_trx_hub + " / " + tm.vc_cod_error + " - " + tm.vc_desc_error);

                            return UtilSql.sOutPutTransaccion(codigo, mensaje);
                        }
                    }

                }

                mensaje_error = "No se encontro deuda con el número de documento " + model.numero_documento;
                return UtilSql.sOutPutTransaccion("69", mensaje_error);
            }
            catch (Exception ex)
            {
                if (ins_bd)
                {
                    tran_sql.Rollback();
                }

                _logger.Error("idtrx: " + model.id_trx_hub + " / " + ex.Message);
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

                    model_saldo_extorno.nu_id_trx_hub = Convert.ToInt64(model.id_trx_hub);
                    model_saldo_extorno.bi_extorno = true;
                    model_saldo_extorno.bi_error = true;
                    model_saldo_extorno.vc_mensaje_error = mensaje_error;
                    var cmd_saldo_extorno = global_service.updTrxhubSaldo(con_sql, model_saldo_extorno);
                    con_sql.Close();
                }

                if (transaccion_completada == false && model.id_trx_hub != "")
                {
                    con_sql.Open();
                    TrxHubModel model_hub_error = new TrxHubModel();

                    model_hub_error.nu_id_trx_hub = Convert.ToInt64(model.id_trx_hub);
                    model_hub_error.vc_mensaje_error = mensaje_error;
                    var cmd_trxhub_error = global_service.updTrxhubError(con_sql, model_hub_error);
                    con_sql.Close();

                }
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
    }
}
