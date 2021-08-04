using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Controllers
{
    [Route("api/Pin")]
    public class PinController : Controller
    {
        private readonly IPinService _IPinService;
        public IConfigurationRoot Configuration { get; }
        public string ruta_app { get; set; }

        public PinController(IHostingEnvironment env, IPinService IPinService)
        {
            _IPinService = IPinService;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
            ruta_app = env.ContentRootPath;
        }

        [HttpPost("get")]
        public IActionResult generar([FromBody]DatosPinModel model)
        {
            try
            {
                return this.Ok(_IPinService.get(model.key,model.pin, ruta_app));
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}