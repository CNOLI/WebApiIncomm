using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class ProductoClientModel
    {
        public string id_producto { get; set; }
        public string nombre_producto { get; set; }
        public string precio { get; set; }
        public class ProductoClientModelInput
        {
            public string id_producto { get; set; }
            public string codigo_distribuidor { get; set; }
            public string id_convenio { get; set; }

        }
    }
}
