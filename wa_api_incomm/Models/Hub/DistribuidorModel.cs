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
        public string   vc_zip_code         { get; set; }
        public decimal? nu_saldo            { get; set; }

    }
}
