using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for VolumeSeriesResult
/// </summary>
namespace Tennet.Models.Allocatie2
{
    public class VolumeSeriesResult
    {
        public string technicalMessageId { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime receivedTime { get; set; }
        public string notificationId { get; set; }
        public string product { get; set; }
        public long ean18_Code { get; set; }
        public long measuringResponsibleParty { get; set; }
        public string domainId { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public Volumequantity[] volumeQuantities { get; set; }
        public object[] maxQuantities { get; set; }
        public object[] weekMaxQuantities { get; set; }
        public object[] registers { get; set; }
        public Header header { get; set; }
        public Acknowledgement[] acknowledgements { get; set; }
    }

    public class Volumequantity
    {
        public decimal quantity { get; set; }
        public string tariffZone { get; set; }
        public string productId { get; set; }
        public string measureUnit { get; set; }
        public string flowDirection { get; set; }
    }

    

    //public class Reason
    //{
    //    public string code { get; set; }
    //}

}