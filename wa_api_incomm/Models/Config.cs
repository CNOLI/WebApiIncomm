using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public class Config
    {
        public const bool bi_produccion = false;


        public const string vc_url_sentinel_qa = "https://www2.sentinelperu.com/wsrest/";
        public const string vc_url_sentinel_prod = "https://www2.sentinelperu.com/wsrest/";

        public const string vc_url_incomm_qa = "http://192.168.29.20:2555/";
        public const string vc_url_incomm_prod = "http://192.168.31.19:20966/";

        public const string vc_url_equifax_qa = "https://accepwse.equifax.com.pe/salespartner/";
        public const string vc_url_equifax_prod = "https://ws.equifax.com.pe/salespartner/";

        public const string vc_url_izipay_qa = "https://172.26.158.236:8443/";
        public const string vc_url_izipay_prod = "https://172.26.158.240:8443/";

        public const string vc_url_izipay_finanzas_qa = "https://psrdes.izipay.pe:8088/";
        public const string vc_url_izipay_finanzas_prod = "https://psr.izipay.pe:8090/";

        public const string vc_url_banbif_qa = "https://api-recaudaciones.uatapps.banbifapimarket.com.pe/";
        public const string vc_url_banbif_token_qa = "https://rh-sso-rhsso.uatapps.banbifapimarket.com.pe/auth/realms/Banbif-API-External/protocol/openid-connect/token";
        public const string vc_url_banbif_prod = "";
        public const string vc_url_banbif_token_prod = "";


        //URL's

        public const string vc_url_sentinel = bi_produccion ? vc_url_sentinel_prod : vc_url_sentinel_qa;
        public const string vc_url_incomm = bi_produccion ? vc_url_incomm_prod : vc_url_incomm_qa;
        public const string vc_url_equifax = bi_produccion ? vc_url_equifax_prod : vc_url_equifax_qa;
        public const string vc_url_izipay = bi_produccion ? vc_url_izipay_prod : vc_url_izipay_qa;
        public const string vc_url_izipay_finanzas = bi_produccion ? vc_url_izipay_finanzas_prod : vc_url_izipay_finanzas_qa;
        public const string vc_url_banbif = bi_produccion ? vc_url_banbif_prod : vc_url_banbif_qa;
        public const string vc_url_banbif_token = bi_produccion ? vc_url_banbif_token_prod : vc_url_banbif_token_qa;
    }
}
