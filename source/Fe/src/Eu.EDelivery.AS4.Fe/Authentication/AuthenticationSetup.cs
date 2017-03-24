﻿using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Eu.EDelivery.AS4.Fe.Authentication
{
    public class AuthenticationSetup : IAuthenticationSetup
    {
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            RegisterOptions(services, configuration);

            var databaseName = configuration.GetSection("Authentication")["Database"];

            // Setup Identity
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = databaseName };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            services
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        public void Run(IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<JwtOptions>>().Value;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = options.SigningKey,
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = false,
                ValidAudience = options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = "JWT",
                TokenValidationParameters = tokenValidationParameters
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                context.SaveChanges();

                var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

                var user1 = new ApplicationUser { UserName = "admin" };
                var user2 = new ApplicationUser { UserName = "readonly" };

                var user1result = userManager.CreateAsync(user1, "gl0M+`pxas").Result;
                var user2result = userManager.CreateAsync(user2, "gl0M+`pxas").Result;

                userManager.AddClaimsAsync(user1, new[] { new Claim(ClaimTypes.Role, Roles.Admin) }).Wait();
                userManager.AddClaimsAsync(user2, new[] { new Claim(ClaimTypes.Role, Roles.Readonly) }).Wait();
            }
        }

        private static void RegisterOptions(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<AuthenticationConfiguration>(configuration.GetSection("Authentication"));
            services.Configure<JwtOptions>(configuration.GetSection("Authentication:JwtOptions"));
        }
    }
}