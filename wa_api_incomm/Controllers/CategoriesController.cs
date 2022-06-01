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
    [Route("Categories")]
    public class CategoriesController : Controller
    {
        private readonly ICategoriesService _ICategoriesService;
        public IConfigurationRoot Configuration { get; }
        public CategoriesController(IHostingEnvironment env, ICategoriesService ICategoriesService)
        {
            _ICategoriesService = ICategoriesService;
            var builder = new ConfigurationBuilder()
                       .SetBasePath(env.ContentRootPath)
                       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        //[HttpPost("upd")]
        //public IActionResult upd()
        //{
        //    try
        //    {
        //        return this.Ok(_ICategoriesService.sel(Configuration.GetSection("SQL").Value));
        //    }
        //    catch (Exception ex)
        //    {
        //        return this.BadRequest(Utilitarios.JsonErrorSel(ex));
        //    }
        //}
    }
}