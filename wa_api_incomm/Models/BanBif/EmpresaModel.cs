using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class EmpresaModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_cod_empresa { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_nro_doc_empresa { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_nombre_empresa { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_razon_social_empresa { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_estado_empresa { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<RubroModel> ls_rubro { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<CanalModel> ls_canal { get; set; }
        public EmpresaModel()
        {
            this.ls_rubro = new List<RubroModel>();
            this.ls_canal = new List<CanalModel>();
        }
    }
}
