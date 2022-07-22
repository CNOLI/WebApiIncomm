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

        public class ResultPago
        {
            public string descripcion { get; set; }
            public string rc { get; set; }

            public string codigo_autorizacion { get; set; }
            public string id_pago { get; set; }
            public string nuevo_saldo { get; set; }
            public bool timeout { get; set; } = false;
            public DateTime dt_inicio { get; set; }
            public DateTime dt_fin { get; set; }
        }
        public class ResultConsultarRecibos
        {
            public string descripcion { get; set; }
            public string rc { get; set; }

            public List<Recibo> recibos { get; set; }
            public string numero_cliente { get; set; }
            public string nombre_cliente { get; set; }
            public string codigo_autorizacion { get; set; }
            public DateTime dt_inicio { get; set; }
            public DateTime dt_fin { get; set; }
            public bool timeout { get; set; } = false;
        }
        public class Recibo
        {
            public string numero_recibo { get; set; }
            public string cod_moneda { get; set; }
            public string fecha_recibo { get; set; }
            public decimal? saldo_recibo { get; set; }
            public decimal? importe_com_cli { get; set; }
            public decimal? total_a_pagar { get; set; }

        }

    }
}

