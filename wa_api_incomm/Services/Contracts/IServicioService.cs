using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models.Hub;
using static wa_api_incomm.Models.Hub.EmpresaClientModel;
using static wa_api_incomm.Models.Hub.RubroClientModel;
using static wa_api_incomm.Models.Hub.ServicioClientModel;

namespace wa_api_incomm.Services.Contracts
{
    public interface IServicioService
    {
        object sel_rubros(string conexion, RubroClientModelInput input);
        object sel_empresas(string conexion, EmpresaClientModelInput input);
        object sel(string conexion, ServicioClientModelInput model);
    }
}
