using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using static wa_api_incomm.Models.ReporteCrediticioModel;

namespace wa_api_incomm.Services.Contracts
{
    public interface IReporteCrediticioService
    {
        object validar_persona(string conexion, SentinelInfo info, Sentinel_InputModel.Consultado model);

        object generar(string conexion, ReporteCrediticio_Input model, SentinelInfo info, IHttpClientFactory client_factory);
    }
}
