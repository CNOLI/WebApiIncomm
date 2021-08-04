using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.Hub
{
    public class ReenvioMensajeModel : EntidadBase
    {
        public string nu_id_trx { get; set; }
        public string ch_tipo_mensaje { get; set; }
    }
}
