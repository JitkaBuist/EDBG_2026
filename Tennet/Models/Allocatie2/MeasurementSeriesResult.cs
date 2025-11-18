using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MeasurementSeriesResult
/// </summary>
namespace Tennet.Models.Allocatie2
{
    public class MeasurementSeriesResult
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
        public Detail[] details { get; set; }
        public Header header { get; set; }
        public Acknowledgement[] acknowledgements { get; set; }
    }

    public class Detail
    {
        public Resolution resolution { get; set; }
        public string productId { get; set; }
        public string measureUnit { get; set; }
        public string flowDirection { get; set; }
        public Point[] points { get; set; }
    }

    

    public class Point
    {
        public int position { get; set; }
        public decimal quantity { get; set; }
        public string origin { get; set; }
        public string validationStatus { get; set; }
    }
}