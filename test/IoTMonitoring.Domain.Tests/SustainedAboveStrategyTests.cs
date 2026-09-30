using System;
using System.Collections.Generic;
using Xunit;
using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.RuleEngine;
using IoTMonitoring.Domain.RuleEngine.Strategies;
namespace IoTMonitoring.Domain.Tests
{
    public class SustainedAboveStrategyTests
    {
        [Fact]
        public void Evaluate_WhenValueSustainedAboveThreshold_ShouldGenerateAlert()
        {
            // Arrange
            var strategy = new SustainedAboveStrategy();
            var rule = new RuleDefinition
            {
                Id = "rule-001",
                Threshold = 80,
                DurationSeconds = 30
            };

            var baseTs = new DateTime(2025, 06, 01, 8, 0, 0, DateTimeKind.Utc);

            var readings = new List<SensorReading>
            {
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs, Value = 85 },                   // لحظه 0: شروع تخطی
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs.AddSeconds(15), Value = 82 },    // 15 ثانیه بعد: همچنان متخلف
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs.AddSeconds(30), Value = 81 },    // 30 ثانیه بعد: تخطی تکمیل شد! (باید Alert بدهد)
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs.AddSeconds(40), Value = 75 }     // 40 ثانیه بعد: بازگشت به حالت عادی (پایان Alert)
            };

            // Act
            var result = strategy.Evaluate(readings, rule);

            // Assert

            // Ensures exactly one alert is generated for the continuous episode (prevents duplicate alert bugs)
            Assert.Single(result.Alerts); 
            Assert.Equal("rule-001", result.Alerts[0].RuleId);
            Assert.Equal(baseTs, result.Alerts[0].StartTs); 

            // Validates that the alert's end time is locked to the last invalid reading (T+30s), ignoring the valid T+40s reading
            Assert.Equal(baseTs.AddSeconds(30), result.Alerts[0].EndTs); 
        }
    }
}
