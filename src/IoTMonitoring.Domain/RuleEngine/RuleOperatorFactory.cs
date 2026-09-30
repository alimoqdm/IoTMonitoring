using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.RuleEngine
{
    public class RuleOperatorFactory
    {
        private readonly Dictionary<string, IRuleOperatorStrategy> _strategies;

        // در معماری واقعی این کلاس‌ها از طریق Dependency Injection رجیستر و پاس داده می‌شوند
        public RuleOperatorFactory(IEnumerable<IRuleOperatorStrategy> strategies)
        {
            _strategies = strategies.ToDictionary(s => s.OperatorName, s => s, StringComparer.OrdinalIgnoreCase);
        }

        public IRuleOperatorStrategy GetStrategy(string operatorName)
        {
            if (_strategies.TryGetValue(operatorName, out var strategy))
            {
                return strategy;
            }

            throw new NotSupportedException($"Operator '{operatorName}' is not supported by the Rule Engine.");
        }
    }
}
