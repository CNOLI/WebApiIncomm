using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class ProductoModel : EntidadBase
    {
        public int? nu_id_producto { get; set; }
        public string vc_cod_producto { get; set; }
        public string vc_desc_producto { get; set; }
        public int? nu_id_convenio { get; set; }
        public int nu_valor_facial { get; set; }
        public string vc_cod_ean { get; set; }
        public string vc_url_imagen { get; set; }
        public int nu_id_categoria { get; set; }
        public int nu_id_tipo_producto { get; set; }
        public int nu_id_sub_tipo_producto { get; set; }
        public bool bi_afecto_impuesto { get; set; }
        public bool bi_estado { get; set; }
        public string vc_desc_categoria { get; set; }
        public bool bi_envio_email { get; set; }
        public bool bi_envio_sms { get; set; }

        public decimal? nu_precio { get; set; }
        public string vc_cod_distribuidor { get; set; }
        public int nu_id_empresa { get; set; }
        public int nu_id_rubro { get; set; }
        public int nu_id_distribuidor { get; set; }
        public bool bi_doc_dinamico { get; set; }
        public decimal? nu_imp_com_usuario_final { get; set; }


    }
}
