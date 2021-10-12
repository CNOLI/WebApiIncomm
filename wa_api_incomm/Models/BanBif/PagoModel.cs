using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class PagoModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? nu_id_trx { get; set; }
        public E_recaudador recaudador { get; set; }
        public E_convenio convenio { get; set; }
        public E_cliente cliente { get; set; }
        public string moneda { get; set; }
        public int cantidadPagos { get; set; }
        public bool agrupacion { get; set; }
        public decimal? montoTotalDeuda { get; set; }
        public decimal? montoTotalSaldo { get; set; }
        public List<E_datos> deudas { get; set; }
        public PagoModel()
        {
            this.recaudador = new E_recaudador();
            this.convenio = new E_convenio();
            this.cliente = new E_cliente();
            this.deudas = new List<E_datos>();
        }
        public class E_recaudador
        {
            public string codigo { get; set; }
        }
        public class E_convenio
        {
            public string codigo { get; set; }
        }
        public class E_cliente
        {
            public string id { get; set; }
        }
        public class E_datos
        {
            public List<E_datosAdicionales> datosAdicionales { get; set; }
            public string fechaVencimiento { get; set; }
            public E_cliente cliente { get; set; }
            public string idConsulta { get; set; }
            public List<E_servicios> servicios { get; set; }
            public decimal? montoSaldoDestino { get; set; }
            public decimal? montoDescuentoDestino { get; set; }
            public decimal? montoMultaDestino { get; set; }
            public decimal? montoVencidoDestino { get; set; }
            public decimal? montoInteresDestino { get; set; }
            public decimal? montoReajusteDestino { get; set; }
            public decimal? montoTotalDestino { get; set; }

            public E_documento documento { get; set; }
            public List<E_pagos> pagos { get; set; }
            public E_datos()
            {
                this.datosAdicionales = new List<E_datosAdicionales>();
                this.cliente = new E_cliente();
                this.servicios = new List<E_servicios>();
                this.documento = new E_documento();
                this.pagos = new List<E_pagos>();
            }
        }
        public class E_datosAdicionales
        {
            public string nombre { get; set; }
            public string valor { get; set; }
        }
        public class E_servicios
        {
            public string id { get; set; }
        }
        public class E_documento
        {
            public string numero { get; set; }
        }
        public class E_pagos
        {
            public string tipoOperacion { get; set; }
            public string medioPago { get; set; }
            //public E_documento cuentaCargo { get; set; }
            public decimal? monto { get; set; }
            public decimal? deudaAPagar { get; set; }
            //public E_pagos()
            //{
            //    this.cuentaCargo = new E_documento();
            //}
        }
        public class Pago_Input
        {
            public string vc_cod_convenio { get; set; }
            public string nu_id_servicio { get; set; }
            public string vc_nro_documento { get; set; }

        }
    }
}
