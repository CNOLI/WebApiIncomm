using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hub_Encrypt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Hub.EmpresaClientModel;
using static wa_api_incomm.Models.Hub.RubroClientModel;
using static wa_api_incomm.Models.Hub.ServicioModel;

namespace wa_api_incomm.Controllers
{
    [Route("Servicio")]
    public class ServicioController : Controller
    {
        private readonly IServicioService _IServicioService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        public ServicioController(Serilog.ILogger logger, IHostingEnvironment env, IServicioService IServicioService, IHttpClientFactory clientFactory)
        {
            _IServicioService = IServicioService;
            _logger = logger;
            _clientFactory = clientFactory;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();

        }

        [HttpPost("obtenerRubros")]
        public IActionResult obtenerRubros([FromBody]RubroClientModelInput model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                return this.Ok(_IServicioService.obtenerRubros(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("obtenerEmpresas")]
        public IActionResult obtenerEmpresas([FromBody]EmpresaClientModelInput model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                return this.Ok(_IServicioService.obtenerEmpresas(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("obtenerServicios")]
        public IActionResult obtenerServicios([FromBody]ServicioModelInput model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                return this.Ok(_IServicioService.obtenerServicios(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("obtenerDeuda")]
        public IActionResult obtenerDeuda([FromBody]ServicioObtenerDeudaPagoModelInput model)
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
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.id_servicio);
                if (a != model.clave)
                {
                    return this.BadRequest(UtilSql.sOutPutTransaccion("401", "La clave es incorrecta"));
                }
                else
                {
                    return this.Ok(_IServicioService.obtenerDeuda(Configuration.GetSection("SQL").Value, model, _clientFactory));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("procesarPago")]
        public IActionResult procesarPago([FromBody]ServicioProcesarPagoModelInput model)
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
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.id_servicio);
                if (a != model.clave)
                {
                    return this.BadRequest(UtilSql.sOutPutTransaccion("401", "La clave es incorrecta"));
                }
                else
                {
                    return this.Ok(_IServicioService.procesarPago(Configuration.GetSection("SQL").Value, model, _clientFactory));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
         }
    }
}