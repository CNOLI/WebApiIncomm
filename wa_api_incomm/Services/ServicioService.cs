using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.BanBif;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Models.Servicios;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Hub.EmpresaClientModel;
using static wa_api_incomm.Models.Hub.RubroClientModel;
using static wa_api_incomm.Models.Hub.ServicioModel;
using static wa_api_incomm.Models.Izipay_InputModel;

namespace wa_api_incomm.Services
{
    public class ServicioService : IServicioService
    {

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        private readonly Serilog.ILogger _logger;

        public ServicioService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object obtenerRubros(string conexion, RubroClientModelInput input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor_rubros", cn))
                    {
                        List<RubroClientModel> ls = new List<RubroClientModel>();

                        ProductoModel model = new ProductoModel();
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 1;
                        model.vc_cod_distribuidor = input.codigo_distribuidor;
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                        UtilSql.iGet(cmd, model);
                        SqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            RubroClientModel m = new RubroClientModel();
                            if (Ec(dr, "nu_id_rubro"))
                                m.id_rubro = dr["nu_id_rubro"].ToString();
                            if (Ec(dr, "vc_cod_rubro"))
                                m.codigo_rubro = dr["vc_cod_rubro"].ToString();
                            if (Ec(dr, "vc_desc_rubro"))
                                m.nombre_rubro = dr["vc_desc_rubro"].ToString();

                            ls.Add(m);
                        }
                        return ls;

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }
        public object obtenerEmpresas(string conexion, EmpresaClientModelInput input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor_empresas", cn))
                    {
                        List<EmpresaClientModel> ls = new List<EmpresaClientModel>();

                        ProductoModel model = new ProductoModel();
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 1;
                        model.vc_cod_distribuidor = input.codigo_distribuidor;
                        if (!String.IsNullOrEmpty(input.id_rubro))
                        {
                            model.nu_id_rubro = int.Parse(input.id_rubro);
                        }
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                        cmd.Parameters.AddWithValue("@nu_id_rubro", model.nu_id_rubro);
                        UtilSql.iGet(cmd, model);
                        SqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            EmpresaClientModel m = new EmpresaClientModel();
                            if (Ec(dr, "nu_id_empresa"))
                                m.id_empresa = dr["nu_id_empresa"].ToString();
                            if (Ec(dr, "vc_nro_doc_identidad"))
                                m.ruc_empresa = dr["vc_nro_doc_identidad"].ToString();
                            if (Ec(dr, "vc_nombre"))
                                m.nombre_empresa = dr["vc_nombre"].ToString();

                            ls.Add(m);
                        }
                        return ls;

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }
        public object obtenerServicios(string conexion, ServicioModelInput input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor_producto", cn))
                    {
                        ProductoModel model = new ProductoModel();
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 3;
                        model.vc_cod_distribuidor = input.codigo_distribuidor;
                        if (!String.IsNullOrEmpty(input.id_empresa))
                        {
                            model.nu_id_empresa = int.Parse(input.id_empresa);
                        }
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                        cmd.Parameters.AddWithValue("@nu_id_empresa", model.nu_id_empresa);
                        UtilSql.iGet(cmd, model);
                        SqlDataReader dr = cmd.ExecuteReader();
                        return Query(dr);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }

        public object obtenerDeuda(string conexion, ServicioObtenerDeudaPagoModelInput input, IHttpClientFactory client_factory)
        {
            SqlConnection con_sql = null;

            dynamic obj = null;

            try
            {
                con_sql = new SqlConnection(conexion);

                GlobalService global_service = new GlobalService();

                //Obtener Distribuidor
                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = input.codigo_distribuidor;

                con_sql.Open();
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);
                con_sql.Close();

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("01", "El código de distribuidor no existe.");
                }

                //Obtener Producto
                ProductoModel producto = new ProductoModel();
                producto.nu_id_producto = Convert.ToInt32(input.id_servicio);
                producto.nu_id_distribuidor = distribuidor.nu_id_distribuidor;

                con_sql.Open();
                producto = global_service.get_producto(con_sql, producto);
                con_sql.Close();

                if (producto.nu_id_producto <= 0)
                {
                    return UtilSql.sOutPutTransaccion("04", "El producto no existe.");
                }

                //Dirigir a APIS
                if (producto.nu_id_convenio == 3)
                {
                    //BANBIF
                    DeudaModel.Deuda_Input model_banbif = new DeudaModel.Deuda_Input();
                    model_banbif.codigo_distribuidor = input.codigo_distribuidor;
                    model_banbif.codigo_comercio = input.codigo_comercio;
                    model_banbif.nombre_comercio = input.nombre_comercio;
                    model_banbif.vc_cod_convenio = producto.vc_cod_producto;
                    model_banbif.numero_servicio = input.numero_servicio;
                    model_banbif.comision_usuario_final = producto.nu_imp_com_usuario_final.ToString();
                    BanBifService Banbif_Service = new BanBifService(_logger);
                    obj = Banbif_Service.get_deuda(conexion, model_banbif);

                }
                else if (producto.nu_id_convenio == 5)
                {
                    //IZIPAY
                    DeudaModel.Deuda_Input model_izipay = new DeudaModel.Deuda_Input();
                    model_izipay.codigo_distribuidor = input.codigo_distribuidor;
                    model_izipay.codigo_comercio = input.codigo_comercio;
                    model_izipay.nombre_comercio = input.nombre_comercio;
                    model_izipay.vc_cod_convenio = producto.vc_cod_producto;
                    model_izipay.numero_servicio = input.numero_servicio;
                    model_izipay.comision_usuario_final = producto.nu_imp_com_usuario_final.ToString();
                    IzipayService Izipay_Service = new IzipayService(_logger);
                    obj = Izipay_Service.ConsultaRecibos(conexion, model_izipay, client_factory);

                }
                else
                {
                    return UtilSql.sOutPutTransaccion("05", "El producto no está habilitado para el distribuidor.");
                }

                return obj;

            }
            catch (Exception ex)
            {

                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }
        public object procesarPago(string conexion, ServicioProcesarPagoModelInput input, IHttpClientFactory client_factory)
        {
            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string id_trx_hub = "";
            string id_trans_global = "";
            GlobalService global_service = new GlobalService();

