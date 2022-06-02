using Hub_Encrypt;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;

namespace Hub_InformarCliente
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            string URL = config.GetSection("HubTISIInfo:apiURL").Value;
            string userName = config.GetSection("HubTISIInfo:userName").Value;
            string password = config.GetSection("HubTISIInfo:password").Value;

            string conexion = config.GetSection("SQL").Value;


            HubTISIApi api = new HubTISIApi(URL, userName, password);

            //Obtener transacciones por informar
            Transaccion trx_m = new Transaccion();
            List<Transaccion> ls_trx = new List<Transaccion>();
            ls_trx = sel_transaccion_por_informar(config.GetSection("SQL").Value,trx_m);

            //Informar Transaccion HUB
            foreach (var item_trx in ls_trx)
            {
                Hub_Informar hub_informar = new Hub_Informar();

                hub_informar.codigo_distribuidor = item_trx.codigo_distribuidor;
                hub_informar.codigo_comercio = item_trx.codigo_comercio;
                hub_informar.envio_email = item_trx.envio_email;
                hub_informar.envio_sms = item_trx.envio_sms;

                hub_informar.nro_transaccion = item_trx.nro_transaccion;
                hub_informar.fecha_envio = DateTime.Now.ToString("yyyyMMddHHmmssmss");

                EncrypDecrypt enc_confirmar = new EncrypDecrypt();
                var a_confirmar = enc_confirmar.ENCRYPT(hub_informar.fecha_envio, hub_informar.codigo_distribuidor, hub_informar.codigo_comercio, hub_informar.nro_transaccion);
                hub_informar.clave = a_confirmar;

                var result_confirmar = api.InformarTransaccion(hub_informar).Result;

            }


        }

        public static List<Transaccion> sel_transaccion_por_informar(string conexion, Transaccion model)
        {
            List<Transaccion> ls = new List<Transaccion>();
            Transaccion m = new Transaccion();

            using (var cn = new SqlConnection(conexion))
            {
                cn.Open();
                using (var cmd = new SqlCommand("TISI_TRX.USP_SEL_TRANSACCION_X_INFORMAR", cn))
                {
                    model.nu_tran_ruta = 1;
                    cmd.CommandType = CommandType.StoredProcedure;
                    Models.UtilSql.iGet(cmd, model);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        m = new Transaccion();

                        if (Convertidor_oR.Ec(dr, "VC_COD_DISTRIBUIDOR"))
                            m.codigo_distribuidor = dr["VC_COD_DISTRIBUIDOR"].ToString();
                        if (Convertidor_oR.Ec(dr, "VC_COD_COMERCIO"))
                            m.codigo_comercio = dr["VC_COD_COMERCIO"].ToString();
                        if (Convertidor_oR.Ec(dr, "NU_ID_TRX"))
                            m.nro_transaccion = dr["NU_ID_TRX"].ToString();
                        if (Convertidor_oR.Ec(dr, "BI_ENVIO_EMAIL"))
                            m.envio_email = dr["BI_ENVIO_EMAIL"].ToBool();
                        if (Convertidor_oR.Ec(dr, "BI_ENVIO_SMS"))
                            m.envio_sms = dr["BI_ENVIO_SMS"].ToBool();

                        ls.Add(m);
                    }

                    return ls;
                }
            }
        }
    }
}
