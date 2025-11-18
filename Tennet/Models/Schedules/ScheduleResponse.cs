using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.Schedules
{
    public class ScheduleResponse
    {
        public DateTime creationTime { get; set; }
        public string technicalMessageId { get; set; }
        public string correlationId { get; set; }
        public int revisionNumber { get; set; }
        public string responsableParty { get; set; }
        public string transmissionId { get; set; }
        public string documentType { get; set; }
        public DateTime scheduleStartDateTime { get; set; }
        public DateTime scheduleEndDateTime { get; set; }
        public string domain { get; set; }
        public Timesery[] timeSeries { get; set; }
    }

    public class Timesery
    {
        public string timeSeriesUniqueId { get; set; }
        public string inDomain { get; set; }
        public string outDomain { get; set; }
        public string inParticipant { get; set; }
        public string outParticipant { get; set; }
        public string marketAgreementId { get; set; }
        public int version { get; set; }
        public Period[] periods { get; set; }
    }
    //public class Point
    //{
    //    public int position { get; set; }
    //    public int quantity { get; set; }
    //}

}
