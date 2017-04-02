using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Expressions;
using cLion.Goo;

namespace cLion.Parameters
{
    public class NodeParam : GH_Param<NodeGoo>
    {
        public NodeParam():base(new GH_InstanceDescription("Node", "N", "Node (3d)", "cLion", "Parameters"))
        { }


        public override Guid ComponentGuid
        { 
            get
            {
                return new Guid("c0d603b0-1a3e-4739-babe-9a69475177ac");
            }
        }

    }
}
