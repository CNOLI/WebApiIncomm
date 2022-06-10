using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Megapunto_ResponseModel
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public string id_transaccion { get; set; }
        public string cod_autorizacion { get; set; }
        public DateTime dt_inicio { get; set; }
        public DateTime dt_fin { get; set; }
        public bool timeout { get; set; } = false;

    }
}
    
