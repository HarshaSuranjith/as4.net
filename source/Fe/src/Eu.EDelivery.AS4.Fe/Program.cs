﻿using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Eu.EDelivery.AS4.Fe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseEnvironment(args != null && args.Contains("inprocess") ? "inprocess" : "production")
                .UseKestrel()
                .UseWebRoot("ui/dist")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
