using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Servicios
{
    public class DeudaModel
    {
        public string numero_documento { get; set; }
        public string cliente { get; set; }
        public string fecha_factura { get; set; }
        public string fecha_vencimento { get; set; }
        public string moneda { get; set; }
        public string simbolo_moneda { get; set; }
        public decimal? monto_documento { get; set; }
        public decimal? comision_cliente { get; set; }
        public decimal? monto_deuda { get; set; }
        public class Deuda_Input
        {
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string id_servicio { get; set; }
            public string numero_servicio { get; set; }
            public string clave { get; set; }
            public string fecha_envio { get; set; }

            public string direccion { get; set; } = "LAS GARZAS 344         ";
            public string nombre_ciudad { get; set; } = "PUEBLO LIBRE ";
            public string codigo_provincia { get; set; } = "SF";
            public string codigo_pais { get; set; } = "PE";

            public string vc_cod_convenio { get; set; }
            public string comision_usuario_final { get; set; }
        }
    }
}
