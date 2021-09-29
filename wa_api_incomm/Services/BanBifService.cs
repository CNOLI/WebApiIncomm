using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.ApiRest;
using wa_api_incomm.Models;
using wa_api_incomm.Models.BanBif;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.BanBif.Response;

namespace wa_api_incomm.Services
{
    public class BanBifService : IBanBifService
    {
        public int nu_id_convenio = 3;

        public object sel_rubro_recaudador(string conexion)
        {
            List<RubroModel> ls_rubro = new List<RubroModel>();
            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.get_rubros_recaudador().Result;

                foreach (var item in (List<Response.E_datos>)result.datos)
                {
                    RubroModel e_rubro = new RubroModel();
                    e_rubro.vc_cod_rubro = item.codigo;
                    e_rubro.vc_desc_rubro = item.descripcion;
                    ls_rubro.Add(e_rubro);
                }

                //Guardar en BD
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ls_rubro;
        }
        public object sel_empresa_rubros(string conexion, RubroModel model)
        {
            List<EmpresaModel> ls_empresa = new List<EmpresaModel>();
            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.get_empresa_rubros(model).Result;

                foreach (var item in (List<Response.E_datos>)result.datos)
                {
                    EmpresaModel e_empresa = new EmpresaModel();
                    e_empresa.vc_cod_empresa = item.codigo;
                    e_empresa.vc_nro_doc_empresa = item.documento;
                    e_empresa.vc_nombre_empresa = item.nombre;
                    e_empresa.vc_razon_social_empresa = item.razonSocial;
                    foreach (var item_rubro in item.rubros)
                    {
                        if (!(item_rubro.codigo is null))
                        {
                            RubroModel e_rubro = new RubroModel();
                            e_rubro.vc_cod_rubro = item_rubro.codigo;
                            e_rubro.vc_desc_rubro = item_rubro.descripcion;
                            e_empresa.ls_rubro.Add(e_rubro);
                        }
                    }
                    foreach (var item_canal in item.canales)
                    {
                        if (!(item_canal.codigo is null))
                        {
                            CanalModel e_canal = new CanalModel();
                            e_canal.vc_cod_canal = item_canal.codigo;
                            e_canal.vc_desc_canal = item_canal.descripcion;
                            e_empresa.ls_canal.Add(e_canal);
                        }
                    }
                    ls_empresa.Add(e_empresa);
                }

                //Guardar en BD
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ls_empresa;
        }

        public object sel_convenio(string conexion, EmpresaModel model)
        {
            List<Models.BanBif.ConvenioModel> ls_convenio = new List<Models.BanBif.ConvenioModel>();
            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.get_lista_convenio(model).Result;

                foreach (var item in (List<Response.E_datos>)result.datos)
                {
                    Models.BanBif.ConvenioModel e_convenio = new Models.BanBif.ConvenioModel();
                    e_convenio.vc_cod_convenio = item.codigo;
                    e_convenio.vc_desc_convenio = item.descripcion;

                    e_convenio.e_empresa.vc_cod_empresa = item.empresa.codigo;
                    e_convenio.e_empresa.vc_nro_doc_empresa = item.empresa.documento;
                    e_convenio.e_empresa.vc_nombre_empresa = item.empresa.nombre;
                    e_convenio.e_empresa.vc_estado_empresa = item.empresa.estado;

                    e_convenio.e_producto.vc_cod_producto = item.producto.codigo;

                    foreach (var item_canal in item.canales)
                    {
                        if (!(item_canal.codigo is null))
                        {
                            CanalModel e_canal = new CanalModel();
                            e_canal.vc_cod_canal = item_canal.codigo;
                            e_canal.vc_desc_canal = item_canal.descripcion;
                            e_convenio.ls_canal.Add(e_canal);
                        }
                    }
                    ls_convenio.Add(e_convenio);

                }

                //Guardar en BD
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ls_convenio;
        }
        public object get_convenio(string conexion, Models.BanBif.ConvenioModel model)
        {
            Models.BanBif.ConvenioModel e_convenio = new Models.BanBif.ConvenioModel();
            SqlConnection con_sql = new SqlConnection(conexion);
            SqlTransaction tran_sql = null;
            SqlCommand cmd = null;
            try
            {
                BanBifApi client = new BanBifApi();

                var result = client.get_convenio(model).Result;

                Response.E_datos e_datos = (Response.E_datos)result.datos[0];

                e_convenio.vc_cod_convenio = e_datos.codigo;
                e_convenio.vc_desc_convenio = e_datos.descripcion;

                e_convenio.e_empresa.vc_cod_empresa = e_datos.empresa.codigo;
                e_convenio.e_empresa.vc_nro_doc_empresa = e_datos.empresa.documento;
                e_convenio.e_empresa.vc_nombre_empresa = e_datos.empresa.nombre;
                e_convenio.e_empresa.vc_estado_empresa = e_datos.empresa.estado;

                e_convenio.e_producto.vc_cod_producto = e_datos.producto.codigo;

                foreach (var item_canal in e_datos.canales)
                {
                    if (!(item_canal.codigo is null))
                    {
                        CanalModel e_canal = new CanalModel();
                        e_canal.vc_cod_canal = item_canal.codigo;
                        e_canal.vc_desc_canal = item_canal.descripcion;
                        e_convenio.ls_canal.Add(e_canal);
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return e_convenio;
        }
    }
}
