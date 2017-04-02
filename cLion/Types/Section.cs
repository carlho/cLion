using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cLion.Types
{
    public class Section : ICloneable
    {
        public Section()
        {

        }

        public virtual object Clone()
        {
            return new Section();
        }
    }
}
