using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lamar;
using Lamar.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotiHub.Api.Configuration.Auth;
using SpotiHub.Core.Entity;
using SpotiHub.Persistence.Context;

namespace SpotiHub.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureContainer(ServiceRegistry services)
        {
            services.AddControllers();
            services.AddHttpContextAccessor();

            services.AddCommonDataProtection(Configuration);
            
            #region Identity

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = Configuration["IDENTITY_CONNECTION_STRING"];
                options.UseNpgsql(connectionString, optionsBuilder =>
                {
                    optionsBuilder
                        .MigrationsAssembly("SpotiHub.Persistence.Migrations");
                });
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            #endregion

            #region Auth

            services.AddCommonCors();
            services.AddCommonAuthentication(Configuration);
            
            #endregion
            
            #region Healthcheks
            
            services.CheckLamarConfiguration();
            
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
            serviceScope?.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

            app.UseRouting();

            app.UseCommonCors(Configuration);
            app.UseCommonAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}