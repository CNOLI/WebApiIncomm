using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class RubroModel : EntidadBase
    {
        public decimal? nu_id_rubro { get; set; }
        public string vc_cod_rubro { get; set; }
        public string vc_desc_rubro { get; set; }
        public decimal? nu_id_convenio { get; set; }
        public bool? bi_estado { get; set; }
        public class Rubro_Input
        {
            public string vc_cod_rubro { get; set; }
        }
    }
}
