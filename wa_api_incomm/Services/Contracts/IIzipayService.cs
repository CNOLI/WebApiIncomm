using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Servicios;
using static wa_api_incomm.Models.Izipay_InputModel;

namespace wa_api_incomm.Services.Contracts
{
    public interface IIzipayService
    {
        object CrearComercio(string conexion, Crear_Comercio_Input model);
        object ActualizarRegla(string conexion, Actualizar_Regla_Input model);
        object RealizarRecarga(string conexion, Pago_Directo_Input model, IHttpClientFactory client_factory);
        object ConsultaRecibos(string conexion, DeudaModel.Deuda_Input model, IHttpClientFactory client_factory);
    }
}
