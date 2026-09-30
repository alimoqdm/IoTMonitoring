using IoTMonitoring.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Domain.Repositories
{
    public interface IAlertRepository
    {
        Task SaveAlertsIdempotentAsync(List<Alert> alerts);
    }
}
