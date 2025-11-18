using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Header
/// </summary>
namespace Tennet.Models.Allocatie2
{
    public class Header
    {
        public DateTime creationTime { get; set; }
        public string messageType { get; set; }
        public string contentType { get; set; }
        public DateTime receivedTime { get; set; }
        public string correlationId { get; set; }
        public long responsableParty { get; set; }
        public string technicalMessageId { get; set; }
        public long counterParty { get; set; }
    }
}