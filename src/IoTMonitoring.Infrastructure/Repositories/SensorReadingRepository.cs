using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Repositories;
using IoTMonitoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Infrastructure.Repositories
{
    public class SensorReadingRepository : ISensorReadingRepository
    {
        private readonly IoTDbContext _context;

        public SensorReadingRepository(IoTDbContext context)
        {
            _context = context;
        }

        public async Task SaveProcessedDataAsync(
            List<SensorReading> acceptableReadings,
            List<SensorReading> unacceptableReadings,
            List<ReadingViolation> violations)
        {
            var allReadings = acceptableReadings.Concat(unacceptableReadings).ToList();
            if (!allReadings.Any()) return;

            // Defines a query bounding box (time range and involved devices) to drastically narrow down the database scan
            var minTs = allReadings.Min(r => r.Ts);
            var maxTs = allReadings.Max(r => r.Ts);
            var deviceIds = allReadings.Select(r => r.DeviceId).Distinct().ToList();

            var existingKeys = await _context.SensorReadings
                .Where(r => r.Ts >= minTs && r.Ts <= maxTs && deviceIds.Contains(r.DeviceId))
                .Select(r => new { r.DeviceId, r.Metric, r.Ts, r.Seq })
                .ToListAsync();

            // O(1) lookup table for extremely fast duplicate checking in memory
            var existingKeysSet = new HashSet<(string, string, System.DateTime, int)>(
                existingKeys.Select(k => (k.DeviceId, k.Metric, k.Ts, k.Seq))
            );

            var newAcceptable = acceptableReadings
                .Where(r => !existingKeysSet.Contains((r.DeviceId, r.Metric, r.Ts, r.Seq)))
                .ToList();

            var newUnacceptable = unacceptableReadings
                .Where(r => !existingKeysSet.Contains((r.DeviceId, r.Metric, r.Ts, r.Seq)))
                .ToList();

            newAcceptable.ForEach(r => r.IsAcceptable = true);
            newUnacceptable.ForEach(r => r.IsAcceptable = false);

            var newViolations = violations
                .Where(v => !existingKeysSet.Contains((v.Reading.DeviceId, v.Reading.Metric, v.Reading.Ts, v.Reading.Seq)))
                .ToList();

            await _context.SensorReadings.AddRangeAsync(newAcceptable);
            await _context.SensorReadings.AddRangeAsync(newUnacceptable);
            await _context.Violations.AddRangeAsync(newViolations);

            await _context.SaveChangesAsync();
        }


        public async Task<List<SensorReading>> GetAcceptableReadingsAsync(string deviceId, string metric, DateTime from, DateTime to)
        {
            return await _context.SensorReadings
                .AsNoTracking()
                .Where(r => r.IsAcceptable
                         && r.DeviceId == deviceId
                         && r.Metric == metric
                         && r.Ts >= from
                         && r.Ts < to)
                .ToListAsync();
        }
    }
}
