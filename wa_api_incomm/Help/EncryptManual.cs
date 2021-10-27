using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Help
{
    public class EncryptManual
    {
        string cad_des = "abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ123456789";
        string cad_en = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÑñ";

        public string ENCRYPT(string encryp)
        {
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
