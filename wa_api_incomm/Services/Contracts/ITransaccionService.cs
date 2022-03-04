using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models.Hub;

namespace wa_api_incomm.Services.Contracts
{
    public interface ITransaccionService
    {
        object execute_trans(string conexion, InputTransModel input);
        object pr_sms(string conexion, string nro_telefono);
        object pr_email(string conexion, string correo_destino);
    }
}
