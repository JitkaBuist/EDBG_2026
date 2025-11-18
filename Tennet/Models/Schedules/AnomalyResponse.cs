using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.Schedules
{
   
    public class AnomalyResponse
    {
        public Messageidentity messageIdentity { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime receivedDateTime { get; set; }
        public string transmissionId { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public string domainId { get; set; }
        public List<Marketdocument> marketDocuments { get; set; }
    }

    public class Marketdocument
    {
        public string marketDocumentId { get; set; }
        public int revisionNumber { get; set; }
        public string seriesId { get; set; }
        public string version { get; set; }
        public string inDomain { get; set; }
        public string inParticipant { get; set; }
        public string outDomain { get; set; }
        public string outParticipant { get; set; }
        public string marketAgreementID { get; set; }
        public List<Period> periods { get; set; }
        public List<Rejection> rejections { get; set; }
    }

    //public class Period
    //{
    //    public DateTime startDateTime { get; set; }
    //    public DateTime endDateTime { get; set; }
    //    public string resolution { get; set; }
    //    public Point[] points { get; set; }
    //}

    //public class Point
    //{
    //    public int position { get; set; }
    //    public int quantity { get; set; }
    //}

    //public class Rejection
    //{
    //    public string code { get; set; }
    //    public string text { get; set; }
    //}

}
