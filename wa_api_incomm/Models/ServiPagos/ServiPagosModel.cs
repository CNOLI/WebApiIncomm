using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.ServiPagos
{
    public class ServiPagosModel :EntidadBase
    {
        public decimal? nu_id_trx_app { get; set; }
        public decimal? nu_id_producto_app { get; set; }
        public string vc_numero_servicio { get; set; }
        public decimal? nu_id_tipo_moneda_vta { get; set; }
        public decimal? nu_precio_vta { get; set; }
        public string vc_id_ref_trx { get; set; }
        public string vc_cod_autorizacion { get; set; }
        public string vc_id_ref_trx_distribuidor { get; set; }

    }
}

