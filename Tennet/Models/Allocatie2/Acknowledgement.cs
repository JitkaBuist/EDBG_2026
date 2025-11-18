using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tennet.Models.Generic;
/// <summary>
/// Summary description for Acknowledgement
/// </summary>
namespace Tennet.Models.Allocatie2
{
    public class Acknowledgement
    {
        public DateTime creationTime { get; set; }
        public Reason[] reasons { get; set; }
    }
}