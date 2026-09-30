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
                    // اگر تازه وارد فاز تخطی شده‌ایم
                    episodeStartTs ??= reading.Ts;

                    // بررسی می‌کنیم که آیا زمان کافی از شروع تخطی گذشته است تا آلرت تولید شود؟
                    double sustainedDuration = (reading.Ts - episodeStartTs.Value).TotalSeconds;

                    if (!isAlertActive && sustainedDuration >= rule.DurationSeconds.Value)
                    {
                        // ایجاد آلرت اولیه (EndTs را موقتاً زمان فعلی در نظر می‌گیریم تا در ادامه آپدیت شود)
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
                        // در حین تداوم تخطی، EndTs هشدار را با آخرین رکورد معتبر آپدیت می‌کنیم
                        currentAlert.EndTs = reading.Ts;
                    }
                }
                else
                {
                    // اگر مقدار به زیر آستانه برگشت، اپیزود تمام می‌شود
                    if (isAlertActive && currentAlert != null)
                    {
                        result.Alerts.Add(currentAlert);
                    }

                    // ریست کردن State برای اپیزودهای بعدی
                    episodeStartTs = null;
                    isAlertActive = false;
                    currentAlert = null;
                }
            }

            // در صورتی که دیتای مشاهده شده به پایان رسید اما آلرت هنوز در جریان است
            if (isAlertActive && currentAlert != null)
            {
                result.Alerts.Add(currentAlert);
            }

            // نکته: طبق داکیومنت، SustainedAbove باعث Unacceptable شدن خود رکوردها نمی‌شود، بلکه Alert تولید می‌کند.
            // به همین دلیل لیست Violations در اینجا خالی می‌ماند.

            return result;
        }
    }
}
