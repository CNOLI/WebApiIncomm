using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Hub_Encrypt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Controllers
{
    [Route("Transaccion")]
    public class TransaccionController : Controller
    {
        private readonly ITransaccionService _ITransaccionService;
        public IConfigurationRoot Configuration { get; }
        public TransaccionController(IHostingEnvironment env, ITransaccionService ITransaccionService)
        {
            _ITransaccionService = ITransaccionService;
            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        [HttpPost("confirmar")]
        public IActionResult Confirmar([FromBody]TransaccionModel.Transaccion_Input_Confirmar model)
        {
            try
            {
                // Validar configuracion distribuidor
                GlobalService oGlobalService = new GlobalService();
                DistribuidorModel ds = new DistribuidorModel();
                ds.vc_cod_distribuidor = model.codigo_distribuidor;
                SqlConnection con_sql = new SqlConnection(Configuration.GetSection("SQL").Value);
                con_sql.Open();
                ds = oGlobalService.get_distribuidor(con_sql, ds);
                con_sql.Close();

                EncrypDecrypt enc = new EncrypDecrypt();
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.nro_transaccion);
                if (a != model.clave && ds.bi_encriptacion_trx == true)
                {
                    return this.Ok(UtilSql.sOutPutTransaccion("98", "La clave de seguridad es incorrecta."));
                }
                else
                {

                    return this.Ok(_ITransaccionService.Confirmar(Configuration.GetSection("SQL").Value, model));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
        [HttpPost("informar")]
        public IActionResult Informar([FromBody]TransaccionModel.Transaccion_Input_Confirmar model)
        {
            try
            {
                // Validar configuracion distribuidor
                GlobalService oGlobalService = new GlobalService();
                DistribuidorModel ds = new DistribuidorModel();
                ds.vc_cod_distribuidor = model.codigo_distribuidor;
                SqlConnection con_sql = new SqlConnection(Configuration.GetSection("SQL").Value);
                con_sql.Open();
                ds = oGlobalService.get_distribuidor(con_sql, ds);
                con_sql.Close();

                EncrypDecrypt enc = new EncrypDecrypt();
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.nro_transaccion);
                if (a != model.clave && ds.bi_encriptacion_trx == true)
                {
                    return this.Ok(UtilSql.sOutPutTransaccion("98", "La clave de seguridad es incorrecta."));
                }
                else
                {

                    return this.Ok(_ITransaccionService.Informar(Configuration.GetSection("SQL").Value, model));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
    }
}