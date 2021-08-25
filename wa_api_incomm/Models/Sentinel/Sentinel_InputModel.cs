using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Sentinel_InputModel
    {
        public class Solicitante
        {
            public string cod_tip_doc_solicitante { get; set; }
            public string nro_doc_solicitante { get; set; }
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
            public string cod_distribuidor { get; set; }
            public string cod_comercio { get; set; }

            public string cod_producto { get; set; }

            public string cod_tip_doc_solicitante { get; set; }
            public string nro_doc_solicitante { get; set; }
            public string dig_ver_solicitante { get; set; }
            public string tel_solicitante { get; set; }
            public string email_solicitante { get; set; }

            public string cod_tip_doc_consultado { get; set; }
            public string nro_doc_consultado { get; set; }

            public string tip_doc_facturacion { get; set; }  // FAC ;  BOL
            public string nro_ruc { get; set; }


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
        public class Busqueda
        {
            public string cod_distribuidor { get; set; }
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
    
