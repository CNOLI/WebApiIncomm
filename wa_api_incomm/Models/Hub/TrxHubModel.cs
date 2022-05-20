using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class TrxHubModel :EntidadBase
    {
        public Int64 nu_id_trx_hub { get; set; }
        public bool bi_extorno { get; set; } = false;
        public string vc_mensaje_error { get; set; }

        public string vc_cod_distribuidor { get; set; }

        public string vc_cod_comercio { get; set; }

        public string vc_nombre_comercio { get; set; }
        public string vc_nro_telefono { get; set; }
        public string vc_email { get; set; }
        public string vc_id_producto { get; set; }
        public decimal? nu_precio_vta { get; set; }


        public int? nu_id_tipo_doc_sol { get; set; }
        public string vc_nro_doc_sol { get; set; }
        public string ch_dig_ver_sol { get; set; }
        public int? nu_id_tipo_doc_cpt { get; set; }
        public string vc_nro_doc_cpt { get; set; }
        public string nu_id_tipo_comprobante { get; set; }
        public string vc_ruc { get; set; }
        public string vc_numero_servicio { get; set; }
        public string vc_nro_doc_pago { get; set; }
        public string vc_id_ref_trx_distribuidor { get; set; }
    }
}
