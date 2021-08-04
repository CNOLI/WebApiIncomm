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

namespace wa_api_incomm.Controllers
{
    [Route("Producto")]
    public class ProductosController : Controller
    {
        private readonly IProductoService _IProductoService;
        public IConfigurationRoot Configuration { get; }
        public ProductosController(IHostingEnvironment env, IProductoService IProductoService)
        {
            _IProductoService = IProductoService;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();

        }

        [HttpPost("sel")]
        public IActionResult generar([FromBody]DistribuidorClientModel model)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(this.ModelState);
            try
            {
                return this.Ok(_IProductoService.sel(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}