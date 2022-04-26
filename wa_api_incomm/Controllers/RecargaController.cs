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
using static wa_api_incomm.Models.RecargaModel;
using Hub_Encrypt;

namespace wa_api_incomm.Controllers
{
    [Route("Recarga")]
    [ApiController]
    public class RecargaController : ControllerBase
    {
        private readonly IRecargaService _IRecargaService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        public RecargaController(Serilog.ILogger logger, IHostingEnvironment env, IRecargaService IRecargaService, IHttpClientFactory clientFactory)
        {
            _IRecargaService = IRecargaService;
            _logger = logger;
            _clientFactory = clientFactory;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        [HttpPost("procesar")]
        public IActionResult procesar([FromBody]Recarga_Input model)
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
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.id_producto);
                if (a != model.clave)
                {
                    return this.BadRequest(UtilSql.sOutPutTransaccion("401", "La clave es incorrecta"));
                }
                else
                {
                    return this.Ok(_IRecargaService.procesar(Configuration.GetSection("SQL").Value, model, _clientFactory));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(Utilitarios.JsonErrorSel(ex));
            }
        }
    }
}