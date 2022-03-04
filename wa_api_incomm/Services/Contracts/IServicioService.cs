using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models.Hub;
using static wa_api_incomm.Models.Hub.EmpresaClientModel;
using static wa_api_incomm.Models.Hub.RubroClientModel;
using static wa_api_incomm.Models.Hub.ServicioModel;

namespace wa_api_incomm.Services.Contracts
{
    public interface IServicioService
    {
        object obtenerRubros(string conexion, RubroClientModelInput input);
        object obtenerEmpresas(string conexion, EmpresaClientModelInput input);
        object obtenerServicios(string conexion, ServicioModelInput model);
        object obtenerDeuda(string conexion, ServicioObtenerDeudaPagoModelInput input);
        object procesarPago(string conexion, ServicioProcesarPagoModelInput model);
    }
}
