using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class RubroClientModel
    {
        public string id_rubro { get; set; }
        public string codigo_rubro { get; set; }
        public string nombre_rubro { get; set; }
        public class RubroClientModelInput
        {
            public string codigo_distribuidor { get; set; }

        }
    }
}
