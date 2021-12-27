using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using static wa_api_incomm.Models.Hub.EmpresaClientModel;
using static wa_api_incomm.Models.Hub.RubroClientModel;
using static wa_api_incomm.Models.Hub.ServicioClientModel;

namespace wa_api_incomm.Controllers
{
    [Route("Servicio")]
    public class ServicioController : Controller
    {
        private readonly IServicioService _IServicioService;
        public IConfigurationRoot Configuration { get; }
        public ServicioController(IHostingEnvironment env, IServicioService IServicioService)
        {
            _IServicioService = IServicioService;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();

        }

        [HttpPost("sel_rubros")]
        public IActionResult sel_rubros([FromBody]RubroClientModelInput model)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(this.ModelState);
            try
            {
                return this.Ok(_IServicioService.sel_rubros(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("sel_empresas")]
        public IActionResult sel_empresas([FromBody]EmpresaClientModelInput model)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(this.ModelState);
            try
            {
                return this.Ok(_IServicioService.sel_empresas(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
        [HttpPost("sel")]
        public IActionResult sel([FromBody]ServicioClientModelInput model)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(this.ModelState);
            try
            {
                return this.Ok(_IServicioService.sel(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}