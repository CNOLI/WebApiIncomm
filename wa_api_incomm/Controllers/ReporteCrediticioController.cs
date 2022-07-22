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
using static wa_api_incomm.Models.ReporteCrediticioModel;
using Hub_Encrypt;
using System.Threading;
using static wa_api_incomm.Models.Sentinel_InputModel;
using static wa_api_incomm.Models.Hub.ProductoClientModel;
using wa_api_incomm.Services;
using wa_api_incomm.Models.Hub;
using System.Data.SqlClient;

namespace wa_api_incomm.Controllers
{
    [Route("ReporteCrediticio")]
    [ApiController]
    public class ReporteCrediticioController : ControllerBase
    {
        private readonly IReporteCrediticioService _IReporteCrediticioService;
        private readonly IProductoService _IProductoService;
        public IConfigurationRoot Configuration { get; }
        private readonly Serilog.ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        public ReporteCrediticioController(Serilog.ILogger logger, IHostingEnvironment env, IReporteCrediticioService IReporteCrediticioService, IHttpClientFactory clientFactory, IProductoService IProductoService)
        {
            _IReporteCrediticioService = IReporteCrediticioService;
            _IProductoService = IProductoService;
            _logger = logger;
            _clientFactory = clientFactory;

            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        [HttpPost("listar")]
        public IActionResult get_reportes([FromBody] ProductoClientModelInput model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                return this.BadRequest(UtilSql.sOutPutTransaccion("97", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                model.id_tipo_producto = "4";
                return this.Ok(_IProductoService.sel(Configuration.GetSection("SQL").Value, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
        [HttpPost("validarPersona")]
        public IActionResult get_validar_titular([FromBody] Consultado model)
        {
            if (!this.ModelState.IsValid)
            {
                var allErrors = this.ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage));
                _logger.Error(allErrors.First());
                return this.BadRequest(UtilSql.sOutPutTransaccion("97", "Datos incorrectos: " + allErrors.First()));
            }
            try
            {
                SentinelInfo info = Configuration.GetSection("SentinelInfo").Get<SentinelInfo>();
                return this.Ok(_IReporteCrediticioService.validar_persona(Configuration.GetSection("SQL").Value, info, model));
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
        [HttpPost("generar")]
        public IActionResult generar([FromBody]ReporteCrediticio_Input model)
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
                    SentinelInfo info = Configuration.GetSection("SentinelInfo").Get<SentinelInfo>();
                    return this.Ok(_IReporteCrediticioService.generar(Configuration.GetSection("SQL").Value, model, info, _clientFactory));
                }
            }
            catch (Exception ex)
            {
                return this.BadRequest(UtilSql.sOutPutTransaccion("99", "Hubo un error al procesar la transacción, vuelva a intentarlo en unos minutos."));
            }
        }
    }
}