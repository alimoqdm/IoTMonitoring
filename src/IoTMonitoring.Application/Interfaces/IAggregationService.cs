using IoTMonitoring.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.Interfaces
{
    public interface IAggregationService
    {
        Task<List<AggregationResult>> AggregateAsync(string deviceId, string metric, DateTime from, DateTime to, int bucketSizeSeconds);
    }
}
