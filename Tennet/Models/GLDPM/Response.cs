using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.GLDPM
{
    public class Response
    {
        public DateTime creationTime { get; set; }
        public string transmissionId { get; set; }
        public DateTime receivedDateTime { get; set; }
        public int revisionNumber { get; set; }
        public Rejection[] rejections { get; set; }
        public Timeserie[] timeSeries { get; set; }
        public TimePeriod[] timePeriods { get; set; }
    }
}
