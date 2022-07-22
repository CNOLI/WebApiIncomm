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
    [Route("Consulta")]
    public class ConsultaController : Controller
    {
        private readonly IConsultaService _IConsultaService;
        public IConfigurationRoot Configuration { get; }
        public ConsultaController(IHostingEnvironment env, IConsultaService IConsultaService)
        {
            _IConsultaService = IConsultaService;
            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        [HttpPost("transaccion")]
        public IActionResult Transaccion([FromBody]Consulta.Transaccion_Input model)
        {
            /*PRUEBA
            {
              "fecha_envio": "20201009185835711",
              "codigo_distribuidor": "0000009",
              "codigo_comercio": "MX000002",
              "nro_transaccion": "1276",
              "clave": "uutñtÑxÑvxzttñequtuzy"
            }
            */
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
                    return this.Ok(UtilSql.sOutPutTransaccion("98", "La clave de seguridad es incorrecta"));
                }
                else
                {

                    return this.Ok(_IConsultaService.Transaccion(Configuration.GetSection("SQL").Value, model));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
        [HttpPost("estado")]
        public IActionResult Estado([FromBody]Consulta.Transaccion_Input_Estado model)
        {
            try
            {
                return this.Ok(_IConsultaService.Estado(Configuration.GetSection("SQL").Value, model));

            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
        [HttpPost("saldo")]
        public IActionResult Saldo([FromBody] Consulta.Distribuidor_Saldo_Input model)
        {
            try
            {
                return this.Ok(_IConsultaService.Saldo(Configuration.GetSection("SQL").Value, model));

            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
    }
}