using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class IncommModel :EntidadBase
    {
        public long nu_id_trx { get; set; }
        public long nu_id_trx_hub { get; set; }
        public int nu_id_distribuidor { get; set; }
        public int nu_id_comercio { get; set; }

        public string vc_url_api_aes { get; set; }
        public string vc_clave_aes { get; set; }
        public string vc_aws_access_key_id { get; set; }
        public string vc_aws_secrect_access_key { get; set; }
        public string vc_desc_empresa { get; set; }
        public string vc_color_header_email { get; set; }
        public string vc_color_body_email { get; set; }
        public string vc_email_envio { get; set; }
        public string vc_password_email { get; set; }
        public string vc_smtp_email { get; set; }
        public int? nu_puerto_smtp_email { get; set; }
        public bool? bi_ssl_email { get; set; }
        public DateTime? dt_fec_reg { get; set; }
        public string vc_telefono_sol { get; set; }
        public string vc_email_sol { get; set; }
        public decimal? nu_precio_vta { get; set; }
        public string vc_nro_pin { get; set; }
        public string vc_id_ref_trx { get; set; }
        public string vc_cod_autorizacion { get; set; }

        public string vc_cod_comercio { get; set; }
        public string vc_desc_categoria { get; set; }
        public string vc_desc_producto { get; set; }
        public string vc_url_web_terminos { get; set; }
        public bool? bi_valor_pin { get; set; }
        public bool? bi_confirmado { get; set; }
        public bool? bi_informado { get; set; }

    }
}
