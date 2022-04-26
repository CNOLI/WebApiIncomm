using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class EquifaxModel : EntidadBase
    {
        public decimal? nu_id_trx_app { get; set; }
        public string vc_id_ref_trx { get; set; }
        public class EquifaxInfo
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class GenerarReporte
        {

            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string id_producto { get; set; }
            public string tipo_documento_consultante { get; set; }
            public string numero_documento_consultante { get; set; }
            public string email_consultante { get; set; }
            public string digito_verificador_consultante { get; set; }
            public string tipo_documento_consultado { get; set; }
            public string numero_documento_consultado { get; set; }
            public string clave { get; set; }
            public string fecha_envio { get; set; }
            public string nro_transaccion_referencia { get; set; }

        }

        public class ConsultarReporte
        {
            public string nro_transaccion { get; set; }

        }
    }

}
