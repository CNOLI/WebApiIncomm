using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.ServiPagos
{
    public class ServiPagos_InputModel
    {
        public string vc_cod_producto { get; set; }
        public string vc_numero_servicio { get; set; }
        public string nu_precio_vta { get; set; }
        public class ServiPagos_Input
        {
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string id_producto { get; set; }
            public string numero_servicio { get; set; }
            public string importe_recarga { get; set; }

            public string clave { get; set; }
            public string fecha_envio { get; set; }
            public string nro_transaccion_referencia { get; set; }


            public string id_trx_hub { get; set; }
            public string id_distribuidor { get; set; }
            public string id_comercio { get; set; }
        }
    }
}

