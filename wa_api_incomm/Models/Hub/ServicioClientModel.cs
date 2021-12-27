using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class ServicioClientModel
    {
        public string id_servicio { get; set; }
        public string nombre_servicio { get; set; }
        public class ServicioClientModelInput
        {
            public string codigo_distribuidor { get; set; }
            public string id_empresa { get; set; }

        }
    }
}
