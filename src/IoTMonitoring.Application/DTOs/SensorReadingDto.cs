using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.DTOs
{
    public class SensorReadingDto
    {
        [JsonPropertyName("deviceId")]
        public string? DeviceId { get; set; }

        [JsonPropertyName("metric")]
        public string? Metric { get; set; }

        [JsonPropertyName("ts")]
        public DateTime? Ts { get; set; }

        [JsonPropertyName("value")]
        public decimal? Value { get; set; }

        [JsonPropertyName("seq")]
        public int? Seq { get; set; }
    }
}
