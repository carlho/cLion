using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cLion.Types
{
    public class Element : ICloneable
    {
        public Element()
        {

        }

        public Element(Node sNode, Node eNode, Material m, Section s, )

        public virtual object Clone()
        {
            return new Element()
        }
    }
}
