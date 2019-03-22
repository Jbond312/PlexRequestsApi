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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

            MongoDefaults.AssignIdOnInsert = true;

            services.RegisterDependencies();
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

            PrimeSettings(app);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plex Requests Api");
            });

            app.UseHttpsRedirection();
            app.UseMvc(routes => { routes.MapRoute("default", "api/{controller}/{action}"); });
        }

        private static void PrimeSettings(IApplicationBuilder app)
        {
            var settingsService = app.ApplicationServices.GetService<ISettingsService>();

            var envOverwriteSettings = Environment.GetEnvironmentVariable(EnvironmentVariableKeys.OverwriteSettings);

            var overwrite = false;
            if (!string.IsNullOrEmpty(envOverwriteSettings))
            {
                bool.TryParse(envOverwriteSettings, out overwrite);
            }

            var applicationName = Environment.GetEnvironmentVariable(EnvironmentVariableKeys.ApplicationName);

            settingsService.PrimeSettings(new Store.Models.Settings
            {
                ApplicationName = string.IsNullOrEmpty(applicationName) ? "PlexRequests" : applicationName,
                PlexClientId = Guid.NewGuid()
            }, overwrite);
        }
    }
}
