using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.GLDPM
{
    public class Timeserie
    {
        public string seriesId { get; set; }
        public string timeSeriesType { get; set; }
        public string ean18Code { get; set; }
        public string powerSystemType { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public List<Decimal> volumes { get; set; }
    }
}
