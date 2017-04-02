using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace cLion.Types
{
    public class Node
    {


        public Node Copy()
        {
            return new Node();
        }
    }
}
