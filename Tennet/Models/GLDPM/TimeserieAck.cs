using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.GLDPM
{
    public class TimeserieAck
    {
        public string seriesId { get; set; }
        public string version { get; set; }
        public TimePeriod[] periods { get; set; }
        public Rejection[] rejections { get; set; }
    }
}
