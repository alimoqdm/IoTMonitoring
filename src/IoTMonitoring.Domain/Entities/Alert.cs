using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.Entities
{
    public class Alert
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RuleId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Metric { get; set; } = string.Empty;
        public DateTime StartTs { get; set; }
        public DateTime EndTs { get; set; }
    }
}
