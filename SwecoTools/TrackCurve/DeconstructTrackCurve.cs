using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel;

namespace SwecoTools
{
    public class DeconstructTrackCurve : GH_Component
    {
        public DeconstructTrackCurve() : base("DeconstructTrackCurve", "DTC", "Deconstructs a track curve into a curve and a list of track sections", "SwecoTools", "Utilities")
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("dc320ce7-4d52-477a-8f62-157b18cf1236");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new TrackCurveParameter(), "TrackCurve", "TC", "A track curve", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("TrackCurve", "C", "The track curve", GH_ParamAccess.item);
            pManager.AddParameter(new TrackSectionParameter(), "TrackSection", "TS", "The track sections", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            TrackCurve tc = null;
            if (!DA.GetData(0, ref tc)) { return; }

            DA.SetData(0, tc.Curve);
            DA.SetDataList(1, tc.TrackSections);
        }
    }
}
