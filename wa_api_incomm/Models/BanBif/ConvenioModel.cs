using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class ConvenioModel : EntidadBase
    {
        public string vc_cod_convenio { get; set; }
        public string vc_desc_convenio { get; set; }
        public string vc_cod_empresa { get; set; }
        public string vc_nro_doc_empresa { get; set; }
        public string vc_nombre_empresa { get; set; }
        public string vc_cod_producto { get; set; }
        public bool? bi_estado { get; set; }
        public class Convenio_Input
        {
            public string vc_cod_convenio { get; set; }
        }
    }
}
