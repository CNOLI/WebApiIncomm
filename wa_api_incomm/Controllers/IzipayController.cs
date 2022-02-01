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
using static wa_api_incomm.Models.Izipay_InputModel;
using static wa_api_incomm.Models.IzipayModel;
using Hub_Encrypt;

namespace wa_api_incomm.Controllers
{
    [Route("Izipay")]
    [ApiController]
    public class IzipayController : ControllerBase
    {
        private readonly IIzipayService _IIzipayService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        public IzipayController(Serilog.ILogger logger, IHostingEnvironment env, IIzipayService IIzipayService, IHttpClientFactory clientFactory)
        {
            _IIzipayService = IIzipayService;
            _logger = logger;
            _clientFactory = clientFactory;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        [HttpPost("CrearComercio")]
        public IActionResult CrearComercio([FromBody]Crear_Comercio_Input model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                return this.Ok(_IIzipayService.CrearComercio(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("ActualizarRegla")]
        public IActionResult ActualizarRegla([FromBody]Actualizar_Regla_Input model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                return this.Ok(_IIzipayService.ActualizarRegla(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}