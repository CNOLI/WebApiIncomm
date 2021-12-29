using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class RecargaModel
    {

        public class Recarga_Input
        {
            public string clave { get; set; }
            public string fecha_envio { get; set; }
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string id_producto { get; set; }
            public string numero { get; set; }
            public string importe { get; set; }

        }

    }
}

