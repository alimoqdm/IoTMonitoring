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
    public class AlertRepository : IAlertRepository
    {
        private readonly IoTDbContext _context;

        public AlertRepository(IoTDbContext context)
        {
            _context = context;
        }

        public async Task SaveAlertsIdempotentAsync(List<Alert> alerts)
        {
            if (!alerts.Any()) return;

            var existingAlerts = await _context.Alerts
                .Select(a => new { a.RuleId, a.DeviceId, a.Metric, a.StartTs })
                .ToListAsync();

            var existingSet = new HashSet<(string, string, string, DateTime)>(
                existingAlerts.Select(a => (a.RuleId, a.DeviceId, a.Metric, a.StartTs))
            );

            var newAlerts = alerts
                .Where(a => !existingSet.Contains((a.RuleId, a.DeviceId, a.Metric, a.StartTs)))
                .ToList();

            if (newAlerts.Any())
            {
                await _context.Alerts.AddRangeAsync(newAlerts);
                await _context.SaveChangesAsync();
            }
        }
    }
}
