using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Incomm_InputITransConfirmarModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string codigo_distribuidor { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string codigo_comercio { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El {0} es obligatorio")]
        public string nro_transaccion { get; set; }
        public bool envio_sms { get; set; }
        public bool envio_email { get; set; }
        public string clave { get; set; }
        public string fecha_envio { get; set; }

    }
}
