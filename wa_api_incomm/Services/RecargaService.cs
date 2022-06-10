using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Izipay_InputModel;
using System.Net.Http;
using static wa_api_incomm.Models.RecargaModel;
using static wa_api_incomm.Models.ServiPagos.ServiPagos_InputModel;
using static wa_api_incomm.Models.Megapunto_InputModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace wa_api_incomm.Services
{
    public class RecargaService : IRecargaService
    {

        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();


        private readonly Serilog.ILogger _logger;

        public RecargaService(Serilog.ILogger logger)
        {
            _logger = logger;
        }
        public object procesar(string conexion, Recarga_Input model, IHttpClientFactory client_factory)
        {

            SqlConnection con_sql = null;
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            string id_trx_hub = "";
            string id_trans_global = "";
            GlobalService global_service = new GlobalService();

            dynamic obj = null;
            try
            {
                con_sql = new SqlConnection(conexion);

                //1) Inserta TRX_HUB y validaciones por BD

                //Obtener Distribuidor
                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;

                con_sql.Open();
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);
                con_sql.Close();

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El código de distribuidor " + distribuidor.nu_id_distribuidor.ToString() + " no existe");
                    return UtilSql.sOutPutTransaccion("01", "El código de distribuidor no existe");
                }

                //Obtener Comercio
                con_sql.Open();
                ComercioModel comercio = global_service.get_comercio(con_sql, model.codigo_comercio, model.nombre_comercio, distribuidor.nu_id_distribuidor);
                con_sql.Close();
                                             
                //Insertar Transaccion HUB
                con_sql.Open();

                TrxHubModel trx_hub = new TrxHubModel();
                trx_hub.vc_cod_distribuidor = model.codigo_distribuidor;
                trx_hub.vc_cod_comercio = model.codigo_comercio;
                trx_hub.vc_nombre_comercio = model.nombre_comercio;
                trx_hub.vc_nro_telefono = model.numero;
                trx_hub.vc_email = "";
                trx_hub.vc_id_producto = model.id_producto;
                trx_hub.nu_precio_vta = model.importe.ToDecimal();
                trx_hub.vc_numero_servicio = model.numero;
                trx_hub.vc_id_ref_trx_distribuidor = model.nro_transaccion_referencia;

                cmd = global_service.insTrxhub(con_sql, trx_hub);
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
                _logger.Information("idtrx: " + id_trx_hub + " / " + "Modelo recibido: " + JsonConvert.SerializeObject(model));

                // 2) Validar Campos adicionales.

                if (!Regex.Match(model.numero, @"(^[0-9]+$)").Success)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El número del cliente debe ser numerico.");
                    return UtilSql.sOutPutTransaccion("40", "El número del cliente debe ser numerico.");
                }

                if (!Regex.Match(model.importe, @"^[0-9]+(\.[0-9]{1,2})?$").Success)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El importe de recarga debe ser numerico.");
                    return UtilSql.sOutPutTransaccion("41", "El importe de recarga debe ser numerico.");
                }

                if (!Regex.Match(model.id_producto, @"(^[0-9]+$)").Success)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El id del producto debe ser numerico.");
                    return UtilSql.sOutPutTransaccion("04", "El id del producto debe ser numerico.");
                }

                //Obtener Producto
                ProductoModel producto = new ProductoModel();
                producto.nu_id_producto = Convert.ToInt32(model.id_producto);
                producto.nu_id_distribuidor = distribuidor.nu_id_distribuidor;

                con_sql.Open();
                producto = global_service.get_producto(con_sql, producto);
                con_sql.Close();

                if (producto.nu_id_producto <= 0)
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El producto no existe.");
                    return UtilSql.sOutPutTransaccion("05", "El producto no existe");
                }


                //Dirigir a APIS
                if (producto.nu_id_convenio == 5)
                {
                    //IZIPAY
                    Pago_Directo_Input model_izipay = new Pago_Directo_Input();
                    model_izipay.id_trx_hub = id_trx_hub;
                    model_izipay.id_distribuidor = distribuidor.nu_id_distribuidor.ToString();
                    model_izipay.id_comercio = comercio.nu_id_comercio.ToString();
                    model_izipay.id_producto = model.id_producto;
                    model_izipay.vc_cod_producto = producto.vc_cod_producto;
                    model_izipay.numero_servicio = model.numero;
                    model_izipay.importe_recarga = model.importe;
                    model_izipay.nro_transaccion_referencia = model.nro_transaccion_referencia;
                    model_izipay.distribuidor = distribuidor;

                    IzipayService Izipay_Service = new IzipayService(_logger);

                    obj = Izipay_Service.RealizarRecarga(conexion, model_izipay, client_factory);
                }
                else if (producto.nu_id_convenio == 6)
                {
                    //SERVIPAGO
                    ServiPagos_Input model_servipago = new ServiPagos_Input();
                    model_servipago.id_trx_hub = id_trx_hub;
                    model_servipago.id_distribuidor = distribuidor.nu_id_distribuidor.ToString();
                    model_servipago.id_comercio = comercio.nu_id_comercio.ToString();
                    model_servipago.id_producto = model.id_producto;
                    model_servipago.vc_cod_producto = producto.vc_cod_producto;
                    model_servipago.numero_servicio = model.numero;
                    model_servipago.importe_recarga = model.importe;
                    model_servipago.nro_transaccion_referencia = model.nro_transaccion_referencia;

                    ServiPagosService ServiPagos_Service = new ServiPagosService(_logger);

                    obj = ServiPagos_Service.RealizarRecarga(conexion, model_servipago);

                }
                else if (producto.nu_id_convenio == 7)
                {
                    //MEGAPUNTO
                    Megapunto_Input model_megapunto = new Megapunto_Input();
                    model_megapunto.id_trx_hub = id_trx_hub;
                    model_megapunto.id_distribuidor = distribuidor.nu_id_distribuidor.ToString();
                    model_megapunto.id_comercio = comercio.nu_id_comercio.ToString();
                    model_megapunto.id_producto = model.id_producto;
                    model_megapunto.vc_cod_producto = producto.vc_cod_producto;
                    model_megapunto.codigo_comercio = model.codigo_comercio;
                    model_megapunto.numero_servicio = model.numero;
                    model_megapunto.importe_recarga = model.importe;
                    model_megapunto.nro_transaccion_referencia = model.nro_transaccion_referencia;

                    MegapuntoService Megapunto_Service = new MegapuntoService(_logger);

                    obj = Megapunto_Service.RealizarRecarga(conexion, model_megapunto);

                }
                else
                {
                    _logger.Error("idtrx: " + id_trx_hub + " / " + "El producto  " + producto.nu_id_producto.ToString() + " no se encuentra convenio configurado.");
                    return UtilSql.sOutPutTransaccion("XX", "No se encuentra configurado convenio para el producto.");
                }

                return obj;

            }
            catch (Exception ex)
            {
                return UtilSql.sOutPutTransaccion("500", ex.Message);
            }

        }
    }
}
