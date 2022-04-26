using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Equifax_InputModel;
using static wa_api_incomm.Models.EquifaxModel;
using Hub_Encrypt;

namespace wa_api_incomm.Controllers
{
    [Route("Equifax")]
    [ApiController]
    public class EquifaxController : ControllerBase
    {
        private readonly IEquifaxService _IEquifaxService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        public EquifaxController(Serilog.ILogger logger, IHostingEnvironment env, IEquifaxService IEquifaxService)
        {
            _IEquifaxService = IEquifaxService;
            _logger = logger;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        [HttpPost("TipoDocIdentidad")]
        public IActionResult sel_tipo_documento_identidad([FromBody]Busqueda_Equifax_Input model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                return this.Ok(_IEquifaxService.sel_tipo_documento_identidad(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("GenerarReporte")]
        public IActionResult GenerarReporte([FromBody]GenerarReporte model)
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
                    return this.Ok(_IEquifaxService.GenerarReporte(Configuration.GetSection("SQL").Value, model));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}