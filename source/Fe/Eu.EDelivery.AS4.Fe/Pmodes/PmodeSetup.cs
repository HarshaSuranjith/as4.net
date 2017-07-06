﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    public class PmodeSetup : IPmodeSetup
    {
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<PmodeSettings>(configuration.GetSection("Pmodes"));

            services.AddSingleton<IAs4PmodeSource, As4PmodeSource>();
            services.AddSingleton<IPmodeService, PmodeService>();
        }
    }
}