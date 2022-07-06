using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class TransaccionModel : EntidadBase
    {
        public class Transaccion_Input_Confirmar
        {
            public string fecha_envio { get; set; }
            public string codigo_distribuidor { get; set; }
            public string codigo_comercio { get; set; }
            public string nro_transaccion { get; set; }
            public bool envio_sms { get; set; }
            public bool envio_email { get; set; }
            public string clave { get; set; }
        }

        public decimal? nu_id_convenio { get; set; }
        public Int64? nu_id_trx_hub { get; set; }

        public Int64? nu_id_trx { get; set; }
        public decimal? nu_id_trx_app { get; set; }
        public int nu_id_distribuidor { get; set; }
        public int nu_id_comercio { get; set; }
        public DateTime? dt_fecha { get; set; }
        public int? nu_id_producto { get; set; }
        public decimal nu_precio { get; set; }
        public string vc_id_ref_trx { get; set; }
        public string vc_cod_autorizacion { get; set; }
        public string vc_nro_pin { get; set; }
        public string vc_nro_pin_desencriptado { get; set; }
        public string vc_cod_error { get; set; }
        public string vc_desc_error { get; set; }
        public string vc_desc_tipo_error { get; set; }

        public string vc_cod_distribuidor { get; set; }
        public string vc_cod_comercio { get; set; }
        public string vc_desc_producto { get; set; }

        //Sentinel
        public int? nu_id_tipo_doc_sol { get; set; }
        public string vc_cod_tipo_doc_sol { get; set; }
        public string vc_nro_doc_sol { get; set; }
        public string ch_dig_ver_sol { get; set; }
        public string vc_email_sol { get; set; }
        public string vc_telefono_sol { get; set; }
        public int? nu_id_tipo_comprobante { get; set; }
        public string vc_tipo_comprobante { get; set; }
        public int? nu_id_tipo_doc_cpt { get; set; }
        public string vc_cod_tipo_doc_cpt { get; set; }
        public string vc_nro_doc_cpt { get; set; }
        public string vc_cod_producto { get; set; }
        public string vc_ruc { get; set; }
        public string PDVTipoDoc { get; set; }
        public string PDVNroDoc { get; set; }
        public string PDVRazSocNom { get; set; }

        //
        public string vc_numero_servicio { get; set; }
        public string vc_nro_doc_pago { get; set; }
        //
        public decimal? nu_id_tipo_moneda_vta { get; set; }
        public decimal? nu_saldo_izipay { get; set; }

        public decimal? nu_id_trx_extorno { get; set; }


        //Reportes
        public string vc_desc_distribuidor { get; set; }
        public string vc_nombre_comercio { get; set; }
        public decimal? nu_valor_comision { get; set; }
        public decimal? nu_precio_vta { get; set; }
        public decimal? nu_imp_trx { get; set; }
        public string vc_tipo_doc_sol { get; set; }
        public string vc_tipo_doc_cpt { get; set; }
        public string vc_fecha_reg { get; set; }

        public Int64? nu_id_trx_ref { get; set; }
        public bool bi_confirmado { get; set; }
        public bool bi_informado { get; set; }
        public bool bi_extorno { get; set; }
        public string vc_observacion { get; set; }

        public string vc_id_ref_trx_distribuidor { get; set; }

        public TimeSpan ti_respuesta_api { get; set; }

        public string vc_datos_adicionales { get; set; }

        public string vc_desc_categoria { get; set; }


        public decimal? nu_id_estado { get; set; }
        public string vc_error { get; set; }
        public string vc_ubigeo { get; set; }

    }
}
