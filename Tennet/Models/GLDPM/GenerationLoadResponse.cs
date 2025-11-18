using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.GLDPM
{
    //class GenerationLoadResponse
    //{
    //}

    //public class Rootobject
    //{
    //    public Class1[] Property1 { get; set; }
    //}

    public class GenerationLoadResponse
    {
        public DateTime creationTime { get; set; }
        public string technicalMessageId { get; set; }
        public string correlationId { get; set; }
        public int revisionNumber { get; set; }
        public string responsableParty { get; set; }
        public string _operator { get; set; }
        public string transmissionId { get; set; }
        public string forecastType { get; set; }
        public Timeserie[] timeSeries { get; set; }
    }

}
