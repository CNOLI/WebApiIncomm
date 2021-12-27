using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class EmpresaClientModel
    {
        public string id_empresa { get; set; }
        public string ruc { get; set; }
        public string nombre { get; set; }
        public class EmpresaClientModelInput
        {
            public string codigo_distribuidor { get; set; }
            public string id_rubro { get; set; }

        }
    }
}
