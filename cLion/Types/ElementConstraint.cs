using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cLion.Types
{
    public class ElementConstraint
    {
        public bool X { get; private set; }
        public bool Y { get; private set; }
        public bool Z { get; private set; }
        public bool XX { get; private set; }
        public bool YY { get; private set; }
        public bool ZZ { get; private set; }

        public ElementConstraint(bool x, bool y, bool z, bool xx, bool yy, bool zz)
        {
            X = x;
            Y = y;
            Z = z;
            XX = xx;
            YY = yy;
            ZZ = zz;
        }
    }
}
