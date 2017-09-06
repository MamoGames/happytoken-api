using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HappyTokenApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureConfigDb(services);

            ConfigureCoreDb(services);

            ConfigureTokenAuth(services);

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, CoreDbContext coreDbContext)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseAuthentication();

            app.UseMvc();
        }

        private void ConfigureTokenAuth(IServiceCollection services)
        {
            services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));

            var tokenSettings = Configuration.GetSection("TokenSettings").Get<TokenSettings>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidIssuer = tokenSettings.Issuer,
                            ValidAudience = tokenSettings.Audience,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey))
                        };
                    });
        }

        private void ConfigureConfigDb(IServiceCollection services)
        {
            // Setup the ArangoDB config data
            services.Configure<ConfigDbSettings>(Configuration.GetSection("ConfigDbSettings"));
            services.AddSingleton<ConfigDbContext>();
        }

        private void ConfigureCoreDb(IServiceCollection services)
        {
            // Setup the PostgreSQL Core DB
            var coreDbSettings = Configuration.GetSection("CoreDbSettings").Get<CoreDbSettings>();
            services.AddDbContext<CoreDbContext>(options => options.UseNpgsql(coreDbSettings.ConnectionString));
        }
    }
}
