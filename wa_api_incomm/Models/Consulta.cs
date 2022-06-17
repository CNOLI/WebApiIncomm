using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Consulta
    {
        public class Transaccion_Input
        {
            public string fecha_envio { get; set; }
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nro_transaccion { get; set; }
            public string clave { get; set; }

        }
        public class Transaccion_Input_Estado
        {
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nro_transaccion_referencia { get; set; }
        }
        public class Distribuidor_Saldo_Input
        {
            public string codigo_distribuidor { get; set; }
        }
        public class Transaccion_Result
        {
            public string nro_transaccion { get; set; }
            public string codigo_distribuidor { get; set; }
            public string nombre_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string fecha { get; set; }
            public string nombre_producto { get; set; }
            public string precio_venta { get; set; }
            public string valor_comision { get; set; }
            public string importe { get; set; }
            public string nro_referencia { get; set; }
            public string nro_autorizacion { get; set; }
            public string tipo_documento_consultante { get; set; }
            public string nro_documento_consultante { get; set; }
            public string telefono_consultante { get; set; }
            public string email_consultante { get; set; }
            public string tipo_documento_consultado { get; set; }
            public string nro_documento_consultado { get; set; }
            public string tipo_documento_facturacion { get; set; }
            public string nro_ruc { get; set; }
            public string nro_servicio { get; set; }
            public string nro_documento_pago { get; set; }
            public string fecha_registro { get; set; }

        }
    }
}
