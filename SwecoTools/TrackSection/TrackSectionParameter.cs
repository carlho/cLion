using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;

namespace SwecoTools
{
    public class TrackSectionParameter : GH_Param<TrackSectionGoo>
    {
        public TrackSectionParameter():base(new GH_InstanceDescription("TrackSection", "TS", "Track Section", "SwecoTools", "Parameters"))
        { }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("e53efb1a-a3b7-47dc-b635-2d6038251de9");
            }
        }
    }
}
