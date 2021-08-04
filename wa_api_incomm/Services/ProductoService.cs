using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Services
{
    public class ProductoService : IProductoService
    {
        public object sel(string conexion, DistribuidorClientModel model)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor_producto", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 2;
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.codigo_distribuidor);
                        UtilSql.iGet(cmd, model);
                        SqlDataReader dr = cmd.ExecuteReader();
                        return Query(dr);
                    }
                } 
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }

        private List<ProductoClientModel> Query(SqlDataReader or)
        {
            var lm = new List<ProductoClientModel>();
            while (or.Read())
            {
                lm.Add(Decode(or));
            }
            return lm;
        }

        private ProductoClientModel Decode(SqlDataReader or)
        {
            ProductoClientModel m = new ProductoClientModel();
            if (Ec(or, "id_producto"))
                m.id_producto = or["id_producto"].ToString();
            if (Ec(or, "nombre_producto"))
                m.nombre_producto = or["nombre_producto"].ToString();
            if (Ec(or, "precio"))
                m.precio = or["precio"].ToString();

            return m;
        }

        public static bool Ec(IDataReader or, string columna)
        {
            or.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + columna + "'";
            return (or.GetSchemaTable().DefaultView.Count > 0);
        }



    }
}
