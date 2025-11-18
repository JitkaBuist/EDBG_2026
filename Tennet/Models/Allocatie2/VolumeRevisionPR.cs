using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for VolumeRevision
/// </summary>
namespace Tennet.Models.Allocatie2
{
    public class VolumeRevisionPR
    {
        public string product { get; set; }
        public string referenceSeriesId { get; set; }
        public string reason { get; set; }
        public string explanation { get; set; }
        public int marketEvaluationPontId { get; set; }
        public int counterParty { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
    }
}
