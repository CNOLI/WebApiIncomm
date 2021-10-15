using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class InputTransModel
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string codigo_distribuidor { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string codigo_comercio { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string nombre_comercio { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El {0} es obligatorio")]
        //[RegularExpression("(^[0-9]+$)", ErrorMessage = "El {0} solo permite 9 digitos")]
        //[StringLength(9,MinimumLength =9,ErrorMessage = "El {0} solo permite 9 digitos")]
        public string nro_telefono { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        //[EmailAddress(ErrorMessage = "El campo {0} no es una dirección de correo electrónico válida.")]
        public string email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El {0} es obligatorio")]
        //[RegularExpression("(^[0-9]+$)", ErrorMessage = "El {0} solo permite digitos")]
        public string id_producto { get; set; }
        //public string id_opcion_telefono { get; set; }

    }
}
