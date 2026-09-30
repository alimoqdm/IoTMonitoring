using IoTMonitoring.Domain.Repositories;
using IoTMonitoring.Infrastructure.Data;
using IoTMonitoring.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<IoTDbContext>(options =>
                options.UseSqlite(connectionString));

             services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
             services.AddScoped<IAlertRepository, AlertRepository>();

            return services;
        }
    }
}
