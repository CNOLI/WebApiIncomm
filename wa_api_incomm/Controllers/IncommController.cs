using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using wa_api_incomm.Help;
using wa_api_incomm.Models;
using wa_api_incomm.Models.Hub;
using wa_api_incomm.Services.Contracts;
using Hub_Encrypt;

namespace wa_api_incomm.Controllers
{
    [Route("Transaccion")]
    public class IncommController : Controller
    {
        private readonly IIncommService _IIncommService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        public IncommController(Serilog.ILogger logger, IHostingEnvironment env, IIncommService IIncommService)
        {
            _IIncommService = IIncommService;
            _logger = logger;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }


        [HttpPost("generar")]
        public IActionResult generar([FromBody]Incomm_InputTransModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));

                _logger.Error(allErrors.First());
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos"));
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

                    return this.Ok(_IIncommService.execute_trans(Configuration.GetSection("SQL").Value, model));
                }

            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion"));
            }
        }

        [HttpPost("confirmar")]
        public IActionResult confirmar([FromBody]Incomm_InputITransConfirmarModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));

                _logger.Error(allErrors.First());
                return this.BadRequest(UtilSql.sOutPutTransaccion("01", "Datos incorrectos"));
            }
            try
            {
                EncrypDecrypt enc = new EncrypDecrypt();
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.nro_transaccion);
                if (a != model.clave)
                {
                    return this.BadRequest(UtilSql.sOutPutTransaccion("401", "La clave es incorrecta"));
                }
                else
                {

                    return this.Ok(_IIncommService.confirmar(Configuration.GetSection("SQL").Value, model));
                }

            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion"));
            }
        }


        [HttpPost("prSms")]
        public IActionResult prSms([FromQuery] string nroCelular)
        {
            try
            {
                return this.Ok(_IIncommService.pr_sms(Configuration.GetSection("SQL").Value, nroCelular));
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion"));
            }
        }

        [HttpPost("prEmail")]
        public IActionResult prEmail([FromQuery] string correoDestino)
        {
            try
            {
                return this.Ok(_IIncommService.pr_email(Configuration.GetSection("SQL").Value, correoDestino));
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("500", "Ocurrio un error en la transaccion"));
            }
        }
    }
}