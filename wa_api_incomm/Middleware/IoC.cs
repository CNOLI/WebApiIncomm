using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wa_api_incomm.Services;
using wa_api_incomm.Services.Contracts;
using wa_api_incomm.Smtp;

namespace wa_api_incomm.Middleware
{
    public static class IoC
    {
        public static IServiceCollection AddRegistration(this IServiceCollection services)
        {
            services.AddSingleton<ICategoriesService, CategoriesService>();
            services.AddSingleton<ITransaccionService, TransaccionService>();
            services.AddSingleton<IPinService, PinService>();
            services.AddSingleton<Send, Send>();
            services.AddSingleton<IProductoService, ProductoService>();

            services.AddSingleton<ISentinelService, SentinelService>();

            services.AddSingleton<IBanBifService, BanBifService>();

            services.AddSingleton<IEquifaxService, EquifaxService>();

            services.AddSingleton<IIzipayService, IzipayService>();

            return services;
        }
    }
}
