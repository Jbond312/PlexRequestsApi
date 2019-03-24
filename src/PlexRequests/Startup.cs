using System;
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

namespace PlexRequests
{
    public class Startup
    {
        private const string SettingsKey = "PlexRequests";

        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore()
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
            });

            services.AddMemoryCache();

            MongoDefaults.AssignIdOnInsert = true;

            var settings = Configuration.GetSection(SettingsKey).Get<Store.Models.Settings>();

            services.RegisterDependencies(settings);
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

            app.UseHttpsRedirection();
            app.UseMvc(routes => { routes.MapRoute("default", "api/{controller}/{action}"); });
        }

        private static void PrimeSettings(IApplicationBuilder app, IConfiguration configuration)
        {
            var settingsService = app.ApplicationServices.GetService<ISettingsService>();

            var settings = configuration.GetSection(SettingsKey).Get<Store.Models.Settings>();

            settings.ApplicationName = string.IsNullOrEmpty(settings.ApplicationName)
                ? SettingsKey
                : settings.ApplicationName;
            settings.PlexClientId = Guid.NewGuid();

            settingsService.PrimeSettings(settings);
        }
    }
}
