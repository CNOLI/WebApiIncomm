using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class ReporteCrediticioModel
    {

        public class ReporteCrediticio_Input
        {
            public string clave { get; set; }
            public string fecha_envio { get; set; }
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }
            public string id_producto { get; set; }


            public string tipo_documento_PDV { get; set; }
            public string numero_documento_PDV { get; set; }
            public string razon_social_PDV { get; set; }


            public string tipo_documento_consultante { get; set; }
            public string numero_documento_consultante { get; set; }
            public string digito_verificador_consultante { get; set; }
            public string telefono_consultante { get; set; }
            public string email_consultante { get; set; }

            public string tipo_documento_consultado { get; set; }
            public string numero_documento_consultado { get; set; }

            public string tipo_documento_facturacion { get; set; }  // FAC ;  BOL
            public string numero_ruc { get; set; }




            public string nro_transaccion_referencia { get; set; }
            public bool bono { get; set; } = false;

        }

    }
}

