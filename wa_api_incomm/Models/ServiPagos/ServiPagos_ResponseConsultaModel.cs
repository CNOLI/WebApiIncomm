using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.ServiPagos
{
    public class ServiPagos_ResponseConsultaModel
    {
        public Consulta consulta { get; set; }
        public Respuesta respuesta { get; set; }
        public bool timeout { get; set; } = false;
        public class Solicitud
        {
            public string tipo { get; set; }
            public string parametro { get; set; }
            public string valor { get; set; }
        }
        public class Respuesta
        {
            public string resultado { get; set; }
            public string fechahora { get; set; }
            public string obs { get; set; }
            public Datos datos { get; set; }
        }
        public class Datos
        {
            public string fechahora { get; set; }
            public string transacid { get; set; }
            public string producto_id { get; set; }
            public string msgid { get; set; }
            public string numero { get; set; }
            public string monto { get; set; }
            public string resultado { get; set; }
            public string nro_op { get; set; }
        }
    }
}

