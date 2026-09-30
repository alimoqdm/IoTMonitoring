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
    public class ReadingViolationConfiguration : IEntityTypeConfiguration<ReadingViolation>
    {
        public void Configure(EntityTypeBuilder<ReadingViolation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Reading)
                   .WithMany()
                   .HasForeignKey(x => x.SensorReadingId);
        }
    }
}
