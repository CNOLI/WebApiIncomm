using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class ServicioModel
    {
        public string id_servicio { get; set; }
        public string nombre_servicio { get; set; }
        public class ServicioModelInput
        {
            public string codigo_distribuidor { get; set; }
            public string id_empresa { get; set; }

        }
        public class ServicioObtenerDeudaPagoModelInput
        {
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string id_servicio { get; set; }
            public string numero_servicio { get; set; }
            public string clave { get; set; }
            public string fecha_envio { get; set; }
        }
        public class ServicioProcesarPagoModelInput
        {
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string id_servicio { get; set; }
            public string numero_servicio { get; set; }
            public string numero_documento { get; set; }
            public string importe_pago { get; set; }
            public string clave { get; set; }
            public string fecha_envio { get; set; }
        }
    }
}