            dynamic obj = null;
            string mensaje_error = "";
            try
            {
                con_sql = new SqlConnection(conexion);

                //1) Inserta TRX_HUB y validaciones por BD

                //Obtener Distribuidor
                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = input.codigo_distribuidor;

                con_sql.Open();
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);
                con_sql.Close();

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    mensaje_error = "El código de distribuidor " + distribuidor.nu_id_distribuidor.ToString() + " no existe";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("01", mensaje_error);
                }

                //Obtener Comercio
                con_sql.Open();
                ComercioModel comercio = global_service.get_comercio(con_sql, input.codigo_comercio, input.nombre_comercio, distribuidor.nu_id_distribuidor);
                con_sql.Close();

                //Insertar Transaccion HUB
                con_sql.Open();

                TrxHubModel trx_hub = new TrxHubModel();
                trx_hub.vc_cod_distribuidor = input.codigo_distribuidor;
                trx_hub.vc_cod_comercio = input.codigo_comercio;
                trx_hub.vc_nombre_comercio = input.nombre_comercio;
                trx_hub.vc_nro_telefono = "";
                trx_hub.vc_email = "";
                trx_hub.vc_id_producto = input.id_servicio;
                trx_hub.nu_precio_vta = input.importe_pago.ToDecimal();
                trx_hub.vc_numero_servicio = input.numero_servicio;
                trx_hub.vc_nro_doc_pago = input.numero_documento;
                trx_hub.vc_id_ref_trx_distribuidor = input.nro_transaccion_referencia;

                cmd = global_service.insTrxhub(con_sql, trx_hub);
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                {
                    mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
                }
                if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 2)
                {
                    string codigo_error = cmd.Parameters["@vc_tran_codi"].Value.ToText();
                    mensaje_error = cmd.Parameters["@tx_tran_mnsg"].Value.ToText();
                    _logger.Error(mensaje_error);
                    return UtilSql.sOutPutTransaccion(codigo_error, mensaje_error);
                }

                id_trx_hub = cmd.Parameters["@nu_tran_pkey"].Value.ToString();

                con_sql.Close();

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Inicio de transaccion");

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Modelo recibido: " + JsonConvert.SerializeObject(input));

                // 2) Validar Campos adicionales.

                //Obtener Producto
                ProductoModel producto = new ProductoModel();
                producto.nu_id_producto = Convert.ToInt32(input.id_servicio);
                producto.nu_id_distribuidor = distribuidor.nu_id_distribuidor;

