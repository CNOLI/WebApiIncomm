using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;

namespace wa_api_incomm.Controllers
{
    [Route("Transaccion")]
    public class TransaccionController : Controller
    {
        private readonly ITransaccionService _ITransaccionService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        public TransaccionController(Serilog.ILogger logger, IHostingEnvironment env, ITransaccionService ITransaccionService)
        {
            _ITransaccionService = ITransaccionService;
            _logger = logger;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }


        [HttpPost("generar")]
        public IActionResult generar([FromBody]InputTransModel model)
        {


            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));

                _logger.Error(allErrors.First());
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos"));
            }                
            try
            {
                return this.Ok(_ITransaccionService.execute_trans(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion"));
            }
        }
    }
}