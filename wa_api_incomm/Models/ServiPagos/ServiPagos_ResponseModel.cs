using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.ServiPagos
{
    public class ServiPagos_ResponseModel
    {
        public Solicitud solicitud { get; set; }
        public Respuesta respuesta { get; set; }
        public class Solicitud
        {
            public string producto { get; set; }
            public string numero { get; set; }
            public string monto { get; set; }
        }
        public class Respuesta
        {
            public string resultado { get; set; }
            public string fechahora { get; set; }
            public string transacid { get; set; }
            public string obs { get; set; }
            public string nro_op { get; set; }
        }
    }
}

