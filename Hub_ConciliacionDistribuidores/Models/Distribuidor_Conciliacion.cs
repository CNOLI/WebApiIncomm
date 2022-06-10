using System;
using System.Collections.Generic;
using System.Text;

namespace Hub_ConciliacionDistribuidores
{
    public class Distribuidor_Conciliacion : Models.EntidadBase
    {
        public string vc_cod_distribuidor { get; set; }
        public string vc_nom_archivo { get; set; }
        public string vc_archivo { get; set; }
        public string dt_fecha { get; set; }
        public decimal? nu_total_trx { get; set; }
        public decimal? nu_imp_trx { get; set; }
    }
}
