using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.Schedules
{
    public class AcknowledgementRequest
    {
        public string responsableParty { get; set; }
        public string correlationId { get; set; }
    }

}
