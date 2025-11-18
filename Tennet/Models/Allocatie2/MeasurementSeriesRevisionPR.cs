using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MeasurementSeriesRevisionPR
/// </summary>
namespace Tennet.Models.Allocatie2
{
    public class MeasurementSeriesRevisionPR
    {
        public string product { get; set; }
        public string referenceSeriesId { get; set; }
        public string reason { get; set; }
        public string explanation { get; set; }
        public int marketEvaluationPontId { get; set; }
        public int counterParty { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public Series[] series { get; set; }
    }

    public class Series
    {
        public Resolution resolution { get; set; }
        public string productId { get; set; }
        public string measureUnit { get; set; }
        public string flowDirection { get; set; }
        public Originalpoint[] originalPoints { get; set; }
        public Proposedpoint[] proposedPoints { get; set; }
    }


    public class Originalpoint
    {
        public int position { get; set; }
        public int quantity { get; set; }
    }

    public class Proposedpoint
    {
        public int position { get; set; }
        public int quantity { get; set; }
    }

}