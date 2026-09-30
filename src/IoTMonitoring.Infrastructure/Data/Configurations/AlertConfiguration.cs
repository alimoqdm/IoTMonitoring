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
    public class AlertConfiguration : IEntityTypeConfiguration<Alert>
    {
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            builder.HasKey(x => x.Id);

            // جلوگیری از ثبت هشدار تکراری برای یک رویداد یکسان
            builder.HasIndex(x => new { x.RuleId, x.DeviceId, x.Metric, x.StartTs })
                   .IsUnique()
                   .HasDatabaseName("IX_Unique_Alert");
        }
    }
}
