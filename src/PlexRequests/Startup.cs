using System;
using System.Collections.Generic;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PlexRequests.Middleware;
using PlexRequests.Settings;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;

namespace PlexRequests
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Environment = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters()
                .AddApiExplorer()
                .AddJsonOptions(
                options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Formatting = Formatting.Indented;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Plex Requests Api",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };
                options.AddSecurityRequirement(security);
            });

            ConfigureLogging();

            services.AddMemoryCache();
            services.AddAutoMapper();
            services.AddMediatR();

            services.Configure<AuthenticationSettings>(Configuration.GetSection(nameof(AuthenticationSettings)));
            services.Configure<TheMovieDbSettings>(Configuration.GetSection(nameof(TheMovieDbSettings)));
            services.Configure<PlexSettings>(Configuration.GetSection(nameof(PlexSettings)));
            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));

            var authSettings = Configuration.GetSection(nameof(AuthenticationSettings)).Get<AuthenticationSettings>();
            var databaseSettings = Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            
            if(Environment.IsProduction())
            {
                var dbUser = System.Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_USERNAME");
                var dbUserPass = System.Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD");
                databaseSettings.User = dbUser;
                databaseSettings.Password = dbUserPass;
            }

            services.RegisterDependencies(databaseSettings);
            services.ConfigureJwtAuthentication(authSettings, Environment.IsProduction());

            MongoDefaults.AssignIdOnInsert = true;
        }

        private void ConfigureLogging()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile("logs\\log-{Date}.txt");

            if (Environment.IsProduction())
            {
                loggerConfiguration.MinimumLevel.Information();
                loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Error);
            }
            else
            {
                loggerConfiguration.MinimumLevel.Debug();
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            loggerFactory.AddSerilog();

            PrimeSettings(app, Configuration);

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plex Requests Api");
            });

            app.UseAuthentication();

            app.UseMvc(routes => { routes.MapRoute("default", "api/{controller}/{action}"); });
        }

        private static void PrimeSettings(IApplicationBuilder app, IConfiguration configuration)
        {
            var logger = app.ApplicationServices.GetService<ILogger<Startup>>();
            var settingsService = app.ApplicationServices.GetService<ISettingsService>();

            var settings = configuration.GetSection(nameof(Settings)).Get<Store.Models.Settings>();

            logger.LogDebug($"Persisting settings to database. Overwrite: {settings.OverwriteSettings}");

            settings.ApplicationName = string.IsNullOrEmpty(settings.ApplicationName)
                ? "PlexRequests"
                : settings.ApplicationName;
            settings.PlexClientId = Guid.NewGuid();

            settingsService.PrimeSettings(settings).GetAwaiter().GetResult();
        }
    }
}
