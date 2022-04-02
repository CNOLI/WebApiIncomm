using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;

namespace wa_api_incomm.Services.Contracts
{
    public interface IConsultaService
    {
        object Transaccion(string conexion, Consulta.Transaccion_Input model);
    }
}
