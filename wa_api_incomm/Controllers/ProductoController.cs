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
using static wa_api_incomm.Models.Hub.ProductoClientModel;

namespace wa_api_incomm.Controllers
{
    [Route("Producto")]
    public class ProductoController : Controller
    {
        private readonly IProductoService _IProductoService;
        public IConfigurationRoot Configuration { get; }
        public ProductoController(IHostingEnvironment env, IProductoService IProductoService)
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
        public IActionResult get([FromBody]ProductoClientModelInput model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                return this.BadRequest(UtilSql.sOutPutTransaccion("97", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                return this.Ok(_IProductoService.sel(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
    }
}