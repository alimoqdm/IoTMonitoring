using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMonitoring.Application.DTOs
{
    public class ProcessingReport
    {
        public int TotalLinesRead { get; set; }
        public int ParsedReadings { get; set; }
        public int InvalidRecords { get; set; }
        public int DuplicatesRemoved { get; set; }
        public int AcceptableReadings { get; set; }
        public int UnacceptableReadings { get; set; }
        public int AlertsGenerated { get; set; }
    }
}
