using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models.IzipayFinazas
{
    public class Token : IzipayModel.InfoBase
    {
        public string bin_acq { get; set; }
        public string mac { get; set; }

    }

    public class TokenResult : IzipayModel.ResponseBase
    {
        public string token { get; set; }
    }
}
