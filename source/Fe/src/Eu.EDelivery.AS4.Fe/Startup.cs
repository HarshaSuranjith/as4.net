﻿using System.Net;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Logging;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Runtime;
using Eu.EDelivery.AS4.Fe.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Fe
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class Startup
    {
        public IConfigurationRoot Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            var moduleMappings = services.BuildServiceProvider().GetService<IOptions<ApplicationSettings>>().Value.Modules;
            IConfigurationRoot config;
            services.AddModules(moduleMappings, (configBuilder, env) =>
            {
                configBuilder
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);
            }, out config);
            Configuration = config;

            services
                .AddMvc(options =>
                {
                    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
                })
                .AddJsonOptions(options => { options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; });
            services.AddSingleton<ILogging, Logging.Logging>();
            services.AddSingleton<ISettingsSource, FileSettingsSource>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IRuntimeLoader, RuntimeLoader>();

            services.Configure<ApplicationSettings>(Configuration.GetSection("Settings"));
            services.AddOptions();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            app.ExecuteStartupServices();
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Request.Path.StartsWithSegments("/api")) return;
                if (context.Response.StatusCode != 200)
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();

            var logger = app.ApplicationServices.GetService<ILogging>();
            var settings = app.ApplicationServices.GetService<IOptions<ApplicationSettings>>();
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        var response = new
                        {
                            IsError = true,
                            Exception = !settings.Value.ShowStackTraceInExceptions ? null : ex.Error.StackTrace
                        };
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                        logger.Error(ex.Error);
                    }
                });
            });

            app.UseMvc();
        }
    }
}