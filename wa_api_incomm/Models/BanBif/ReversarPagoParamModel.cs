using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.BanBif
{
    public class ReversarPagoParamModel
    {
        public string codigoConvenio { get; set; }
        public string codigoMoneda { get; set; }
        public int cantidadPagos { get; set; }
        public bool agrupacion { get; set; }
        public decimal? montoTotalDeuda { get; set; }
        public decimal? montoTotalSaldo { get; set; }

    }
}
