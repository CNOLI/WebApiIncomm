using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Sentinel_InputModel
    {
        public class Consultado
        {
            public string tipo_documento_consultado { get; set; }
            public string numero_documento_consultado { get; set; }
        }
        public class Sel_Transaccion
        {
            public string cod_distribuidor { get; set; }
            public string cod_comercio { get; set; }
            public DateTime fec_inicio { get; set; }
            public DateTime fec_final { get; set; }
        }
        public class Ins_Transaccion
        {
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nombre_comercio { get; set; }

            public string id_producto { get; set; }

            public string tipo_documento_consultante { get; set; }
            public string numero_documento_consultante { get; set; }
            public string digito_verificador_consultante { get; set; }
            public string telefono_consultante { get; set; }
            public string email_consultante { get; set; }

            public string tipo_documento_consultado { get; set; }
            public string numero_documento_consultado { get; set; }

            public string tipo_documento_facturacion { get; set; }  // FAC ;  BOL
            public string numero_ruc { get; set; }
            public string tipo_documento_PDV { get; set; }
            public string numero_documento_PDV { get; set; }
            public string razon_social_PDV { get; set; }
            public string clave { get; set; }
            public string fecha_envio { get; set; }
            public string nro_transaccion_referencia { get; set; }
            public bool bono { get; set; } = false;

            //----------------------------------------

            //public decimal? nu_id_comercio { get; set; }

            //public string vc_cod_producto { get; set; }

            //public decimal? nu_id_tipo_doc_sol { get; set; }
            //public string vc_cod_tipo_doc_sol { get; set; }
            //public string vc_nro_doc_sol { get; set; }
            //public string vc_telefono_sol { get; set; }
            //public string vc_email_sol { get; set; }

            //public string vc_cod_tipo_doc_cpt { get; set; }
            //public decimal? nu_id_tipo_doc_cpt { get; set; }
            //public string vc_nro_doc_cpt { get; set; }


            //public string vc_usuario { get; set; }

        }
        public class Busqueda_Sentinel_Input
        {
            public string codigo_distribuidor { get; set; }
            public string buscador { get; set; }

        }
        public class Get_Producto
        {
            public string cod_producto { get; set; }

        }
        public class Get_Distribuidor
        {
            public string cod_distribuidor { get; set; }

        }
    }
}
    
