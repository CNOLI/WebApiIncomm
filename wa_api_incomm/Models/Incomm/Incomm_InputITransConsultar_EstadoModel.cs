using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Incomm_InputITransConsultar_EstadoModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string codigo_distribuidor { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string codigo_comercio { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El {0} es obligatorio")]
        public string nro_transaccion { get; set; }

    }
}
