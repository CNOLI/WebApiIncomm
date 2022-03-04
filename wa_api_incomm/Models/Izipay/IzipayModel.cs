using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class IzipayModel : EntidadBase
    {
        public decimal? nu_id_trx_app { get; set; }
        public string vc_id_ref_trx { get; set; }
        public string vc_numero_servicio { get; set; }
        public decimal? nu_id_tipo_moneda_vta { get; set; }
        public decimal? nu_precio_vta { get; set; }
        public string vc_cod_autorizacion { get; set; }
        public decimal? nu_saldo_izipay { get; set; }

        public class InfoBase
        {
            public string secuencia { get; set; }
            public string fecha_hora { get; set; }
            public string id_estab { get; set; }
            public string id_term { get; set; }
            public string secreto { get; set; }
        }

        public class ResponseBase
        {
            public string descripcion { get; set; }
            public string rc { get; set; }
        }
    }

}
