using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;

namespace SwecoTools
{
    public class FindTrackSection : GH_Component
    {
        public FindTrackSection() : base("FindTrackSection", "FTS", "Finds a track section on a track curve", "SwecoTools", "Utilities")
        { }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("2c8bb605-80aa-494d-9e5f-890f8491ac75");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TrackSectionString", "s", "A text string in the format KMXXX + YYY.ZZZ", GH_ParamAccess.item);
            pManager.AddParameter(new TrackCurveParameter(), "TrackCurve", "TC", "The track curve on which to search for the track section", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new TrackSectionParameter(), "TrackSection", "TS", "The track section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string s = "";
            if (!DA.GetData(0, ref s))
                return;

            TrackCurve tc = null;
            if (!DA.GetData(1, ref tc))
                return;

            TrackSection ts = null;

            try
            {
                if (!tc.FindSection(s, out ts))
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Section value exceeds the boundaries of the track curve");
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }

            DA.SetData(0, ts);
        }
    }
}
