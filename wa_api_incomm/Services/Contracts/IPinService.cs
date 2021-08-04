using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Services.Contracts
{
    public interface  IPinService
    {
        object get(string key, string pin,string ruta);
    }
}
