using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub_Encrypt
{
    public class EncrypDecrypt
    {
        string cad_des = "O7ITQlh8yzpVSCso6FNti2ndRKewkuq9ñxAm50YEPWHÑcZgbLM3aBD1JvfGXrh";
        string cad_en  = "M0gOGVP8yai6lSwvñ5bWcYZ73FEIfXBrxs9NKozen2UupAmh1CdRQkJLqÑHtTh";


        public string ENCRYPT(string fecha_envio, string codigo_distribuidor, string codigo_comercio, string id_producto)
        {


            var encryp = fecha_envio + codigo_distribuidor.PadRight(3) + codigo_comercio.PadRight(3) + id_producto;

            string ordenado = "";
            int longitud = encryp.Length;
            int longituddes = cad_des.Length;
            for (int i = 0; i < longitud; i++)
            {
                var a = encryp[i];
                for (int j = 0; j < longituddes; j++)
                {
                    if (a == cad_des[j])
                    {
                        ordenado += cad_en[j];
                        break;
                    }
                }
            }
            return ordenado;
        }

        public string DECRYPT(string encryp)
        {
            string ordenado = "";
            int longitud = encryp.Length;
            int longitudden = cad_en.Length;
            for (int i = 0; i < longitud; i++)
            {
                var a = encryp[i];
                for (int j = 0; j < longitudden; j++)
                {
                    if (a == cad_en[j])
                    {
                        ordenado += cad_des[j];
                        break;
                    }
                }
            }
            return ordenado;
        }
    }
}
