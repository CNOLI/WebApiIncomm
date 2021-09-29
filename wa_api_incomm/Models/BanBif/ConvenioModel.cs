using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class ConvenioModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_cod_convenio { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_desc_convenio { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public EmpresaModel e_empresa { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ProductoModel e_producto { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<CanalModel> ls_canal { get; set; }
        public ConvenioModel()
        {
            this.e_empresa = new EmpresaModel();
            this.e_producto = new ProductoModel();
            this.ls_canal = new List<CanalModel>();
        }
    }
}
