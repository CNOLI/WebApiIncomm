using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.ReporteCrediticioModel;
using Hub_Encrypt;
using System.Threading;
using static wa_api_incomm.Models.Sentinel_InputModel;

namespace wa_api_incomm.Controllers
{
    [Route("ReporteCrediticio")]
    [ApiController]
    public class ReporteCrediticioController : ControllerBase
    {
        private readonly IReporteCrediticioService _IReporteCrediticioService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        public ReporteCrediticioController(Serilog.ILogger logger, IHostingEnvironment env, IReporteCrediticioService IReporteCrediticioService, IHttpClientFactory clientFactory)
        {
            _IReporteCrediticioService = IReporteCrediticioService;
            _logger = logger;
            _clientFactory = clientFactory;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        [HttpPost("validarPersona")]
        public IActionResult get_validar_titular([FromBody] Consultado model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                SentinelInfo info = Configuration.GetSection("SentinelInfo").Get<SentinelInfo>();
                return this.Ok(_IReporteCrediticioService.validar_persona(Configuration.GetSection("SQL").Value, info, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("generar")]
        public IActionResult procesar([FromBody]ReporteCrediticio_Input model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                EncrypDecrypt enc = new EncrypDecrypt();
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.id_producto);
                if (a != model.clave)
                {
                    return this.BadRequest(UtilSql.sOutPutTransaccion("401", "La clave es incorrecta"));
                }
                else
                {
                    SentinelInfo info = Configuration.GetSection("SentinelInfo").Get<SentinelInfo>();
                    return this.Ok(_IReporteCrediticioService.generar(Configuration.GetSection("SQL").Value, model, info, _clientFactory));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}