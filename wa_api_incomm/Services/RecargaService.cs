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
            try
            {
                dynamic obj;

                //IZIPAY
                Pago_Directo_Input model_izipay = new Pago_Directo_Input();
                model_izipay.codigo_distribuidor = model.codigo_distribuidor;
                model_izipay.codigo_comercio = model.codigo_comercio;
                model_izipay.nombre_comercio = model.nombre_comercio;
                model_izipay.id_producto = model.id_producto;
                model_izipay.numero_servicio = model.numero;
                model_izipay.importe_recarga = model.importe;

                IzipayService Izipay_Service = new IzipayService(_logger);

                obj = Izipay_Service.RealizarRecarga(conexion, model_izipay, client_factory);

                return obj;

            }
            catch (Exception ex)
            {

                return UtilSql.sOutPutTransaccion("500", ex.Message);
            }

        }
    }
}
