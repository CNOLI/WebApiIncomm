using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class TrxHubModel :EntidadBase
    {

        public string codigo_distribuidor { get; set; }

        public string codigo_comercio { get; set; }

        public string nombe_comercio { get; set; }
        public string nro_telefono { get; set; }
        public string email { get; set; }
        public string id_producto { get; set; }
    }
}
