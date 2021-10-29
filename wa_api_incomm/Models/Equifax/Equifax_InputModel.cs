using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Equifax_InputModel
    {

        public class Generar_Reporte_Input
        {
            public int documentType { get; set; }
            public string documentNumber { get; set; }
            public string email { get; set; }
            public string verifierDigit { get; set; }
            public int documentTypeThird { get; set; }
            public string documentNumberThird { get; set; }
        }

        public class Consultar_Reporte_Input
        {
            public string operationNumber { get; set; }
        }

        public class Busqueda_Equifax_Input
        {
            public string codigo_distribuidor { get; set; }
            public string buscador { get; set; }

        }

    }
}

