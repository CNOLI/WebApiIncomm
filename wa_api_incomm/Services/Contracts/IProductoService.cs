using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models.Hub;

namespace wa_api_incomm.Services.Contracts
{
    public interface IProductoService
    {
        object sel(string conexion, DistribuidorClientModel model);
    }
}
