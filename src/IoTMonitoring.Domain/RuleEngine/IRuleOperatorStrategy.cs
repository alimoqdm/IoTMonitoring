using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.RuleEngine
{
    public interface IRuleOperatorStrategy
    {
        string OperatorName { get; }

        // داده‌های ورودی حتماً باید بر اساس (DeviceId, Metric) گروه‌بندی شده و بر اساس زمان (Ts) مرتب شده باشند
        RuleEvaluationResult Evaluate(IReadOnlyList<SensorReading> sortedReadings, RuleDefinition rule);
    }
}
