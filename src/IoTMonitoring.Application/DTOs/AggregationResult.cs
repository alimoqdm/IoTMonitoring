using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.DTOs
{
    public class AggregationResult
    {
        public DateTime BucketStart { get; set; }
        public int Count { get; set; }
        public decimal Average { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }
}
