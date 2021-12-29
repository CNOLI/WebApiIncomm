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
using static wa_api_incomm.Models.Sentinel_InputModel;
using Hub_Encrypt;

namespace wa_api_incomm.Controllers
{
    [Route("prdigital/Sentinel")]
    [ApiController]
    public class SentinelController : ControllerBase
    {
        private readonly ISentinelService _ISentinelService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        public SentinelController(Serilog.ILogger logger, IHostingEnvironment env, ISentinelService ISentinelService)
        {
            _ISentinelService = ISentinelService;
            _logger = logger;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        [HttpPost("TipoDocIdentidad")]
        public IActionResult sel_tipo_documento_identidad([FromBody]Busqueda_Sentinel_Input model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                return this.Ok(_ISentinelService.sel_tipo_documento_identidad(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("ValidarTitular")]
        public IActionResult get_validar_titular([FromBody]Consultado model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                SentinelInfo info = Configuration.GetSection("SentinelInfo").Get<SentinelInfo>();
                return this.Ok(_ISentinelService.get_validar_titular(Configuration.GetSection("SQL").Value,info, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("RegistrarSolicitudReporte")]
        public IActionResult ins_transaccion([FromBody]Ins_Transaccion model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
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
                    return this.Ok(_ISentinelService.ins_transaccion(Configuration.GetSection("SQL").Value, info, model));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}