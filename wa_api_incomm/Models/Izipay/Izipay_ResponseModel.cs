using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Izipay_ResponseModel
    {
        public class ResultApi
        {
            public string descripcion { get; set; }
            public string rc { get; set; }

        }

        public class ResultRegla : ResultApi
        {
            public string id_regla { get; set; }
        }

        public class ResultPagoDirecto
        {
            public string descripcion { get; set; }
            public string rc { get; set; }

            public string codigo_autorizacion { get; set; }
            public string id_pago { get; set; }
            public string nuevo_saldo { get; set; }
            public DateTime dt_inicio { get; set; }
            public DateTime dt_fin { get; set; }
        }

    }
}

