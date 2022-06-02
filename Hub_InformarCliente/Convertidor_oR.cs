using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Hub_InformarCliente
{

    public static class Convertidor_oR
    {
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

        public static Decimal ToDecimal2(this object s)
        {
            return Convert.ToDecimal(s);
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


        public static bool Ec(IDataReader or, string columna)
        {
            or.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + columna + "'";
            return (or.GetSchemaTable().DefaultView.Count > 0);
        }

    }
}
