using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.GLDPM
{
    //class GetAcknowledgementResponse
    //{
    //}

    //public class Rootobject
    //{
    //    public Class1[] Property1 { get; set; }
    //}

    public class GetAcknowledgementResponse
    {
        public DateTime creationTime { get; set; }
        public string correlationId { get; set; }
        public string responsableParty { get; set; }
        public string _operator { get; set; }
        public string technicalMessageId { get; set; }
        public Response[] responses { get; set; }
    }

    

    //public class Rejection
    //{
    //    public string code { get; set; }
    //    public string text { get; set; }
    //}

    //public class Timeserie
    //{
    //    public string seriesId { get; set; }
    //    public string version { get; set; }
    //    public Period[] periods { get; set; }
    //    public Rejection[] rejections { get; set; }
    //}

    //public class Period
    //{
    //    public DateTime startDateTime { get; set; }
    //    public DateTime endDateTime { get; set; }
    //    public Rejection[] rejections { get; set; }
    //}
}
