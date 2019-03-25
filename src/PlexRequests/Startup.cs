using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PlexRequests.Settings;
using Swashbuckle.AspNetCore.Swagger;
using Settings = PlexRequests.Store.Models.Settings;

namespace PlexRequests
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
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

            services.AddMemoryCache();

            services.Configure<AuthenticationSettings>(Configuration.GetSection(nameof(AuthenticationSettings)));

            var authSettings = Configuration.GetSection(nameof(AuthenticationSettings)).Get<AuthenticationSettings>();
            var appSettings = Configuration.GetSection(nameof(Settings)).Get<Store.Models.Settings>();
            
            services.RegisterDependencies(appSettings);
            services.ConfigureJwtAuthentication(authSettings, Environment.IsProduction());

            MongoDefaults.AssignIdOnInsert = true;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            PrimeSettings(app, Configuration);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plex Requests Api");
            });

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseMvc(routes => { routes.MapRoute("default", "api/{controller}/{action}"); });
        }

        private static void PrimeSettings(IApplicationBuilder app, IConfiguration configuration)
        {
            var settingsService = app.ApplicationServices.GetService<ISettingsService>();

            var settings = configuration.GetSection(nameof(Settings)).Get<Store.Models.Settings>();

            settings.ApplicationName = string.IsNullOrEmpty(settings.ApplicationName)
                ? "PlexRequests"
                : settings.ApplicationName;
            settings.PlexClientId = Guid.NewGuid();

            settingsService.PrimeSettings(settings).GetAwaiter().GetResult();
        }
    }
}
