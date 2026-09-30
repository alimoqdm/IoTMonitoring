using IoTMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Infrastructure.Data.Configurations
{
    public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
    {
        public void Configure(EntityTypeBuilder<SensorReading> builder)
        {
            builder.HasKey(x => x.Id); 

            // Idempotency Requirement
            builder.HasIndex(x => new { x.DeviceId, x.Metric, x.Ts, x.Seq })
                   .IsUnique()
                   .HasDatabaseName("IX_Unique_SensorReading");
        }
    }
}
