using IoTMonitoring.Application.DTOs;
using IoTMonitoring.Application.Interfaces;
using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.Services
{
    public class DataIngestionService : IDataIngestionService
    {
        public async Task<(List<SensorReading> ValidReadings, ProcessingReport Report)> IngestFileAsync(string filePath)
        {
            var validReadings = new List<SensorReading>();
            var report = new ProcessingReport();

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            using var streamReader = new StreamReader(filePath);
            string? line;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                report.TotalLinesRead++;

                try
                {
                    // Deserialize into a nullable DTO to safely evaluate missing or structurally valid but semantically dirty fields
                    var dto = JsonSerializer.Deserialize<SensorReadingDto>(line, jsonOptions);

                    if (dto == null ||
                        string.IsNullOrWhiteSpace(dto.DeviceId) ||
                        string.IsNullOrWhiteSpace(dto.Metric) ||
                        !dto.Value.HasValue ||
                        !dto.Ts.HasValue ||
                        !dto.Seq.HasValue)
                    {
                        report.InvalidRecords++;
                        continue;
                    }

                    validReadings.Add(new SensorReading
                    {
                        DeviceId = dto.DeviceId,
                        Metric = dto.Metric,
                        Ts = dto.Ts.Value,
                        Value = dto.Value.Value,
                        Seq = dto.Seq.Value
                    });

                    report.ParsedReadings++;
                }
                catch (JsonException)
                {
                    report.InvalidRecords++;
                }
            }

            return (validReadings, report);
        }
    }
}
