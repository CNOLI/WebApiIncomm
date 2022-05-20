using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class ResultTransaccionIncomm
    {
        public string transactionId { get; set; }
        public string amount { get; set; }
        public string iva { get; set; }
        public string ivaValue { get; set; }
        public string valueBeforeIva { get; set; }
        public string errorCode { get; set; }
        public string errorType { get; set; }
        
        public string errorMessage { get; set; }
        public string authorizationCode { get; set; }
        public string pin { get; set; }
        public DateTime dt_inicio { get; set; }
        public DateTime dt_fin { get; set; }

    }
}
