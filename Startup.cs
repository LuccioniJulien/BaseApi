using System;
using System.Text;
using BaseApi.Models;
using dotenv.net.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BaseApi {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_2);

            // ajout .env
            services.AddEnv (builder => {
                builder
                    .AddEnvFile ("./.env")
                    .AddThrowOnError (false)
                    .AddEncoding (Encoding.ASCII);
            });

            // ajout support postgres
            services.AddEntityFrameworkNpgsql ()
                .AddDbContext<DBcontext> ()
                .BuildServiceProvider ();

            // ajout JWT
            string SECRET = Environment.GetEnvironmentVariable ("SECRET");
            var key = Encoding.ASCII.GetBytes (SECRET);
            services.AddAuthentication (x => {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer (x => {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey (key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            app.UseCors (x => x
                .AllowAnyOrigin ()
                .AllowAnyMethod ()
                .AllowAnyHeader ());
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
                UpdateDatabase (app);
            } else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts ();
            }
            app.UseHttpsRedirection ();
            app.UseMvc ();
            app.UseAuthentication ();
        }

        private static void UpdateDatabase (IApplicationBuilder app) {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory> ()
                .CreateScope ()) {
                using (var context = serviceScope.ServiceProvider.GetService<DBcontext> ()) {
                    // créé la bdd si elle n'existe pas
                    context.Database.EnsureCreated ();
                }
            }
        }
    }
}