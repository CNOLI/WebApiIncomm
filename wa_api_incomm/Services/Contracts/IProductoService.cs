using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models.Hub;
using static wa_api_incomm.Models.Hub.ProductoClientModel;

namespace wa_api_incomm.Services.Contracts
{
    public interface IProductoService
    {
        object sel(string conexion, ProductoClientModelInput model);
    }
}
