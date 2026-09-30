using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.RuleEngine.Strategies
{
    public class GreaterThanStrategy : IRuleOperatorStrategy
    {
        public string OperatorName => "GreaterThan";

        public RuleEvaluationResult Evaluate(IReadOnlyList<SensorReading> sortedReadings, RuleDefinition rule)
        {
            var result = new RuleEvaluationResult();

            if (!rule.Threshold.HasValue)
                return result;

            foreach (var reading in sortedReadings)
            {
                if (reading.Value > rule.Threshold.Value)
                {
                    result.Violations.Add(new ReadingViolation(
                        reading,
                        rule.Id,
                        $"Value {reading.Value} is greater than threshold {rule.Threshold.Value}"));
                }
            }

            return result;
        }
    }
}
