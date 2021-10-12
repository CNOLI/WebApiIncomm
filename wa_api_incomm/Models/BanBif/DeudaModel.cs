using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class DeudaModel
    {
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public decimal? nu_id_trx { get; set; }

        //public string vc_cod_convenio { get; set; }
        //public int nu_id_servicio { get; set; }
        //public int nu_id_transaccion_origen { get; set; }
        public string vc_numero_documento { get; set; }
        public string vc_cliente { get; set; }
        public string dt_fecha_factura { get; set; }
        public string dt_fecha_vencimiento { get; set; }
        public string vc_moneda { get; set; }
        //public string vc_glosa_campaña { get; set; }
        //public decimal? nu_monto_saldo_origen { get; set; }
        //public decimal? nu_monto_descuento_origen { get; set; }
        //public decimal? nu_monto_multa_origen { get; set; }
        //public decimal? nu_monto_vencido_origen { get; set; }
        //public decimal? nu_monto_interes_origen { get; set; }
        //public decimal? nu_monto_reajuste_origen { get; set; }
        //public decimal? nu_monto_total_origen { get; set; }
        //public decimal? nu_monto_saldo_destino { get; set; }
        //public decimal? nu_monto_descuento_destino { get; set; }
        //public decimal? nu_monto_multa_destino { get; set; }
        //public decimal? nu_monto_vencido_destino { get; set; }
        //public decimal? nu_monto_interes_destino { get; set; }
        //public decimal? nu_monto_reajuste_destino { get; set; }
        //public decimal? nu_monto_total_destino { get; set; }
        public decimal? nu_monto_deuda { get; set; }
        //public decimal? nu_comision_cliente { get; set; }
        //public E_Documento e_documento { get; set; }
        //public decimal? nu_id_consulta { get; set; }
        //public ConvenioModel e_convenio { get; set; }
        //public E_Cliente e_cliente { get; set; }
        //public List<E_Servicio> ls_servicio { get; set; }
        //public List<E_Datos_Adicionales> ls_datos_adicionales { get; set; }
        //public DeudaModel()
        //{
        //    this.e_documento = new E_Documento();
        //    this.e_convenio = new ConvenioModel();
        //    this.e_cliente = new E_Cliente();
        //    this.ls_servicio = new List<E_Servicio>();
        //    this.ls_datos_adicionales = new List<E_Datos_Adicionales>();
        //}

        //public class E_Cliente
        //{
        //    public string id { get; set; }
        //}
        //public class E_Servicio
        //{
        //    public string nu_id_servicio { get; set; }
        //}
        //public class E_Documento
        //{
        //    public string vc_numero { get; set; }
        //}
        //public class E_Datos_Adicionales
        //{
        //    public string vc_nombre { get; set; }
        //    public string vc_valor { get; set; }
        //}
        public class Deuda_Input
        {
            public string vc_cod_convenio { get; set; }
            public string nu_id_servicio { get; set; }
        }
    }
}
