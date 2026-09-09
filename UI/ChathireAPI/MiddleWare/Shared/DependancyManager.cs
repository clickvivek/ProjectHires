using BusinessLayer.Common;
using DataAccessLayer.Common;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Shared
{
    public class DependancyManager
    {
        public static void ConfigureAPI(IServiceCollection _services)
        {
         
            _services.AddDbContext<EFContexts>(options =>
                options.UseSqlServer("Server=ones.database.windows.net,1433;Initial Catalog=HIRES;Persist Security Info=False;User ID=vijisrk;Password=Thayasrk22@@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            //place holder for DI configuration
            _services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            _services.AddScoped<IManagerFactory, ManagerFactory>();
        }
    }
}
