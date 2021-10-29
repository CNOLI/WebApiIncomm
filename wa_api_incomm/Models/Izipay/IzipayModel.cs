using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class IzipayModel : EntidadBase
    {
        public decimal? nu_id_trx_app { get; set; }
        public string vc_id_ref_trx { get; set; }
        public string vc_numero_servicio { get; set; }
        public decimal? nu_id_tipo_moneda_vta { get; set; }
        public decimal? nu_precio_vta { get; set; }
        public string vc_cod_autorizacion { get; set; }
        public decimal? nu_saldo_izipay { get; set; }
        public class IzipayInfo
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class GenerarReporte
        {

            [Required(ErrorMessage = "El campo {0} es obligatorio.")]
            public string codigo_distribuidor { get; set; }
            [Required(ErrorMessage = "El campo {0} es obligatorio.")]
            public string codigo_comercio { get; set; }
            [Required(ErrorMessage = "El campo {0} es obligatorio.")]
            public string nombre_comercio { get; set; }
            [Required(AllowEmptyStrings = false, ErrorMessage = "El {0} es obligatorio")]
            public string id_producto { get; set; }
            [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es obligatorio.")]
            public string tipo_documento_consultante { get; set; }
            [Required(ErrorMessage = "El campo {0} es obligatorio.")]
            public string numero_documento_consultante { get; set; }
            [Required(ErrorMessage = "El campo {0} es obligatorio.")]
            public string email_consultante { get; set; }
            [Required(ErrorMessage = "El campo {0} es obligatorio.")]
            public string digito_verificador_consultante { get; set; }
            [Required(AllowEmptyStrings = false, ErrorMessage = "El campo {0} es obligatorio.")]
            public string tipo_documento_consultado { get; set; }
            [Required(ErrorMessage = "El campo {0} es obligatorio.")]
            public string numero_documento_consultado { get; set; }

        }

        public class ConsultarReporte
        {
            public string nro_transaccion { get; set; }

        }

        public class InfoBase
        {
            public string secuencia { get; set; }
            public string fecha_hora { get; set; }
            public string id_estab { get; set; }
            public string id_term { get; set; }
            public string secreto { get; set; }
        }

        public class ResponseBase
        {
            public string descripcion { get; set; }
            public string rc { get; set; }
        }
    }

}
