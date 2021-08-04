using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;

namespace wa_api_incomm.Services.Contracts
{
    public interface ICategoriesService
    {
        object sel(string conexion);
    }
}
