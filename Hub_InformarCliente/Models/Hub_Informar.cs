using System;
using System.Collections.Generic;
using System.Text;

namespace Hub_InformarCliente
{
    public class Hub_Informar : Models.EntidadBase
    {
        public string fecha_envio { get; set; }
        public string codigo_distribuidor { get; set; }
        public string codigo_comercio { get; set; }
        public string nro_transaccion { get; set; }
        public bool envio_sms { get; set; }
        public bool envio_email { get; set; }
        public string clave { get; set; }
    }
}
