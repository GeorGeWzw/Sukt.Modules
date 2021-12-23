﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sukt.Module.Core.Extensions;
using Sukt.Module.Core.Modules;
using Sukt.Module.Core.SeedDatas;
using Sukt.Module.Core.UnitOfWorks;
using System.Linq;

namespace Sukt.AuthServer.DemoApi.Startups
{
    public class MigrationModuleBase : SuktAppModule
    {
        public override void ApplicationInitialization(ApplicationContext context)
        {
            var app = context.GetApplicationBuilder();
            var configuration = context.ServiceProvider.GetService<IConfiguration>();
            var isAutoMigration = configuration["SuktCore:Migrations:IsAutoMigration"].AsTo<bool>();
            if (isAutoMigration)
            {
                context.ServiceProvider.CreateScoped(provider =>
                {
                    var unitOfWork = provider.GetService<IUnitOfWork>();
                    var dbContext = unitOfWork.GetDbContext();
#if DEBUG
                    dbContext.Database.EnsureCreated();
#else
                    string[] migrations = dbContext.Database.GetPendingMigrations().ToArray();
                    if (migrations.Length > 0)
                    {
                        dbContext.Database.Migrate();
                    }
#endif
                });
            }
            var isAddSeedData = configuration["SuktCore:Migrations:IsAddSeedData"].AsTo<bool>();
            if (isAddSeedData)
            {
                var seedDatas = context.ServiceProvider.GetServices<ISeedData>();

                foreach (var seed in seedDatas?.OrderBy(o => o.Order).Where(o => !o.Disable))
                {
                    seed.Initialize();
                }
            }
        }
    }
}
