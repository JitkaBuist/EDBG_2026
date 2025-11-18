using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennet
{
    public static class Tools
    {
        public static string GenerateTimeSeriesId(string consumer, string counterParty, int side, DateTime date)
        {
            // NB: Max 35, might take part of consumer instead of all
            date = date.ToLocalTime().Date;
            var sidebit = (side > 0) ? 1 : 0;
            return $"{consumer}{counterParty}{sidebit}{date:yyyyMMdd}";
        }
    }
}
