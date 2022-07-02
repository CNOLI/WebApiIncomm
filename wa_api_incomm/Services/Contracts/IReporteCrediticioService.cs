using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using static wa_api_incomm.Models.RecargaModel;

namespace wa_api_incomm.Services.Contracts
{
    public interface IReporteCrediticioService
    {
        object generar(string conexion, Recarga_Input model, IHttpClientFactory client_factory);
    }
}
