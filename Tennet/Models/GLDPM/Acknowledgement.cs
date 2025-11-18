using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet.Models.GLDPM
{
    public class Acknowledgement
    {
        public DateTime CreationTime { get; set; }
        public DateTime ProcessDate { get; set; }
        public string ResponsibleParty { get; set; }
        public ICollection<AcknowledgementItem> Items { get; set; }
        public bool? Success { get; set; }
        public string Message { get; set; }
    }
}
