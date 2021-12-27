using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;

namespace wa_api_incomm.Services.Contracts
{
    public interface ISentinelService
    {
        object sel_tipo_documento_identidad(string conexion, Sentinel_InputModel.Busqueda_Sentinel_Input input);
        object get_validar_titular(string conexion, SentinelInfo info, Sentinel_InputModel.Consultado model);
        object ins_transaccion(string conexion, SentinelInfo info, Sentinel_InputModel.Ins_Transaccion model);
    }
}
