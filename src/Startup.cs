using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Models;
using HappyTokenApi.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace HappyTokenApi
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Setup config for constructor dependency injection
            services.Configure<ConfigDbSettings>(Configuration.GetSection("ConfigDbSettings"));
            services.Configure<CoreDbSettings>(Configuration.GetSection("CoreDbSettings"));

            // Setup the PostgreSQL Core DB
            var coreDbSettings = Configuration.GetSection("CoreDbSettings").Get<CoreDbSettings>();
            services.AddDbContext<CoreDbContext>(options =>
            {
                options.UseNpgsql(coreDbSettings.ConnectionString);
            });

            // Setup the ArangoDB Config DB
            var configDbSettings = Configuration.GetSection("ConfigDbSettings").Get<ConfigDbSettings>();
            var configDbContext = new ConfigDbContext(configDbSettings)
                .ConfigureConnection()
                .LoadConfigDataFromDb();

            services.AddSingleton(configDbContext);

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            ConfigureAuth(app);

            app.UseMvc();
        }

        private void ConfigureAuth(IApplicationBuilder app)
        {
            var tokenSettings = Configuration.GetSection("TokenSettings").Get<TokenSettings>();

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSettings.SecretKey));

            var tokenProviderOptions = new TokenProviderOptions
            {
                Path = tokenSettings.TokenPath,
                Audience = tokenSettings.Audience,
                Issuer = tokenSettings.Issuer,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            };

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = tokenSettings.Issuer,
                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = tokenSettings.Audience,
                // Validate the token expiry
                ValidateLifetime = true,
                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters
            });

            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(tokenProviderOptions));
        }
    }
}
