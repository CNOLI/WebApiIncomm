using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class EmpresaModel : EntidadBase
    {
        public string vc_cod_rubro { get; set; }
        public string vc_desc_rubro { get; set; }
        public string vc_cod_empresa { get; set; }
        public string vc_nro_doc_identidad { get; set; }
        public string vc_nombre { get; set; }
        public string vc_razon_social { get; set; }
        public decimal? nu_id_convenio { get; set; }
        public bool? bi_estado { get; set; }
        public class Empresa_Input
        {
            public string vc_cod_empresa { get; set; }
        }
    }
}
