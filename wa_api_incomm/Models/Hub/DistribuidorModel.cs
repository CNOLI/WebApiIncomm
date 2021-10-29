using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class DistribuidorModel : EntidadBase
    {
        public int      nu_id_distribuidor  { get; set; }
        public string   vc_cod_distribuidor { get; set; }
        public string   vc_desc_distribuidor { get; set; }
        public string   vc_zip_code         { get; set; }
        public decimal? nu_saldo            { get; set; }
        public string   vc_nombre_contacto { get; set; }
        public string   vc_email_contacto { get; set; }
        public string   vc_celular_contacto { get; set; }
        public bool?    bi_izipay { get; set; }
        public string   vc_contraseña { get; set; }
        public int?      nu_id_comercio { get; set; }

    }
}
