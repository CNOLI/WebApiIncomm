using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Equifax_ResponseModel
    {
        public class E_Response_Generar_Reporte
        {
            public string message { get; set; }
            public string code { get; set; }
            public string data { get; set; }
            public string[] errors { get; set; }
        }
        public class E_Response_Trx
        {
            public int? nu_tran_stdo { get; set; }
            public int? nu_tran_pkey { get; set; }
            public string vc_tran_codi { get; set; }
            public string tx_tran_mnsg { get; set; }
        }

    }
}
    
