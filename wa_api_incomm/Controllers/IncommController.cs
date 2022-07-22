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
using wa_api_incomm.Services;
using System.Data.SqlClient;

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
                return this.BadRequest(UtilSql.sOutPutTransaccion("97", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                // Validar configuracion distribuidor
                GlobalService oGlobalService = new GlobalService();
                DistribuidorModel ds = new DistribuidorModel();
                ds.vc_cod_distribuidor = model.codigo_distribuidor;
                SqlConnection con_sql = new SqlConnection(Configuration.GetSection("SQL").Value);
                con_sql.Open();
                ds = oGlobalService.get_distribuidor(con_sql, ds);
                con_sql.Close();

                EncrypDecrypt enc = new EncrypDecrypt();
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.id_producto);
                if (a != model.clave && ds.bi_encriptacion_trx == true)
                {
                    return this.Ok(UtilSql.sOutPutTransaccion("98", "La clave de seguridad es incorrecta."));
                }
                else
                {
                    return this.Ok(_IIncommService.execute_trans(Configuration.GetSection("SQL").Value, model));
                }

            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }


        [HttpPost("extornar")]
        public IActionResult extornar([FromBody]Incomm_InputTransExtornoModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));

                _logger.Error(allErrors.First());
                return this.BadRequest(UtilSql.sOutPutTransaccion("97", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                // Validar configuracion distribuidor
                GlobalService oGlobalService = new GlobalService();
                DistribuidorModel ds = new DistribuidorModel();
                ds.vc_cod_distribuidor = model.codigo_distribuidor;
                SqlConnection con_sql = new SqlConnection(Configuration.GetSection("SQL").Value);
                con_sql.Open();
                ds = oGlobalService.get_distribuidor(con_sql, ds);
                con_sql.Close();

                EncrypDecrypt enc = new EncrypDecrypt();
                var a = enc.ENCRYPT(model.fecha_envio, model.codigo_distribuidor, model.codigo_comercio, model.id_producto);
                if (a != model.clave && ds.bi_encriptacion_trx == true)
                {
                    return this.Ok(UtilSql.sOutPutTransaccion("98", "La clave de seguridad es incorrecta."));
                }
                else
                {

                    return this.Ok(_IIncommService.extornar(Configuration.GetSection("SQL").Value, model));
                }

            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
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
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
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
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
    }
}