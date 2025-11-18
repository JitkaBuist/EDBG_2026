using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.GLDPM
{
    public class AcknowledgementItem
    {
        public string Operator { get; set; }
        public string CorrelationId { get; set; }
        public string Ean18Code { get; set; }
        public bool? HasRejections { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
