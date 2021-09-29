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
    }
}
