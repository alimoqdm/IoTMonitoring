using IoTMonitoring.Application.DTOs;
using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.Interfaces
{
    public interface IDataIngestionService
    {
        Task<(List<SensorReading> ValidReadings, ProcessingReport Report)> IngestFileAsync(string filePath);
    }
}
