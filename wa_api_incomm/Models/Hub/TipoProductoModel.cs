using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class TipoProductoModel: EntidadBase
    {
        public int? nu_id_tipo_producto      { get; set; }
	    public string vc_cod_tipo_producto   { get; set; }
        public string vc_desc_tipo_producto  { get; set; }
        public int? nu_id_convenio           { get; set; }
        public bool bi_estado                { get; set; }
    }
}
