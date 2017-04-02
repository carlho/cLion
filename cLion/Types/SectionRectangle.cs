using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cLion.Types
{
    public class SectionRectangle : Section
    {
        public double B { get; private set; }
        public double H { get; private set; }

        public SectionRectangle(double b, double h) : base()
        {
            B = b;
            H = h;
        }

        public override object Clone()
        {
            return new SectionRectangle(B, H);
        }
    }
}
