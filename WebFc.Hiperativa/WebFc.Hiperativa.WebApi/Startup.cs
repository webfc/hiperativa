using System.IO;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebFc.Hiperativa.WebApi.Middleware;
using UtilityFramework.Application.Core;

namespace WebFc.Hiperativa.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            ApplicationName = Assembly.GetEntryAssembly().GetName().Name?.Split('.')[0];

        }

        public IConfigurationRoot Configuration { get; }
        public static string ApplicationName { get; set; }

        // This method gets Race by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            /*ENABLE CORS*/
            services.AddCors(options =>
           {
               options.AddPolicy("AllowAllOrigin",
                   builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials().Build());
           });
            services.AddAutoMapper();

            /*CROP IMAGE*/
            services.AddImageResizer();
            /*CROP IMAGE*/
            services.AddJwtSwagger(ApplicationName);
            /*INJEÇÃO DE DEPENDENCIAS DE BANCO*/
            services.AddRepositoryInjection();

            /*INJEÇÃO DE DEPENDENCIAS DE SERVIÇOS*/
            services.AddServicesInjection();
        }

        // This method gets Race by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddFile($"Log/" + ApplicationName + "-{Date}.txt", LogLevel.Warning, retainedFileCountLimit: 5);

            app.UseImageResizer();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), @"Content");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(path),
                    RequestPath = new PathString("/content")
                });
            }

            app.UseCors("AllowAllOrigin");
            /*JWT TOKEN*/
            app.UseJwtTokenApiAuth(Configuration);
            /*LOG BASICO*/
            app.UseRequestResponseLoggingLite();
            /*RETORNO DE ERROS DE API*/
            app.UseResponseShowInternalServerError();
            /*RETORNO COM GZIP*/
            app.UseResponseCompression();

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
           {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{ApplicationName} - API");
               c.EnableDeepLinking();
               c.EnableFilter();
           });

        }
    }
}