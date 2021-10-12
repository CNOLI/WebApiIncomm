using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{

    public class Response
    {
        public E_meta meta { get; set; }
        public List<E_datos> datos { get; set; }
        public class E_Response
        {
            public E_meta meta { get; set; }
            public E_datos datos { get; set; }
        }
        public class Ls_Response_Trx
        {
            public E_meta meta { get; set; }
            public List<E_datos_trx> datos { get; set; }
        }
        public class E_Response_Trx
        {
            public E_meta meta { get; set; }
            public E_datos_trx datos { get; set; }
        }
        public class E_meta
        {
            public List<E_mensajes> mensajes { get; set; }
            public int totalRegistros { get; set; }
            public string idTransaccion { get; set; }
        }
        public class E_mensajes
        {
            public string codigo { get; set; }
            public string mensaje { get; set; }
            public string tipo { get; set; }
        }
        public class E_datos
        {
            // Rubros Recaudador
            public string codigo { get; set; }
            public string descripcion { get; set; }

            // Empresas Rubros
            public string documento { get; set; }
            public string nombre { get; set; }
            public string razonSocial { get; set; }
            public List<E_rubros> rubros { get; set; }
            public List<E_canales> canales { get; set; }

            // Listar Convenios Rubros
            public DateTime fechaInicio { get; set; }
            public DateTime fechaFin { get; set; }
            public DateTime fechaCreacion { get; set; }
            public DateTime fechaActualizacion { get; set; }
            public string estado { get; set; }
            public E_recaudador recaudador { get; set; }
            public E_recaudador empresa { get; set; }
            public E_producto producto { get; set; }

            // Consultar convenio especifico
            public List<E_contactos> contactos { get; set; }

        }
        public class E_datos_trx
        {
            // Consultar deudas
            public DateTime fechaVencimiento { get; set; }
            public E_cliente cliente { get; set; }
            public string glosaCampana { get; set; }
            public decimal? montoSaldoOrigen { get; set; }
            public decimal? montoDescuentoOrigen { get; set; }
            public decimal? montoMultaOrigen { get; set; }
            public decimal? montoVencidoOrigen { get; set; }
            public decimal? montoInteresOrigen { get; set; }
            public decimal? montoReajusteOrigen { get; set; }
            public decimal? montoTotalOrigen { get; set; }
            public decimal? montoSaldoDestino { get; set; }
            public decimal? montoDescuentoDestino { get; set; }
            public decimal? montoMultaDestino { get; set; }
            public decimal? montoVencidoDestino { get; set; }
            public decimal? montoInteresDestino { get; set; }
            public decimal? montoReajusteDestino { get; set; }
            public decimal? montoTotalDestino { get; set; }
            public decimal? montoRedondeo { get; set; }
            public decimal? comisionCliente { get; set; }
            //public E_documento documento { get; set; }
            public decimal? idConsulta { get; set; }
            public E_documento documento { get; set; }
            public E_convenio convenio { get; set; }
            public List<E_servicios> servicios { get; set; }
            public string moneda { get; set; }
            public DateTime fechaFactura { get; set; }
            public List<E_datosAdicionales> datosAdicionales { get; set; }

            // Procesar Pagos
            public int fechaHora { get; set; }
            public List<E_pagos> pagos { get; set; }
            public List<E_datos> deudas { get; set; }
        }
        public class E_rubros
        {
            public string codigo { get; set; }
            public string descripcion { get; set; }
        }
        public class E_canales
        {
            public string codigo { get; set; }
            public string descripcion { get; set; }
        }
        public class E_recaudador
        {
            public string codigo { get; set; }
            public string documento { get; set; }
            public string nombre { get; set; }
            public string estado { get; set; }
        }
        public class E_producto
        {
            public string codigo { get; set; }
        }
        public class E_contactos
        {
            public string documento { get; set; }
            public string nombre { get; set; }
            public string apellidoPaterno { get; set; }
            public string apellidoMaterno { get; set; }
            public string telefonoFijo { get; set; }
            public string celular { get; set; }
            public string email { get; set; }
            public string dependencia { get; set; }
        }

        public class E_cliente
        {
            public string id { get; set; }
        }
        public class E_documento
        {
            public string numero { get; set; }
        }
        public class E_convenio
        {
            public string codigo { get; set; }
        }
        public class E_servicios
        {
            public string id { get; set; }
        }
        public class E_datosAdicionales
        {
            public string nombre { get; set; }
            public string valor { get; set; }
        }
        public class E_pagos
        {
            public decimal? monto { get; set; }
            public E_documento cuentaCargo { get; set; }
            public string tipoOperacion { get; set; }
            public List<E_datosAdicionales> datosAdicionales { get; set; }
            public string comprobante { get; set; }
            public string numeroPago { get; set; }

        }
    }
}
