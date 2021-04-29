using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using virgollanding.AuthurizationService;
using virgollanding.Helper;
using virgollanding.Models;

namespace virgollanding
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment _env)
        {
            Configuration = configuration;
            environment = _env;
        }

        public readonly string AllowOrigin = "AllowOrigin";
        public readonly IWebHostEnvironment environment;
        public IConfiguration Configuration { get; }
        public IConfigurationRoot configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy(
                AllowOrigin , builder => {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                        
                }
            ));
            
            string conStr = "" ; 

            if(!environment.IsDevelopment())
            {
                // IConfigurationSection section = Configuration.GetSection("AppSettings");

                // section.Get<AppSettings>();
                
                // conStr = Configuration.GetConnectionString("PublishConnection_PS");
                string host = Environment.GetEnvironmentVariable("VIRGOL_DATABASE_HOST");
                string port = Environment.GetEnvironmentVariable("VIRGOL_DATABASE_PORT");
                string name = Environment.GetEnvironmentVariable("VIRGOL_DATABASE_NAME");
                string userName = Environment.GetEnvironmentVariable("VIRGOL_DATABASE_USER");
                string password = Environment.GetEnvironmentVariable("VIRGOL_DATABASE_PASSWORD");

                conStr = string.Format("Server={0};Port={1};Database={2};Username={3};Password={4}" , host ,port ,  name , userName ,password);
                
                AppSettings.FarazAPI_URL = Environment.GetEnvironmentVariable("VIRGOL_FARAZAPI_URL");
                AppSettings.FarazAPI_SendNumber = Environment.GetEnvironmentVariable("VIRGOL_FARAZAPI_SENDER_NUMBER");
                AppSettings.FarazAPI_Username = Environment.GetEnvironmentVariable("VIRGOL_FARAZAPI_USERNAME");
                AppSettings.FarazAPI_Password = Environment.GetEnvironmentVariable("VIRGOL_FARAZAPI_PASSWORD");
                AppSettings.FarazAPI_ApiKey = Environment.GetEnvironmentVariable("VIRGOL_FARAZAPI_API_KEY");

                AppSettings.smtpHost = Environment.GetEnvironmentVariable("VIRGOL_SMTP_HOST");
                AppSettings.smtpPassword = Environment.GetEnvironmentVariable("VIRGOL_SMTP_PASS");
                AppSettings.smtpPort = Environment.GetEnvironmentVariable("VIRGOL_SMTP_PORT");

                AppSettings.JWTSecret = Environment.GetEnvironmentVariable("VIRGOL_JWT_SECRET");
            }
            else
            {
                IConfigurationSection section = Configuration.GetSection("AppSettings");
                section.Get<AppSettings>();
        
                conStr = Configuration.GetConnectionString("ConnectionSTR");
            }
           

            AppSettings appSettings = new AppSettings();

            Console.WriteLine(appSettings.ToString());
        
            services.AddDbContext<AppDbContext>(options =>{
                options.UseNpgsql(conStr);
            });

            services.AddIdentity<IdentityUser<int> , IdentityRole<int>>(
                options => 
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                })
                .AddRoles<IdentityRole<int>>() 
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            
            
            var key = Encoding.ASCII.GetBytes(AppSettings.JWTSecret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidAudience = "http://panel.vir-gol.ir",
                    ValidIssuer = "http://panel.vir-gol.ir"
                };
            });

            services.AddHttpContextAccessor();
            // services.AddTransient<IAuthorizationPolicyProvider, MinimumTimeSpendPolicy>();  
            // services.AddSingleton<IAuthorizationHandler, MinimumTimeSpendHandler>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "virgollanding", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(AllowOrigin);
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "virgollanding v1"));
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
        
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
