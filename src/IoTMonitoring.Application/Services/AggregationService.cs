using IoTMonitoring.Application.DTOs;
using IoTMonitoring.Application.Interfaces;
using IoTMonitoring.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly ISensorReadingRepository _repository;

        public AggregationService(ISensorReadingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AggregationResult>> AggregateAsync(string deviceId, string metric, DateTime from, DateTime to, int bucketSizeSeconds)
        {
            var readings = await _repository.GetAcceptableReadingsAsync(deviceId, metric, from, to);

            if (!readings.Any())
                return new List<AggregationResult>();

            long ticksPerBucket = TimeSpan.FromSeconds(bucketSizeSeconds).Ticks;

            var aggregatedData = readings
                .GroupBy(r => new DateTime((r.Ts.Ticks / ticksPerBucket) * ticksPerBucket, DateTimeKind.Utc))
                .Select(g => new AggregationResult
                {
                    BucketStart = g.Key,
                    Count = g.Count(),
                    Average = Math.Round(g.Average(r => r.Value), 2),
                    Min = g.Min(r => r.Value),
                    Max = g.Max(r => r.Value)
                })
                .OrderBy(x => x.BucketStart)
                .ToList();

            return aggregatedData;
        }
    }
}
