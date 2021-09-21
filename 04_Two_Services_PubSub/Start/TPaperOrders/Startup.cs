using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace TPaperOrders
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
            services.AddLogging(options =>
            {
                options.AddFilter("TPaperOrders", LogLevel.Information);
            });

            services.AddOptions<ProjectOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("ProjectOptions").Bind(settings);
                });

            string sqlPaperString = Environment.GetEnvironmentVariable("SqlPaperString");
            string sqlPassword = Environment.GetEnvironmentVariable("SqlPaperPassword");

            string paperConnectionString = new SqlConnectionStringBuilder(sqlPaperString) { Password = sqlPassword }.ConnectionString;

            services.AddDbContextPool<PaperDbContext>(options =>
            {
                if (!string.IsNullOrEmpty(paperConnectionString))
                {
                    options.UseSqlServer(paperConnectionString, providerOptions => providerOptions.EnableRetryOnFailure());
                }
            });

            PaperDbContext.ExecuteMigrations(paperConnectionString);

            services.AddControllers().AddDapr();
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
