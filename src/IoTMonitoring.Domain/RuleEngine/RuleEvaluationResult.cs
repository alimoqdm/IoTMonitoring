using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.RuleEngine
{
    public class RuleEvaluationResult
    {
        public List<ReadingViolation> Violations { get; } = new();
        public List<Alert> Alerts { get; } = new();
    }
}
