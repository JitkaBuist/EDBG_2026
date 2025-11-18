using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.Schedules
{
    public class ScheduleRequest
    {
        public string responsableParty { get; set; }
        public string transmissionId { get; set; }
        public string documentType { get; set; }
        public DateTime scheduleStartDateTime { get; set; }
        public DateTime scheduleEndDateTime { get; set; }
        public string domain { get; set; }
        public List<Timeserie> timeSeries { get; set; }
    }

    public class Timeserie
    {
        public string timeSeriesUniqueId { get; set; }
        public string inDomain { get; set; }
        public string outDomain { get; set; }
        public string inParticipant { get; set; }
        public string outParticipant { get; set; }
        public string marketAgreementId { get; set; }
        public int version { get; set; }
        public List<Period> periods { get; set; }
    }
}
