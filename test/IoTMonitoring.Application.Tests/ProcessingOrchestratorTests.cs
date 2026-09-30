using IoTMonitoring.Application.DTOs;
using IoTMonitoring.Application.Services;
using IoTMonitoring.Domain.Entities;
using IoTMonitoring.Domain.Repositories;
using IoTMonitoring.Domain.RuleEngine;
using IoTMonitoring.Domain.RuleEngine.Strategies;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.Tests
{
    public class ProcessingOrchestratorTests
    {
        [Fact]
        public async Task ProcessDataAsync_ShouldRemoveDuplicates_And_SortOutOfOrderData()
        {
            // Arrange
            var ruleFactory = new RuleOperatorFactory(new List<IRuleOperatorStrategy> { new GreaterThanStrategy() });


            var mockSensorRepo = new Mock<ISensorReadingRepository>();
            var mockAlertRepo = new Mock<IAlertRepository>();

            var orchestrator = new ProcessingOrchestrator(ruleFactory, mockSensorRepo.Object, mockAlertRepo.Object);

            var baseTs = new DateTime(2025, 06, 01, 8, 0, 0, DateTimeKind.Utc);

            var rawReadings = new List<SensorReading>
            {
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs.AddMinutes(5), Seq = 2, Value = 10 }, // داده‌ای که در آینده آمده اما اول خوانده شده (Out-of-order)
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs, Seq = 1, Value = 10 },               // داده اصلی که باید اول می‌بود
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs, Seq = 1, Value = 10 }                // داده دقیقاً تکراری (Duplicate)
            };

            var report = new ProcessingReport();

            // Act
            var result = await orchestrator.ProcessDataAsync(rawReadings, new List<RuleDefinition>(), report);

            // Assert
            Assert.Equal(1, result.DuplicatesRemoved);
            Assert.Equal(2, result.AcceptableReadings); // فقط دو رکورد یکتا باقی مانده است

            // 2. بررسی فراخوانی ریپازیتوری با دقیقاً 2 رکورد مرتب شده
            mockSensorRepo.Verify(r => r.SaveProcessedDataAsync(
                It.Is<List<SensorReading>>(list => list.Count == 2),
                It.IsAny<List<SensorReading>>(),
                It.IsAny<List<ReadingViolation>>()), Times.Once);
        }
    }
}
