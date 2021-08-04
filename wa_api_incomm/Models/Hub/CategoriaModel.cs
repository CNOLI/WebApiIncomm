using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class CategoriaModel : EntidadBase
    {
        public int      nu_id_categoria     { get; set; }
        public  string  vc_cod_categoria    { get; set; }
        public  string  vc_desc_categoria   { get; set; }
        public  string  vc_url_imagen       { get; set; }
        public  int?    nu_id_convenio      { get; set; }
        public  bool    bi_estado           { get; set; }

    }
}
