using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.Generic
{
    public class Messageidentity
    {
        public string correlationId { get; set; }
        public string responsableParty { get; set; }
        public string technicalMessageId { get; set; }
    }
}
