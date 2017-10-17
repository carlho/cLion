using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwecoTools
{
    public static class Utilities
    {
        
        public static double CastToDouble(string s)
        {
            double d;

                if (!ExcelTools.Utilities.SafeParseToDouble(s, out d))
                    throw new Exception("Attempt of casting string " + s + "to double failed.");

            return d;
        }

    }
}
