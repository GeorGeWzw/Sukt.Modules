﻿using Sukt.AuthServer.Demo.Startups;
using Sukt.Module.Core.Modules;

namespace Sukt.AuthServer.Demo
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
            //services.AddControllers();
            //services.AddAppModuleManager<SuktAspNetCoreAppModuleManager>();
            services.AddApplication<SuktAppWebModule>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseMultiTenancy();
            app.InitializeApplication();
        }
    }
}
