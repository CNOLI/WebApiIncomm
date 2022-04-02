using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;

namespace wa_api_incomm.Services.Contracts
{
    public interface IIncommService
    {
        object execute_trans(string conexion, Incomm_InputTransModel input);
        object confirmar(string conexion, Incomm_InputITransConfirmarModel input);
        object pr_sms(string conexion, string nro_telefono);
        object pr_email(string conexion, string correo_destino);
    }
}
