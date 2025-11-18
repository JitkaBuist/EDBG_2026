using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.GLDPM
{
    public class TimePeriod
    {
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public Rejection[] rejections { get; set; }
    }
}
