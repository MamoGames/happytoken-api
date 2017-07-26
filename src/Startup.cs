using HappyTokenApi.Data.Config;
using HappyTokenApi.Data.Core;
using HappyTokenApi.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.Configure<ConfigDbSettings>(Configuration.GetSection("ConfigDbSettings"));
            services.Configure<CoreDbSettings>(Configuration.GetSection("CoreDbSettings"));

            // Setup the PostgreSQL DB cone
            var coreDbSettings = Configuration.GetSection("CoreDbSettings").Get<CoreDbSettings>();
            services.AddDbContext<CoreDbContext>(options =>
            {
                options.UseNpgsql(coreDbSettings.ConnectionString);
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Setup the ArangoDB Config DB data in the ConfigDbContext
            var configDbSettings = Configuration.GetSection("ConfigDbSettings").Get<ConfigDbSettings>();
            var configDbContext = new ConfigDbContext()
                .SetConfigDbSettings(configDbSettings)
                .ConfigureConnection()
                .LoadConfigDataFromDb();

            app.UseMvc();
        }

        //private static void AddTestData(ApiDbContext context)
        //{
        //    context.Users.Add(new DbUser()
        //    {
        //        Id = 17,
        //        FirstName = "Luke",
        //        LastName = "Skywalker"
        //    });

        //    context.Users.Add(new DbUser()
        //    {
        //        Id = 18,
        //        FirstName = "Han",
        //        LastName = "Solo"
        //    });

        //    context.SaveChanges();
        //}
    }
}
