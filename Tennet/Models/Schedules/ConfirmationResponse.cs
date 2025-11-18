using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennet.Models.Generic;

namespace Tennet.Models.Schedules
{
    public class ConfirmationResponse
    {
        public string technicalMessageId { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime receivedDateTime { get; set; }
        public string transmissionId { get; set; }
        public int revisionNumber { get; set; }
        public List<Rejection> rejections { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<Imposedtimesery> imposedTimeSeries { get; set; }
        public List<Confirmedtimesery> confirmedTimeSeries { get; set; }
    }

    //public class Rejection
    //{
    //    public string code { get; set; }
    //    public string text { get; set; }
    //}

    public class Imposedtimesery
    {
        public string businessType { get; set; }
        public string seriesId { get; set; }
        public string version { get; set; }
        public string inDomain { get; set; }
        public string inParticipant { get; set; }
        public string outDomain { get; set; }
        public string outParticipant { get; set; }
        public string marketAgreementType { get; set; }
        public string marketAgreementID { get; set; }
        public List<Rejection> rejections { get; set; }
    }
   
    public class Confirmedtimesery
    {
        public string businessType { get; set; }
        public string seriesId { get; set; }
        public string version { get; set; }
        public string inDomain { get; set; }
        public string inParticipant { get; set; }
        public string outDomain { get; set; }
        public string outParticipant { get; set; }
        public string marketAgreementType { get; set; }
        public string marketAgreementID { get; set; }
        public List<Rejection> rejections { get; set; }
    }
    
}
