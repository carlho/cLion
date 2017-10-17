using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;

namespace SwecoTools
{
    public class TrackCurveParameter : GH_Param<TrackCurveGoo>
    {
        public TrackCurveParameter():base(new GH_InstanceDescription("TrackCurve", "TC", "Track Curve", "SwecoTools", "Parameters"))
        { }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("2d552e30-568c-4604-b42e-2f36914377a6");
            }
        }
    }
}
