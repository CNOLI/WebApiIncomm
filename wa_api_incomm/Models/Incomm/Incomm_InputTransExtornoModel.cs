using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Incomm_InputTransExtornoModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string codigo_distribuidor { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string codigo_comercio { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string nombre_comercio { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El {0} es obligatorio")]
        public string nro_telefono { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El {0} es obligatorio")]
        public string id_producto { get; set; }
        public string clave { get; set; }
        public string fecha_envio { get; set; }
        public string nro_transaccion_referencia { get; set; }

    }
}
