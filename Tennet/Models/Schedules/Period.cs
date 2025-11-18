using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.Schedules
{
    public class Period
    {
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public List<Point> points { get; set; }
    }
}
