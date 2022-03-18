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

            dynamic obj = null;
            try
            {
                con_sql = new SqlConnection(conexion);

                GlobalService global_service = new GlobalService();
                                
                //Insertar Transaccion HUB
                con_sql.Open();

                TrxHubModel trx_hub = new TrxHubModel();
                trx_hub.codigo_distribuidor = model.codigo_distribuidor;
                trx_hub.codigo_comercio = model.codigo_comercio;
                trx_hub.nombre_comercio = model.nombre_comercio;
                trx_hub.nro_telefono = model.numero;
                trx_hub.email = "";
                trx_hub.id_producto = model.id_producto;

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


                //Obtener Distribuidor
                DistribuidorModel distribuidor = new DistribuidorModel();
                distribuidor.vc_cod_distribuidor = model.codigo_distribuidor;

                con_sql.Open();
                distribuidor = global_service.get_distribuidor(con_sql, distribuidor);
                con_sql.Close();

                if (distribuidor.nu_id_distribuidor <= 0)
                {
                    return UtilSql.sOutPutTransaccion("05", "El código de distribuidor no existe");
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
                    return UtilSql.sOutPutTransaccion("06", "El producto no existe");
                }

                //Dirigir a APIS
                if (producto.nu_id_convenio == 5)
                {
                    //IZIPAY
                    Pago_Directo_Input model_izipay = new Pago_Directo_Input();
                    model_izipay.id_trx_hub = id_trx_hub;
                    model_izipay.codigo_distribuidor = model.codigo_distribuidor;
                    model_izipay.codigo_comercio = model.codigo_comercio;
                    model_izipay.nombre_comercio = model.nombre_comercio;
                    model_izipay.id_producto = model.id_producto;
                    model_izipay.numero_servicio = model.numero;
                    model_izipay.importe_recarga = model.importe;

                    IzipayService Izipay_Service = new IzipayService(_logger);

                    obj = Izipay_Service.RealizarRecarga(conexion, model_izipay, client_factory);
                }
                else if (producto.nu_id_convenio == 6)
                {
                    //SERVIPAGO
                    ServiPagos_Input model_servipago = new ServiPagos_Input();
                    model_servipago.id_trx_hub = id_trx_hub;
                    model_servipago.codigo_distribuidor = model.codigo_distribuidor;
                    model_servipago.codigo_comercio = model.codigo_comercio;
                    model_servipago.nombre_comercio = model.nombre_comercio;
                    model_servipago.id_producto = model.id_producto;
                    model_servipago.numero_servicio = model.numero;
                    model_servipago.importe_recarga = model.importe;

                    ServiPagosService ServiPagos_Service = new ServiPagosService(_logger);

                    obj = ServiPagos_Service.RealizarRecarga(conexion, model_servipago);

                }
                else
                {
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
