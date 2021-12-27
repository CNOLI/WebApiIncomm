using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Hub.EmpresaClientModel;
using static wa_api_incomm.Models.Hub.RubroClientModel;
using static wa_api_incomm.Models.Hub.ServicioClientModel;

namespace wa_api_incomm.Services
{
    public class ServicioService : IServicioService
    {
        public object sel_rubros(string conexion, RubroClientModelInput input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor_rubros", cn))
                    {
                        List<RubroClientModel> ls = new List<RubroClientModel>();

                        ProductoModel model = new ProductoModel();
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 1;
                        model.vc_cod_distribuidor = input.codigo_distribuidor;
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                        UtilSql.iGet(cmd, model);
                        SqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            RubroClientModel m = new RubroClientModel();
                            if (Ec(dr, "nu_id_rubro"))
                                m.id_rubro = dr["nu_id_rubro"].ToString();
                            if (Ec(dr, "vc_cod_rubro"))
                                m.codigo_rubro = dr["vc_cod_rubro"].ToString();
                            if (Ec(dr, "vc_desc_rubro"))
                                m.nombre_rubro = dr["vc_desc_rubro"].ToString();

                            ls.Add(m);
                        }
                        return ls;

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }
        public object sel_empresas(string conexion, EmpresaClientModelInput input)
        {
            using (SqlConnection cn = new SqlConnection(conexion))
            {
                try
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("tisi_global.usp_sel_distribuidor_empresas", cn))
                    {
                        List<EmpresaClientModel> ls = new List<EmpresaClientModel>();

                        ProductoModel model = new ProductoModel();
                        cmd.CommandType = CommandType.StoredProcedure;
                        model.nu_tran_ruta = 1;
                        model.vc_cod_distribuidor = input.codigo_distribuidor;
                        if (!String.IsNullOrEmpty(input.id_rubro))
                        {
                            model.nu_id_rubro = int.Parse(input.id_rubro);
                        }
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                        cmd.Parameters.AddWithValue("@nu_id_rubro", model.nu_id_rubro);
                        UtilSql.iGet(cmd, model);
                        SqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            EmpresaClientModel m = new EmpresaClientModel();
                            if (Ec(dr, "nu_id_empresa"))
                                m.id_empresa = dr["nu_id_empresa"].ToString();
                            if (Ec(dr, "vc_nro_doc_identidad"))
                                m.ruc = dr["vc_nro_doc_identidad"].ToString();
                            if (Ec(dr, "vc_nombre"))
                                m.nombre = dr["vc_nombre"].ToString();

                            ls.Add(m);
                        }
                        return ls;

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }
        public object sel(string conexion, ServicioClientModelInput input)
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
                        model.nu_tran_ruta = 3;
                        model.vc_cod_distribuidor = input.codigo_distribuidor;
                        if (!String.IsNullOrEmpty(input.id_empresa))
                        {
                            model.nu_id_empresa = int.Parse(input.id_empresa);
                        }
                        cmd.Parameters.AddWithValue("@vc_cod_distribuidor", model.vc_cod_distribuidor);
                        cmd.Parameters.AddWithValue("@nu_id_empresa", model.nu_id_empresa);
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

        private List<ServicioClientModel> Query(SqlDataReader or)
        {
            var lm = new List<ServicioClientModel>();
            while (or.Read())
            {
                lm.Add(Decode(or));
            }
            return lm;
        }

        private ServicioClientModel Decode(SqlDataReader or)
        {
            ServicioClientModel m = new ServicioClientModel();
            if (Ec(or, "nu_id_producto"))
                m.id_servicio = or["nu_id_producto"].ToString();
            if (Ec(or, "vc_desc_producto"))
                m.nombre_servicio = or["vc_desc_producto"].ToString();

            return m;
        }

        public static bool Ec(IDataReader or, string columna)
        {
            or.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + columna + "'";
            return (or.GetSchemaTable().DefaultView.Count > 0);
        }



    }
}
