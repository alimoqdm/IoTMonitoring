using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.RuleEngine.Strategies
{
    public class BetweenStrategy : IRuleOperatorStrategy
    {
        public string OperatorName => "Between";

        public RuleEvaluationResult Evaluate(IReadOnlyList<SensorReading> sortedReadings, RuleDefinition rule)
        {
            var result = new RuleEvaluationResult();
            if (!rule.MinThreshold.HasValue || !rule.MaxThreshold.HasValue) return result;

            foreach (var reading in sortedReadings)
            {
                if (reading.Value < rule.MinThreshold.Value || reading.Value > rule.MaxThreshold.Value)
                {
                    result.Violations.Add(new ReadingViolation(
                        reading,
                        rule.Id,
                        $"Value {reading.Value} is strictly not between {rule.MinThreshold.Value} and {rule.MaxThreshold.Value}"));
                }
            }

            return result;
        }
    }
}
