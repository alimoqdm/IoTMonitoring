using IoTMonitoring.Application.DTOs;
using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.Interfaces
{
    public interface IProcessingOrchestrator
    {
        Task<ProcessingReport> ProcessDataAsync(List<SensorReading> rawReadings, List<RuleDefinition> rules, ProcessingReport report);
    }
}
