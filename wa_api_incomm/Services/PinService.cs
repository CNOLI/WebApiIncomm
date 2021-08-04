using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Help;
using wa_api_incomm.Models;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Services
{
    public class PinService: IPinService
    {
        public object get(string key, string pin, string ruta)
        {
            PinModel pm = new PinModel();

            var file = Path.Combine(ruta, "AESGCMHelper-1.0.0.jar");
            EncoderAES encoder = new EncoderAES(file, key);
            pm.pin = encoder.Decrypt(pin);

            return pm;
        }
    }
}
