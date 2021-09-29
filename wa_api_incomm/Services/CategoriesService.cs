using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Services
{
    public class CategoriesService : ICategoriesService
    {
        public object sel(string conexion)
        {

            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;

            try
            {

                con_sql.Open();
                ConvenioModel convenio = get_convenio(con_sql, 1);

                IncommApi client = new IncommApi(convenio.vc_merchant_id, convenio.vc_clave_encrip_aut, convenio.vc_pos_id, convenio.vc_source_header);

                con_sql.Close();
                var result = client.SelCategories().Result;


                //Categories result = new Categories();
                //string path = @"c:\product.json";
                //using (StreamReader jsonStream = File.OpenText(path))
                //{
                //    var json = jsonStream.ReadToEnd();
                //    result = JsonConvert.DeserializeObject<Categories>(json);
                //}


                if (result.errorCode == "200")
                {

                    con_sql.Open();
                    tran_sql = con_sql.BeginTransaction();

                    foreach (var categorie in result.categories)
                    {

                        //ins categories

                        CategoriaModel cat = new CategoriaModel();
                        cat.nu_id_convenio = 1;
                        cat.nu_id_categoria = categorie.id;
                        cat.vc_desc_categoria = categorie.name;
                        cat.vc_url_imagen = categorie.productImage;
                        cat.bi_estado = categorie.status == "ENABLED" ? true : false;
                        //tran
                        cat.vc_tran_usua_regi = "API";

                        cmd = insCategoria(con_sql, tran_sql, cat);

                        if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                        {
                            tran_sql.Rollback();
                            return UtilSql.sOutPut(cmd);
                        }
                        int? id_categoria_ins = cmd.Parameters["@nu_tran_pkey"].Value.ToInt();

                        foreach (var product in categorie.products)
                        {
                            TipoProductoModel tp = new TipoProductoModel();
                            tp.nu_id_convenio = 1;
                            tp.vc_desc_tipo_producto = product.type;
                            //tran
                            tp.vc_tran_usua_regi = "API";

                            cmd = insTipoProducto(con_sql, tran_sql, tp);

                            if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                            {
                                tran_sql.Rollback();
                                return UtilSql.sOutPut(cmd);
                            }
                            int? id_tipo_producto_ins = cmd.Parameters["@nu_tran_pkey"].Value.ToInt();


                            SubTipoProductoModel stp = new SubTipoProductoModel();
                            stp.nu_id_convenio = 1;
                            stp.vc_desc_sub_tipo_producto = product.subType;
                            //tran
                            stp.vc_tran_usua_regi = "API";

                            cmd = insSubTipoProducto(con_sql, tran_sql, stp);

                            if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                            {
                                tran_sql.Rollback();
                                return UtilSql.sOutPut(cmd);
                            }
                            int? id_sub_tipo_producto_ins = cmd.Parameters["@nu_tran_pkey"].Value.ToInt();

                            ProductoModel pm = new ProductoModel();
                            pm.vc_cod_producto = product.productCode;
                            pm.vc_desc_producto = product.name;
                            pm.nu_id_convenio = 1;
                            pm.nu_valor_facial = product.amount;
                            pm.vc_cod_ean = product.eanCode;
                            pm.vc_url_imagen = product.productImage;
                            pm.nu_id_categoria = id_categoria_ins.Value;
                            pm.nu_id_tipo_producto = id_tipo_producto_ins.Value;
                            pm.nu_id_sub_tipo_producto = id_sub_tipo_producto_ins.Value;
                            pm.bi_afecto_impuesto = product.calculateIva;
                            pm.bi_estado = product.status == "ENABLED" ? true : false;

                            //tran
                            pm.vc_tran_usua_regi = "API";

                            //ins products
                            cmd = insProducto(con_sql, tran_sql, pm);
                            if (cmd.Parameters["@nu_tran_stdo"].Value.ToDecimal() == 0)
                            {
                                tran_sql.Rollback();
                                return UtilSql.sOutPut(cmd);
                            }
                            int? id_producto = cmd.Parameters["@nu_tran_pkey"].Value.ToInt();
                        }




                    }

                    tran_sql.Commit();

                    return UtilSql.sOutPutSuccess("OK");
                }
                else
                {
                    return UtilSql.sOutPutCatch(result.errorMessage);
                }
            }
            catch (Exception ex)
            {
                tran_sql.Rollback();
                return UtilSql.sOutPutCatch(ex.Message);
            }
            finally 
            {
                if (con_sql.State == ConnectionState.Open) con_sql.Close();
            }
        }        
        private static SqlCommand insCategoria(SqlConnection cn, SqlTransaction tran, CategoriaModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_categoria_incomm", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                cmd.Parameters.AddWithValue("@nu_id", model.nu_id_categoria);
                cmd.Parameters.AddWithValue("@vc_desc_categoria", model.vc_desc_categoria);
                cmd.Parameters.AddWithValue("@vc_url_imagen", model.vc_url_imagen);
                cmd.Parameters.AddWithValue("@bi_estado", model.bi_estado);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        private static SqlCommand insTipoProducto(SqlConnection cn, SqlTransaction tran, TipoProductoModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_tipo_producto_incomm", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                cmd.Parameters.AddWithValue("@vc_desc_tipo_producto", model.vc_desc_tipo_producto);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        private static SqlCommand insSubTipoProducto(SqlConnection cn, SqlTransaction tran, SubTipoProductoModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_sub_tipo_producto_incomm", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                cmd.Parameters.AddWithValue("@vc_desc_sub_tipo_producto", model.vc_desc_sub_tipo_producto);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }
        private static SqlCommand insProducto(SqlConnection cn, SqlTransaction tran, ProductoModel model)
        {
            using (SqlCommand cmd = new SqlCommand("tisi_global.usp_ins_producto_incomm", cn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@vc_cod_producto", model.vc_cod_producto);
                cmd.Parameters.AddWithValue("@vc_desc_producto", model.vc_desc_producto);
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                cmd.Parameters.AddWithValue("@nu_valor_facial", model.nu_valor_facial);
                cmd.Parameters.AddWithValue("@vc_cod_ean", model.vc_cod_ean);
                cmd.Parameters.AddWithValue("@vc_url_imagen", model.vc_url_imagen);
                cmd.Parameters.AddWithValue("@nu_id_categoria", model.nu_id_categoria);
                cmd.Parameters.AddWithValue("@nu_id_tipo_producto", model.nu_id_tipo_producto);
                cmd.Parameters.AddWithValue("@nu_id_sub_tipo_producto", model.nu_id_sub_tipo_producto);
                cmd.Parameters.AddWithValue("@bi_afecto_impuesto", model.bi_afecto_impuesto);
                cmd.Parameters.AddWithValue("@bi_estado", model.bi_estado);
                UtilSql.iIns(cmd, model);
                cmd.ExecuteNonQuery();
                return cmd;
            }
        }

        private ConvenioModel get_convenio(SqlConnection cn, int nu_id_convenio)
        {
            ConvenioModel model = new ConvenioModel();
            using (var cmd = new SqlCommand("tisi_global.usp_get_convenio_incomm", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                model.nu_id_convenio = nu_id_convenio;
                cmd.Parameters.AddWithValue("@nu_id_convenio", model.nu_id_convenio);
                UtilSql.iGet(cmd, model);
                var dr = cmd.ExecuteReader();
                if (dr.Read())

                {
                    if (UtilSql.Ec(dr, "vc_clave_encrip_aut"))
                        model.vc_clave_encrip_aut = dr["vc_clave_encrip_aut"].ToString();
                    if (UtilSql.Ec(dr, "vc_merchant_id"))
                        model.vc_merchant_id = dr["vc_merchant_id"].ToString();
                    if (UtilSql.Ec(dr, "vc_pos_id"))
                        model.vc_pos_id = dr["vc_pos_id"].ToString();
                    if (UtilSql.Ec(dr, "vc_source_header"))
                        model.vc_source_header = dr["vc_source_header"].ToString();
                    if (UtilSql.Ec(dr, "vc_source_body"))
                        model.vc_source_body = dr["vc_source_body"].ToString();
                    if (UtilSql.Ec(dr, "vc_nro_ip"))
                        model.vc_nro_ip = dr["vc_nro_ip"].ToString();
                }
            }
            return model;
        }






    }
}
