using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Izipay_InputModel
    {

        public class Crear_Comercio_Input
        {
            public decimal? id_distribuidor { get; set; }
        }
        public class Actualizar_Regla_Input
        {
            public decimal? id_distribuidor { get; set; }
            public decimal? id_producto { get; set; }
        }
        public class Pago_Directo_Input
        {            
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string id_producto { get; set; }
            public string numero_servicio { get; set; }
            public string importe_recarga { get; set; }

            public string direccion { get; set; } = "LAS GARZAS 344         ";
            public string nombre_ciudad { get; set; } = "PUEBLO LIBRE ";
            public string codigo_provincia { get; set; } = "SF";
            public string codigo_pais { get; set; } = "PE";

            public string clave { get; set; }
            public string fecha_envio { get; set; }

        }

    }
}

