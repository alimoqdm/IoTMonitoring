using IoTMonitoring.Application.DTOs;
using IoTMonitoring.Application.Interfaces;
using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Repositories;
using IoTMonitoring.Domain.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.Services
{
    public class ProcessingOrchestrator : IProcessingOrchestrator
    {
        private readonly RuleOperatorFactory _ruleFactory;
        private readonly ISensorReadingRepository _sensorRepository;
        private readonly IAlertRepository _alertRepository;

        // تزریق ریپازیتوری‌ها
        public ProcessingOrchestrator(
            RuleOperatorFactory ruleFactory,
            ISensorReadingRepository sensorRepository,
            IAlertRepository alertRepository)
        {
            _ruleFactory = ruleFactory;
            _sensorRepository = sensorRepository;
            _alertRepository = alertRepository;
        }

        public async Task<ProcessingReport> ProcessDataAsync(List<SensorReading> rawReadings, List<RuleDefinition> rules, ProcessingReport report)
        {
            var uniqueReadings = new List<SensorReading>();
            var seenKeys = new HashSet<(string, string, DateTime, int)>();

            foreach (var reading in rawReadings)
            {
                if (seenKeys.Add((reading.DeviceId, reading.Metric, reading.Ts, reading.Seq)))
                {
                    uniqueReadings.Add(reading);
                }
                else
                {
                    report.DuplicatesRemoved++;
                }
            }

            var acceptableReadings = new List<SensorReading>();
            var unacceptableReadings = new List<SensorReading>();
            var generatedAlerts = new List<Alert>();
            var allViolations = new List<ReadingViolation>();

            var lastAlertEndTimes = new Dictionary<string, DateTime>();

            var groupedReadings = uniqueReadings
                .GroupBy(r => new { r.DeviceId, r.Metric })
                .Select(g => g.OrderBy(r => r.Ts).ToList())
                .ToList();

            foreach (var group in groupedReadings)
            {
                var deviceId = group.First().DeviceId;
                var metric = group.First().Metric;

                var applicableRules = rules.Where(r =>
                    r.Metric == metric &&
                    (string.IsNullOrEmpty(r.DeviceId) || r.DeviceId == deviceId) &&
                    r.Enabled).ToList();

                var groupViolationsReadings = new HashSet<SensorReading>();
                var groupAlerts = new List<Alert>();

                foreach (var rule in applicableRules)
                {
                    var strategy = _ruleFactory.GetStrategy(rule.Operator);
                    var result = strategy.Evaluate(group, rule);

                    foreach (var violation in result.Violations)
                    {
                        groupViolationsReadings.Add(violation.Reading);
                        allViolations.Add(violation);
                    }

                    foreach (var alert in result.Alerts.OrderBy(a => a.StartTs))
                    {
                        string cooldownKey = $"{alert.RuleId}_{alert.DeviceId}_{alert.Metric}";

                        if (lastAlertEndTimes.TryGetValue(cooldownKey, out var lastEndTs))
                        {
                            if ((alert.StartTs - lastEndTs).TotalMinutes < 5)
                            {
                                continue;
                            }
                        }

                        groupAlerts.Add(alert);
                        lastAlertEndTimes[cooldownKey] = alert.EndTs;
                    }
                }

                foreach (var reading in group)
                {
                    if (groupViolationsReadings.Contains(reading))
                    {
                        unacceptableReadings.Add(reading);
                    }
                    else
                    {
                        acceptableReadings.Add(reading);
                    }
                }

                generatedAlerts.AddRange(groupAlerts);
            }

            report.AcceptableReadings = acceptableReadings.Count;
            report.UnacceptableReadings = unacceptableReadings.Count;
            report.AlertsGenerated = generatedAlerts.Count;

            // فراخوانی ریپازیتوری‌ها برای ذخیره داده‌ها به صورت امن (Idempotent)
            await _sensorRepository.SaveProcessedDataAsync(acceptableReadings, unacceptableReadings, allViolations);
            await _alertRepository.SaveAlertsIdempotentAsync(generatedAlerts);

            return report;
        }
    }
}
