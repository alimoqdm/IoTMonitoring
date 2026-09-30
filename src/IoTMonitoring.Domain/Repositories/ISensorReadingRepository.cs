using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.Repositories
{
    public interface ISensorReadingRepository
    {
        Task SaveProcessedDataAsync(
            List<SensorReading> acceptableReadings,
            List<SensorReading> unacceptableReadings,
            List<ReadingViolation> violations);

        Task<List<SensorReading>> GetAcceptableReadingsAsync(string deviceId, string metric, DateTime from, DateTime to);
    }
}
