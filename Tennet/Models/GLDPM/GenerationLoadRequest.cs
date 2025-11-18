using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.GLDPM
{
    public class GenerationLoadRequest
    {
        public string responsableParty { get; set; }
        public string _operator { get; set; }
        public string transmissionId { get; set; }
        public string forecastType { get; set; }
        public List<Timeserie> timeSeries { get; set; }
    }

    

}
