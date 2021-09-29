using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
using wa_api_incomm.Models.BanBif;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BanBifController : ControllerBase
    {
        private readonly IBanBifService _IBanBifService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        public BanBifController(Serilog.ILogger logger, IHostingEnvironment env, IBanBifService IBanBifService)
        {
            _IBanBifService = IBanBifService;
            _logger = logger;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        [HttpPost("sel_rubro_recaudador")]
        public IActionResult sel_rubro_recaudador()
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                return this.Ok(_IBanBifService.sel_rubro_recaudador(Configuration.GetSection("SQL").Value));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("sel_empresa_rubros")]
        public IActionResult sel_empresa_rubros([FromBody]RubroModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                return this.Ok(_IBanBifService.sel_empresa_rubros(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("sel_convenio")]
        public IActionResult sel_convenio([FromBody]EmpresaModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                return this.Ok(_IBanBifService.sel_convenio(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("get_convenio")]
        public IActionResult get_convenio([FromBody]ConvenioModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                return this.Ok(_IBanBifService.get_convenio(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}