using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Sentinel_ResponseModel
    {
        //Banco
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? id_banco { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string desc_banco { get; set; }

        //Tipo Documento Identidad
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? id_tipo_documento_identidad { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string codigo_tipo_documento_identidad { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string descripcion_tipo_documento_identidad { get; set; }

        //Producto
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? id_producto { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string codigo_producto { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string nombre_producto { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? precio { get; set; }

        //Saldo
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? saldo { get; set; }

        //Transaccion
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? id_transaccion { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string fecha { get; set; }

        //Persona
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string nombres { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string apellido_paterno { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string apellido_materno { get; set; }

        //Respuesta general

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? nu_tran_stdo { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? nu_tran_pkey { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string vc_tran_codi { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string tx_tran_mnsg { get; set; }

        //Respuesta TRX

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string codigo { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string mensaje { get; set; }

    }
}
    
