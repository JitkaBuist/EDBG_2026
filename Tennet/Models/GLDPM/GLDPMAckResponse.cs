using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.GLDPM
{
    //class GLDPMAckResponse
    //{
    //}

    //public class Rootobject
    //{
    //    public Class1[] Property1 { get; set; }
    //}

    public class GLDPMAckResponse
    {
        public DateTime creationTime { get; set; }
        public string correlationId { get; set; }
        public string responsableParty { get; set; }
        public string _operator { get; set; }
        public string technicalMessageId { get; set; }
        public Respons[] responses { get; set; }
    }

    public class Respons
    {
        public DateTime creationTime { get; set; }
        public string transmissionId { get; set; }
        public DateTime receivedDateTime { get; set; }
        public int revisionNumber { get; set; }
        public Rejection[] rejections { get; set; }
        public TimeserieAck[] timeSeries { get; set; }
        public TimePeriod[] timePeriods { get; set; }
    }

    
    

    //public class Period
    //{
    //    public DateTime startDateTime { get; set; }
    //    public DateTime endDateTime { get; set; }
    //    public Rejection[] rejections { get; set; }
    //}

    

    //public class Timeperiod
    //{
    //    public DateTime startDateTime { get; set; }
    //    public DateTime endDateTime { get; set; }
    //    public Rejection[] rejections { get; set; }
    //}

}
