using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.Entities
{
    public class SensorReading
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DeviceId { get; set; } = string.Empty;
        public string Metric { get; set; } = string.Empty;
        public DateTime Ts { get; set; }
        public decimal Value { get; set; }
        public int Seq { get; set; }

        public bool IsAcceptable { get; set; }
    }
}
