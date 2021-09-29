using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class CanalModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_cod_canal { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_desc_canal { get; set; }
    }
}
