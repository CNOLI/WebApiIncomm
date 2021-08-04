using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class ComercioModel :EntidadBase
    {
        public int      nu_id_distribuidor  { get; set; }
        public int      nu_id_comercio      { get; set; }
        public string   vc_cod_comercio     { get; set; }
        public string   vc_nombre_comercio  { get; set; }
    }
}
