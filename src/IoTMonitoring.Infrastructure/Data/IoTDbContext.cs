using IoTMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Infrastructure.Data
{
    public class IoTDbContext : DbContext
    {
        public IoTDbContext(DbContextOptions<IoTDbContext> options) : base(options) { }

        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<ReadingViolation> Violations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
