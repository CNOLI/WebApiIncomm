using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static wa_api_incomm.Models.UtilSql;

namespace wa_api_incomm.Models
{
    public static class Utilitarios
    {
        public static object JsonErrorSel(Exception ex)
        {
            responseClass ec = new responseClass();
            ec.nu_tran_stdo = 0;
            ec.nu_tran_pkey = 0;
            ec.vc_tran_codi = "";
            ec.tx_tran_mnsg = ex.Message;
            return ec;

        }
        public static object JsonErrorTran(Exception ex)
        {
            String jsonObject = "{";
            jsonObject += "\"nu_tran_stdo\":\"0\",";
            jsonObject += "\"nu_tran_pkey\":\"0\",";
            jsonObject += "\"vc_tran_codi\":\"" + ex.GetType() + "\",";
            jsonObject += "\"tx_tran_mnsg\":\"" + ex.Message + "\",";
            jsonObject = jsonObject.Substring(0, jsonObject.Length - 1) + "}";

            return JsonConvert.DeserializeObject<object>(jsonObject);
        }

    }
}
