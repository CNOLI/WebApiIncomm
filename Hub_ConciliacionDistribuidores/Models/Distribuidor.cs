using System;
using System.Collections.Generic;
using System.Text;

namespace Hub_ConciliacionDistribuidores
{
    public class Distribuidor : Models.EntidadBase
    {        
        public decimal? nu_id_distribuidor { get; set; }
        public string vc_cod_distribuidor { get; set; }
        public string vc_desc_distribuidor { get; set; }
        public bool bi_obtener_archivo { get; set; }
        public decimal? nu_id_metodo { get; set; }
        public string vc_ip_archivo { get; set; }
        public int? nu_puerto_archivo { get; set; }
        public string vc_usuario_archivo { get; set; }
        public string vc_contrasena_archivo { get; set; }
        public string vc_ruta_archivo { get; set; }
        public bool bi_resultado_email { get; set; }
        public string vc_email { get; set; }
    }
}
