using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class TransaccionModel :EntidadBase
    {
        public decimal? nu_id_convenio { get; set; }

        public int nu_id_trx                { get; set; }
        public decimal? nu_id_trx_app       { get; set; }
        public int nu_id_distribuidor       { get; set; }
        public int nu_id_comercio           { get; set; }
        public DateTime dt_fecha            { get; set; }
        public int nu_id_producto           { get; set; }
        public decimal nu_precio            { get; set; }
        public string vc_id_ref_trx         { get; set; }
        public string vc_cod_autorizacion   { get; set; }
        public string vc_nro_pin            { get; set; }
        public string vc_cod_error          { get; set; }
        public string vc_desc_error         { get; set; }
        public string vc_desc_tipo_error    { get; set; }

        public string vc_cod_distribuidor   { get; set; }
        public string vc_cod_comercio       { get; set; }
        public string vc_desc_producto      { get; set; }

        //Sentinel
        public string vc_cod_tipo_doc_sol   { get; set; }
        public string vc_nro_doc_sol        { get; set; }
        public string ch_dig_ver_sol        { get; set; }
        public string vc_email_sol          { get; set; }
        public string vc_telefono_sol       { get; set; }
        public int nu_id_tipo_comprobante   { get; set; }
        public string vc_tipo_comprobante   { get; set; }
        public string vc_cod_tipo_doc_cpt   { get; set; }
        public string vc_nro_doc_cpt        { get; set; }
        public string vc_cod_producto       { get; set; }
        public string vc_ruc                { get; set; }


    }
}
