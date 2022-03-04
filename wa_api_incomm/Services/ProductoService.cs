using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Hub.ProductoClientModel;

namespace wa_api_incomm.Services
{
    public class ProductoService : IProductoService
    {
        public object sel(string conexion, ProductoClientModelInput input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor_producto", cn))
                    {
                        ProductoModel model = new ProductoModel();
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 2;
                        model.vc_cod_distribuidor = input.codigo_distribuidor;
                        if (!String.IsNullOrEmpty(input.id_convenio))
                        {
                            model.nu_id_convenio = int.Parse(input.id_convenio);
                        }
                        if (!String.IsNullOrEmpty(input.id_producto))
                        {
                            model.nu_id_producto = int.Parse(input.id_producto);
                        }
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                        cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                        cmd.Parameters.AddWithValue("@nu_id_producto", model.nu_id_producto);
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
            if (Ec(or, "nu_id_producto"))
                m.id_producto = or["nu_id_producto"].ToString();
            if (Ec(or, "vc_desc_producto"))
                m.nombre_producto = or["vc_desc_producto"].ToString();
            if (Ec(or, "nu_precio"))
                m.precio = or["nu_precio"].ToString();

            return m;
        }

        public static bool Ec(IDataReader or, string columna)
        {
            or.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + columna + "'";
            return (or.GetSchemaTable().DefaultView.Count > 0);
        }



    }
}
