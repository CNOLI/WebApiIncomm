using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub_Encrypt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
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
                EncrypDecrypt enc = new EncrypDecrypt();
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.nro_transaccion);
                if (a != model.clave)
                {
                    return this.BadRequest(UtilSql.sOutPutTransaccion("401", "La clave es incorrecta"));
                }
                else
                {

                    return this.Ok(_IConsultaService.Transaccion(Configuration.GetSection("SQL").Value, model));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}