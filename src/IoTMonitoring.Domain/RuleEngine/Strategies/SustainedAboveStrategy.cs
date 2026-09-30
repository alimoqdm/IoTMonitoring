using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.RuleEngine.Strategies
{
    public class SustainedAboveStrategy : IRuleOperatorStrategy
    {
        public string OperatorName => "SustainedAbove";

        public RuleEvaluationResult Evaluate(IReadOnlyList<SensorReading> sortedReadings, RuleDefinition rule)
        {
            var result = new RuleEvaluationResult();

            if (!rule.Threshold.HasValue || !rule.DurationSeconds.HasValue || !sortedReadings.Any())
                return result;

            DateTime? episodeStartTs = null;
            bool isAlertActive = false;
            Alert? currentAlert = null;

            foreach (var reading in sortedReadings)
            {
                if (reading.Value > rule.Threshold.Value)
                {
                    // Marks the exact event-time when the metric first crossed the threshold
                    episodeStartTs ??= reading.Ts;

                    // Calculates the ongoing duration of the current violation episode
                    double sustainedDuration = (reading.Ts - episodeStartTs.Value).TotalSeconds;

                    if (!isAlertActive && sustainedDuration >= rule.DurationSeconds.Value)
                    {
                        // Sustained condition met. Initialize the alert. EndTs will be expanded if the episode continues.
                        currentAlert = new Alert
                        {
                            RuleId = rule.Id,
                            DeviceId = reading.DeviceId,
                            Metric = reading.Metric,
                            StartTs = episodeStartTs.Value,
                            EndTs = reading.Ts
                        };
                        isAlertActive = true;
                    }
                    else if (isAlertActive && currentAlert != null)
                    {
                        // The episode is still ongoing. Extend the alert's end timestamp to the latest valid reading.
                        currentAlert.EndTs = reading.Ts;
                    }
                }
                else
                {
                    // The metric dropped back to normal. Close the active alert and flush it to the result.
                    if (isAlertActive && currentAlert != null)
                    {
                        result.Alerts.Add(currentAlert);
                    }

                    // Reset the state to prepare for future potential episodes
                    episodeStartTs = null;
                    isAlertActive = false;
                    currentAlert = null;
                }
            }

            // Flush any ongoing alert if the data stream ends while the violation is still active
            if (isAlertActive && currentAlert != null)
            {
                result.Alerts.Add(currentAlert);
            }


            return result;
        }
    }
}
