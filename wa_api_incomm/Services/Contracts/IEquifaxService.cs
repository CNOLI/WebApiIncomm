using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using static wa_api_incomm.Models.EquifaxModel;

namespace wa_api_incomm.Services.Contracts
{
    public interface IEquifaxService
    {
        object sel_tipo_documento_identidad(string conexion, Equifax_InputModel.Busqueda_Equifax_Input input);
        object GenerarReporte(string conexion, GenerarReporte model);
    }
}
