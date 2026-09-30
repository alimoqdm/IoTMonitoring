using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.Entities
{
    public class RuleDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Null indicates the rule applies universally to all devices carrying the specified metric.
        public string? DeviceId { get; set; }
        public string Metric { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public decimal? Threshold { get; set; }
        public decimal? MinThreshold { get; set; }
        public decimal? MaxThreshold { get; set; }
        public int? DurationSeconds { get; set; }
        public bool Enabled { get; set; }
    }
}
