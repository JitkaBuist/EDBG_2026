using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.Schedules
{
    public class AcknowledgementResponse
    {
        public List<Acknowledgement> acknowledgements { get; set; }
        public List<Failure> failures { get; set; }
    }

    public class Acknowledgement
    {
        public Messageidentity messageIdentity { get; set; }
        public DateTime creationTime { get; set; }
        public string transmissionId { get; set; }
        public DateTime receivedDateTime { get; set; }
        public int revisionNumber { get; set; }
        public List<Rejection> rejections { get; set; }
        public List<TimeserieAck> timeSeries { get; set; }
        public List<Timeperiod> timePeriods { get; set; }
    }

    public class TimeserieAck
    {
        public string seriesId { get; set; }
        public string version { get; set; }
        public List<Period> periods { get; set; }
        public List<Rejection> rejections { get; set; }
    }

    public class Timeperiod
    {
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public List<Rejection> rejections { get; set; }
    }

    public class Failure
    {
        public Messageid messageId { get; set; }
        public string message { get; set; }
    }

    public class Messageid
    {
        public string correlationId { get; set; }
        public string responsableParty { get; set; }
        public string technicalMessageId { get; set; }
    }

}
