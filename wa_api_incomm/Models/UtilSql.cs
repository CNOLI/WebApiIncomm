using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace wa_api_incomm.Models
{
    public static class UtilSql
    {
        public static SqlCommand Parameters(SqlCommand cmd, object o)
        {
            cmd.CommandType = CommandType.StoredProcedure;
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                cmd.Parameters.AddWithValue("@" + p.Name, p.GetValue(o));
            }
            return cmd;
        }
        public static object Get(SqlDataReader or, object type)
        {
            var m = new object();
            if (or.Read())
            {
                m = Decode(or, type);
            }
            return m;
        }
        public static List<object> Query(SqlDataReader or, object type)
        {
            var lm = new List<object>();
            while (or.Read())
            {
                lm.Add(Decode(or, type));
            }
            return lm;
        }
        public static object Decode(SqlDataReader or, object o)
        {
            String jsonObject = "{";
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (Ec(or, p.Name))
                    jsonObject += "\"" + p.Name + "\":\"" + or[p.Name].ToText() + "\",";
            }
            jsonObject = jsonObject.Substring(0, jsonObject.Length - 1) + "}";

            return JsonConvert.DeserializeObject<object>(jsonObject);
        }




        public static List<object> QueryDsTable(DataTable dt, object type)
        {
            var lm = new List<object>();
            foreach (DataRow pRow in dt.Rows)
            {
                lm.Add(DecodeRowDt(pRow, type));
            }
            return lm;
        }
        public static object DecodeRowDt(DataRow row, object o)
        {
            String jsonObject = "{";
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (row.Table.Columns.IndexOf(p.Name) >= 0)
                    jsonObject += "\"" + p.Name + "\":\"" + row[p.Name].ToText() + "\",";
            }
            jsonObject = jsonObject.Substring(0, jsonObject.Length - 1) + "}";
            return JsonConvert.DeserializeObject<object>(jsonObject);
        }

        public static String ToText(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToString(s).Trim();
            return null;
        }
        public static String ToTextUpper(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToString(s).Trim().ToUpper();
            return null;
        }
        public static int? ToInt(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToInt32(s);
            return null;
        }
        public static Int16? ToInt16(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToInt16(s);
            return null;
        }
        public static Int32? ToInt32(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToInt32(s);
            return null;
        }
        public static Int64? ToInt64(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToInt32(s);
            return null;
        }
        public static Decimal? ToDecimal(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToDecimal(s);
            return null;
        }
        public static Boolean? ToBoolean(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToBoolean(s);
            return null;
        }
        public static bool ToBool(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToBoolean(s);
            return false;
        }
        public static byte? ToBit(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToByte(s);
            return null;
        }
        public static byte[] ToArrayBit(this object s)
        {
            if (s != DBNull.Value)
                return ObjectToByteArray(s);
            return null;
        }
        public static DateTime? ToDateTime(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToDateTime(s);
            return null;
        }
        public static byte? ToByte(this object s)
        {
            if (s != DBNull.Value)
                return Convert.ToByte(s);
            return null;
        }
        private static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            MemoryStream ms = new MemoryStream((byte[])obj);
            return ms.ToArray();
        }
        public static Decimal? ToDecimal2(this object s)
        {
            if (s != DBNull.Value)
            {
                string val = Convert.ToString(s);
                decimal newDecimal;
                if (decimal.TryParse(val, out newDecimal))
                {
                    return newDecimal;
                }
            }
            return null;
        }
        public static bool Ec(IDataReader or, string columna)
        {
            or.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + columna + "'";
            return (or.GetSchemaTable().DefaultView.Count > 0);
        }

        public static SqlCommand iGet(SqlCommand cmd, EntidadBase model)
        {
            if (model.nu_tran_ruta == null) model.nu_tran_ruta = 0;
            if (model.vc_tran_clve_find == null) model.vc_tran_clve_find = "";
            if (model.vc_tran_usua_ptcn == null) model.vc_tran_usua_ptcn = "";
            if (model.nu_tran_stdo == null) model.nu_tran_stdo = 0;
            if (model.tx_tran_mnsg == null) model.tx_tran_mnsg = "";

            cmd.Parameters.AddWithValue("@nu_tran_ruta", model.nu_tran_ruta);
            cmd.Parameters.AddWithValue("@vc_tran_clve_find", model.vc_tran_clve_find);
            cmd.Parameters.AddWithValue("@vc_tran_usua_ptcn", model.vc_tran_usua_ptcn);
            cmd.Parameters.AddWithValue("@nu_tran_regs_pagn", model.nu_tran_regs_pagn);
            cmd.Parameters.AddWithValue("@nu_tran_pagn", model.nu_tran_pagn);

            cmd.Parameters.Add(new SqlParameter("@nu_tran_stdo", SqlDbType.Int, 1, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));
            cmd.Parameters.Add(new SqlParameter("@tx_tran_mnsg", SqlDbType.VarChar, 5000, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));

            return cmd;
        }

        public static SqlCommand iIns(SqlCommand cmd, EntidadBase model)
        {
            if (model.ch_tran_stdo_regi == null) model.ch_tran_stdo_regi = "";
            if (model.nu_tran_ruta == null) model.nu_tran_ruta = 0;
            cmd.Parameters.AddWithValue("@ch_tran_stdo_regi", model.ch_tran_stdo_regi);
            cmd.Parameters.AddWithValue("@nu_tran_ruta", model.nu_tran_ruta);
            cmd.Parameters.AddWithValue("@vc_tran_usua_regi", model.vc_tran_usua_regi);

            cmd.Parameters.Add(new SqlParameter("@nu_tran_stdo", SqlDbType.Int, 1, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));
            cmd.Parameters.Add(new SqlParameter("@nu_tran_pkey", SqlDbType.Decimal, 20, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));
            cmd.Parameters.Add(new SqlParameter("@vc_tran_codi", SqlDbType.VarChar, 25, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));
            cmd.Parameters.Add(new SqlParameter("@tx_tran_mnsg", SqlDbType.VarChar, 5000, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));

            return cmd;
        }

        public static SqlCommand iUpd(SqlCommand cmd, EntidadBase model)
        {
            if (model.ch_tran_stdo_regi == null) model.ch_tran_stdo_regi = "";
            if (model.nu_tran_ruta == null) model.nu_tran_ruta = 0;
            cmd.Parameters.AddWithValue("@ch_tran_stdo_regi", model.ch_tran_stdo_regi);
            cmd.Parameters.AddWithValue("@nu_tran_ruta", model.nu_tran_ruta);
            cmd.Parameters.AddWithValue("@vc_tran_usua_modi", model.vc_tran_usua_modi);

            cmd.Parameters.Add(new SqlParameter("@nu_tran_stdo", SqlDbType.Int, 1, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));
            cmd.Parameters.Add(new SqlParameter("@nu_tran_pkey", SqlDbType.Decimal, 20, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));
            cmd.Parameters.Add(new SqlParameter("@vc_tran_codi", SqlDbType.VarChar, 25, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));
            cmd.Parameters.Add(new SqlParameter("@tx_tran_mnsg", SqlDbType.VarChar, 5000, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, null));

            return cmd;
        }

        public static object sOutPut(SqlCommand cmd)
        {
            responseClass ec = new responseClass();
            ec.nu_tran_stdo = cmd.Parameters["@nu_tran_stdo"].Value.ToInt();
            ec.nu_tran_pkey = cmd.Parameters["@nu_tran_pkey"].Value.ToInt();
            ec.vc_tran_codi = cmd.Parameters["@vc_tran_codi"].Value.ToText();
            ec.tx_tran_mnsg = cmd.Parameters["@tx_tran_mnsg"].Value.ToText().Replace("\"", "");
            return ec;
        }

        public static object sOutPutCatch(string mensaje)
        {
            responseClass ec = new responseClass();
            ec.nu_tran_stdo = 0;
            ec.nu_tran_pkey = 0;
            ec.vc_tran_codi = "";
            ec.tx_tran_mnsg = mensaje.Replace("\"", "");
            return ec;

        }

        public static object sOutPutSuccess(string mensaje)
        {
            responseClass ec = new responseClass();
            ec.nu_tran_stdo = 1;
            ec.nu_tran_pkey = 0;
            ec.vc_tran_codi = "200";
            ec.tx_tran_mnsg = mensaje.Replace("\"", "");
            return ec;

        }

        public static object sOutPutTransaccion(string cod ,string mensaje)
        {
            resulttran rs = new resulttran();
            rs.codigo = cod;
            rs.mensaje = mensaje;
            return rs;

        }



        public class responseClass
        {
            public int? nu_tran_stdo { get; set; }
            public int? nu_tran_pkey { get; set; }
            public string vc_tran_codi { get; set; }
            public string tx_tran_mnsg { get; set; }

        }

        public class resulttran
        {
            public string codigo { get; set; }
            public string mensaje { get; set; }
        }

       



    }
}
