using Hub_Encrypt;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Hub_InformarCliente
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();


            HubTISIApi api = new HubTISIApi(config.GetSection("HubTISIInfo:apiURL").Value, config.GetSection("HubTISIInfo:userName").Value, config.GetSection("HubTISIInfo:password").Value);

            //Obtener transacciones por informar


            //Informar Transaccion HUB
            Hub_Informar hub_informar = new Hub_Informar();

            //hub_informar.codigo_distribuidor = cb.vc_cod_distribuidor_hub;
            //hub_informar.codigo_comercio = cb.vc_cod_comercio;
            //hub_informar.envio_email = true;
            //hub_informar.envio_sms = true;

            //hub_informar.nro_transaccion = result.nro_transaccion.ToString();
            //hub_informar.fecha_envio = DateTime.Now.ToString("yyyyMMddHHmmssmss");

            EncrypDecrypt enc_confirmar = new EncrypDecrypt();
            var a_confirmar = enc_confirmar.ENCRYPT(hub_informar.fecha_envio, hub_informar.codigo_distribuidor, hub_informar.codigo_comercio, hub_informar.nro_transaccion);
            hub_informar.clave = a_confirmar;

            var result_confirmar = api.InformarTransaccion(hub_informar).Result;

        }


        //public List<Transaccion> sel_transaccion_por_informar(string conexion, Transaccion model)
        //{
        //    List<Transaccion> ls = new List<Transaccion>();
        //    Transaccion m = new Transaccion();

        //    using (var cn = new SqlConnection(conexion))
        //    {
        //        cn.Open();
        //        using (var cmd = new SqlCommand("TISI_TRX.USP_SEL_TRANSACCION_X_INFORMAR", cn))
        //        {
        //            model.nu_tran_ruta = 1;
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            Models.UtilSql.iGet(cmd, model);
        //            SqlDataReader dr = cmd.ExecuteReader();
        //            while(dr.Read())
        //            {

        //            }

        //            return Models.UtilSql.Query(dr, model);
        //        }
        //    }
        //}
    }
}
