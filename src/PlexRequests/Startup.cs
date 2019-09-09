using System;
using System.Collections.Generic;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PlexRequests.Core.Settings;
using PlexRequests.Filters;
using PlexRequests.Middleware;
using PlexRequests.Repository.Models;
using Serilog;
using Serilog.Events;

namespace PlexRequests
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Environment = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore(options => options.EnableEndpointRouting = false)
                .AddMvcOptions(options => options.Filters.Add(typeof(ValidationFilter)))
                .AddAuthorization()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .AddApiExplorer()
                .AddDataAnnotations();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Plex Requests Api",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
                options.EnableAnnotations();
            });

            ConfigureLogging();

            services.AddMemoryCache();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            services.AddAutoMapper(assemblies);
            services.AddMediatR(assemblies);

            services.Configure<AuthenticationSettings>(Configuration.GetSection(nameof(AuthenticationSettings)));
            services.Configure<TheMovieDbSettings>(Configuration.GetSection(nameof(TheMovieDbSettings)));
            services.Configure<PlexSettings>(Configuration.GetSection(nameof(PlexSettings)));
            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));

            var authSettings = Configuration.GetSection(nameof(AuthenticationSettings)).Get<AuthenticationSettings>();
            var databaseSettings = Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();

            services.RegisterDependencies(databaseSettings);
            services.ConfigureJwtAuthentication(authSettings, Environment.IsProduction());

            MongoDefaults.AssignIdOnInsert = true;
        }

        private void ConfigureLogging()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console();

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
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

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(routes => { routes.MapControllerRoute("default", "api/{controller}/{action}"); });
        }

        private void PrimeSettings(IApplicationBuilder app, IConfiguration configuration)
        {
            var logger = app.ApplicationServices.GetService<ILogger<Startup>>();
            var settingsService = app.ApplicationServices.GetService<ISettingsService>();

            var settings = configuration.GetSection(nameof(Settings)).Get<Settings>();

            logger.LogDebug($"Persisting settings to database. Overwrite: {settings.OverwriteSettings}");

            settingsService.PrimeSettings(settings).GetAwaiter().GetResult();
        }
    }
}