                con_sql.Open();
                producto = global_service.get_producto(con_sql, producto);
                con_sql.Close();

                if (producto.nu_id_producto <= 0)
                {
                    mensaje_error = "El producto no existe";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("04", mensaje_error);
                }

                if (string.IsNullOrEmpty(input.numero_documento))
                {
                    mensaje_error = "Debe indicar el número de documento de pago.";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("32", mensaje_error);
                }

                //Dirigir a APIS
                if (producto.nu_id_convenio == 3)
                {
                    //BANBIF
                    PagoModel.Pago_Input model_banbif = new PagoModel.Pago_Input();
                    model_banbif.id_trx_hub = id_trx_hub;
                    model_banbif.id_distribuidor = distribuidor.nu_id_distribuidor.ToString();
                    model_banbif.id_comercio = comercio.nu_id_comercio.ToString();
                    model_banbif.id_servicio = input.id_servicio;
                    model_banbif.vc_cod_convenio = producto.vc_cod_producto;
                    model_banbif.bi_doc_dinamico = producto.bi_doc_dinamico;
                    model_banbif.numero_servicio = input.numero_servicio;
                    model_banbif.numero_documento = input.numero_documento;
                    model_banbif.importe_pago = input.importe_pago;
                    model_banbif.comision_usuario_final = producto.nu_imp_com_usuario_final.ToString();
                    model_banbif.nro_transaccion_referencia = input.nro_transaccion_referencia;
                    BanBifService Banbif_Service = new BanBifService(_logger);
                    obj = Banbif_Service.post_pago(conexion, model_banbif);
                }
                else if (producto.nu_id_convenio == 5)
                {
                    //IZIPAY
                    PagoModel.Pago_Input model_izipay = new PagoModel.Pago_Input();
                    model_izipay.id_trx_hub = id_trx_hub;
                    model_izipay.id_distribuidor = distribuidor.nu_id_distribuidor.ToString();
                    model_izipay.id_comercio = comercio.nu_id_comercio.ToString();
                    model_izipay.id_servicio = input.id_servicio;
                    model_izipay.vc_cod_convenio = producto.vc_cod_producto;
                    model_izipay.numero_servicio = input.numero_servicio;
                    model_izipay.numero_documento = input.numero_documento;
                    model_izipay.importe_pago = input.importe_pago;
                    model_izipay.comision_usuario_final = producto.nu_imp_com_usuario_final.ToString();
                    model_izipay.nro_transaccion_referencia = input.nro_transaccion_referencia;
                    IzipayService Izipay_Service = new IzipayService(_logger);
                    obj = Izipay_Service.RealizarPago(conexion, model_izipay, client_factory);
                }
                else
                {
                    mensaje_error = "El producto no está habilitado para el distribuidor.";
                    _logger.Error("idtrx: " + id_trx_hub + " / " + mensaje_error);
                    return UtilSql.sOutPutTransaccion("05", mensaje_error);
                }

                _logger.Information("idtrx: " + id_trx_hub + " / " + "Modelo enviado: " + JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                return obj;

            }
            catch (Exception ex)
            {

                mensaje_error = ex.Message;
                return UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos.");
            }
            finally
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();

                if (mensaje_error != "" && id_trx_hub != "")
                {
                    con_sql.Open();
                    TrxHubModel model_hub_error = new TrxHubModel();

                    model_hub_error.nu_id_trx_hub = Convert.ToInt64(id_trx_hub);
                    model_hub_error.vc_mensaje_error = mensaje_error;
                    var cmd_trxhub_error = global_service.updTrxhubError(con_sql, model_hub_error);
                    con_sql.Close();

                }

            }

        }

        private List<ServicioModel> Query(SqlDataReader or)
        {
            var lm = new List<ServicioModel>();
            while (or.Read())
            {
                lm.Add(Decode(or));
            }
            return lm;
        }

        private ServicioModel Decode(SqlDataReader or)
        {
            ServicioModel m = new ServicioModel();
            if (Ec(or, "nu_id_producto"))
                m.id_servicio = or["nu_id_producto"].ToString();
            if (Ec(or, "vc_desc_producto"))
                m.nombre_servicio = or["vc_desc_producto"].ToString();

            return m;
        }

        public static bool Ec(IDataReader or, string columna)
        {
            or.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + columna + "'";
            return (or.GetSchemaTable().DefaultView.Count > 0);
        }



    }
}
