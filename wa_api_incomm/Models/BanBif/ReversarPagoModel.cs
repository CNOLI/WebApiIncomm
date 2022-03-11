using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class ReversarPagoModel
    {
        public string numeroPago { get; set; }
        public string tipoOperacion { get; set; }
        public string medioPago { get; set; }
        public decimal? monto { get; set; }
        public List<E_datos> deudas { get; set; }
        
        public ReversarPagoModel()
        {
            this.deudas = new List<E_datos>();
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
            public decimal? montoSaldoOrigen { get; set; }
            public decimal? montoDescuentoOrigen { get; set; }
            public decimal? montoMultaOrigen { get; set; }
            public decimal? montoVencidoOrigen { get; set; }
            public decimal? montoInteresOrigen { get; set; }
            public decimal? montoReajusteOrigen { get; set; }
            public decimal? montoTotalOrigen { get; set; }

            public E_documento documento { get; set; }
            public List<E_servicios> servicios { get; set; }
            public E_datos()
            {
                this.datosAdicionales = new List<E_datosAdicionales>();
                this.cliente = new E_cliente();
                this.servicios = new List<E_servicios>();
                this.documento = new E_documento();
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
    }
}
