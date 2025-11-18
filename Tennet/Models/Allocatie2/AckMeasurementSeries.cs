using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.Allocatie2
{
    public class AckMeasurementSeries
    {
        public DateTime creationTime { get; set; }
        public List<Reason> reasons { get; set; }
    }

}
