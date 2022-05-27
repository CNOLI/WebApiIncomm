using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models.Hub;

namespace wa_api_incomm.Services.Contracts
{
    public interface ITransaccionService
    {
        object Confirmar(string conexion, TransaccionModel.Transaccion_Input_Confirmar model);
        object Informar(string conexion, TransaccionModel.Transaccion_Input_Confirmar model);
    }
}
