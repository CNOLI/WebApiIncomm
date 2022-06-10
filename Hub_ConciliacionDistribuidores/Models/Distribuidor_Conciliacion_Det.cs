using System;
using System.Collections.Generic;
using System.Text;

namespace Hub_ConciliacionDistribuidores
{
    public class Distribuidor_Conciliacion_Det : Models.EntidadBase
    {        
        public decimal? nu_id_conciliacion { get; set; }
        public string vc_cod_comercio { get; set; }
        public decimal? nu_id_trx { get; set; }
        public string vc_id_ref_trx_distribuidor { get; set; }
        public string dt_fecha { get; set; }
        public string ti_hora { get; set; }
        public decimal? nu_id_producto { get; set; }
        public decimal? nu_precio { get; set; }
        public string vc_numero_servicio { get; set; }
        public string vc_nro_doc_pago { get; set; }
    }
}
