using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;

namespace wa_api_incomm.Services.Contracts
{
    public interface ISentinelService
    {
        //object sel_banco(string conexion, Sentinel_InputModel.Busqueda input);
        object sel_tipo_documento_identidad(string conexion, Sentinel_InputModel.Busqueda_Sentinel_Input input);
        object sel_producto(string conexion, Sentinel_InputModel.Busqueda_Sentinel_Input input);
        //object get_precio_producto(string conexion, Sentinel_InputModel.Get_Producto input);
        //object get_saldo_distribuidor(string conexion, Sentinel_InputModel.Get_Distribuidor input);
        //object sel_transaccion(string conexion, Sentinel_InputModel.Sel_Transaccion model);


        object get_validar_titular(string conexion, SentinelInfo info, Sentinel_InputModel.Consultado model);
        object ins_transaccion(string conexion, SentinelInfo info, Sentinel_InputModel.Ins_Transaccion model);
    }
}
