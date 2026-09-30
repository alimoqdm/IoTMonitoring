using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.Entities
{
    public class ReadingViolation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SensorReadingId { get; set; }
        public SensorReading Reading { get; set; } = null!;

        public string RuleId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;

        public ReadingViolation(SensorReading reading, string ruleId, string reason)
        {
            Reading = reading;
            SensorReadingId = reading.Id;
            RuleId = ruleId;
            Reason = reason;
        }

        protected ReadingViolation() { }
    }
}
