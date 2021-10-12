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

namespace wa_api_incomm.Controllers
{
    [Route("Sentinel")]
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

        //[HttpPost("sel_banco")]
        //public IActionResult sel_banco([FromBody]Sentinel_InputModel.Busqueda model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
        //        _logger.Error(allErrors.First());
        //        return this.BadRequest(this.ModelState);
        //    }
        //    try
        //    {
        //        return this.Ok(_ISentinelService.sel_banco(Configuration.GetSection("SQL").Value, model));
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.BadRequest(Utilitarios.JsonErrorSel(ex));
        //    }
        //}
        [HttpPost("TipDocIdentidad")]
        public IActionResult sel_tipo_documento_identidad([FromBody]Sentinel_InputModel.Busqueda model)
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
        [HttpPost("Producto")]
        public IActionResult sel_producto([FromBody]Sentinel_InputModel.Busqueda model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(this.ModelState);
            }
            try
            {
                return this.Ok(_ISentinelService.sel_producto(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        //[HttpPost("get_precio_producto")]
        //public IActionResult get_precio_producto([FromBody]Sentinel_InputModel.Get_Producto model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
        //        _logger.Error(allErrors.First());
        //        return this.BadRequest(this.ModelState);
        //    }
        //    try
        //    {
        //        return this.Ok(_ISentinelService.get_precio_producto(Configuration.GetSection("SQL").Value, model));
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.BadRequest(Utilitarios.JsonErrorSel(ex));
        //    }
        //}
        //[HttpPost("get_saldo_distribuidor")]
        //public IActionResult get_saldo_distribuidor([FromBody]Sentinel_InputModel.Get_Distribuidor model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
        //        _logger.Error(allErrors.First());
        //        return this.BadRequest(this.ModelState);
        //    }
        //    try
        //    {
        //        return this.Ok(_ISentinelService.get_saldo_distribuidor(Configuration.GetSection("SQL").Value, model));
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.BadRequest(Utilitarios.JsonErrorSel(ex));
        //    }
        //}
        //[HttpPost("sel_transaccion")]
        //public IActionResult sel_transaccion([FromBody]Sentinel_InputModel.Sel_Transaccion model)
        //{
        //    if (!this.ModelState.IsValid)
        //    {
        //        var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
        //        _logger.Error(allErrors.First());
        //        return this.BadRequest(this.ModelState);
        //    }
        //    try
        //    {
        //        return this.Ok(_ISentinelService.sel_transaccion(Configuration.GetSection("SQL").Value, model));
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.BadRequest(Utilitarios.JsonErrorSel(ex));
        //    }
        //}
        [HttpPost("ValidarTitular")]
        public IActionResult get_validar_titular([FromBody]Sentinel_InputModel.Consultado model)
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
                return this.Ok(_ISentinelService.get_validar_titular(info, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("RegistrarSolicitudReporte")]
        public IActionResult ins_transaccion([FromBody]Sentinel_InputModel.Ins_Transaccion model)
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
                return this.Ok(_ISentinelService.ins_transaccion(Configuration.GetSection("SQL").Value, info, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}