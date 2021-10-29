using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class DistribuidorProductoModel : EntidadBase
    {
        public int      nu_id_distribuidor      { get; set; }
        public int      nu_id_producto          { get; set; }
        public string   vc_cod_producto         { get; set; }
        public string   vc_desc_producto        { get; set; }
        public string   ch_tipo_comision        { get; set; }
        public decimal? nu_valor_comision       { get; set; }
        public bool?    bi_envio_email          { get; set; }
        public bool?    bi_envio_sms            { get; set; }
        public int?     nu_id_regla_izipay      { get; set; }
        public int nu_id_comercio { get; set; }

    }
}
