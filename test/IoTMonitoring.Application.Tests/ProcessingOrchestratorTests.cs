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
                // Read first, but its time is in the future (Out-of-order)
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs.AddMinutes(5), Seq = 2, Value = 10 }, 

                // Read second, but this is the actual starting time
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs, Seq = 1, Value = 10 },    
                
                // Exact duplicate of the second reading
                new SensorReading { DeviceId = "P1", Metric = "temp", Ts = baseTs, Seq = 1, Value = 10 }              
            };

            var report = new ProcessingReport();

            // Act
            var result = await orchestrator.ProcessDataAsync(rawReadings, new List<RuleDefinition>(), report);

            // Assert

            // Check if the duplicate was successfully removed
            Assert.Equal(1, result.DuplicatesRemoved);

            // Check if exactly 2 unique readings are left
            Assert.Equal(2, result.AcceptableReadings);

            // Check if the save method was called exactly once with the 2 cleaned items
            mockSensorRepo.Verify(r => r.SaveProcessedDataAsync(
                It.Is<List<SensorReading>>(list => list.Count == 2),
                It.IsAny<List<SensorReading>>(),
                It.IsAny<List<ReadingViolation>>()), Times.Once);
        }
    }
}
