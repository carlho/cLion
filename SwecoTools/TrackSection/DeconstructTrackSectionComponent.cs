using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grasshopper.Kernel;

namespace SwecoTools
{
    public class DeconstructTrackSectionComponent : GH_Component
    {
        public DeconstructTrackSectionComponent() : base("DeconstructTrackSection", "DTS", "Deconstructs track sections into planes and values", "SwecoTools", "Utilities")
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("fcd80ce9-0fdf-404c-afa9-69223933d9b3");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new TrackSectionParameter(), "TrackSection", "TS", "The track section", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "The track section plane", GH_ParamAccess.item);
            pManager.AddTextParameter("Value", "Val", "Value of the track section", GH_ParamAccess.item);
            pManager.AddNumberParameter("NumberValue", "NumVal", "Value of the track section as a number", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            TrackSection ts = new TrackSection(Plane.WorldYZ, 0);

            if (!DA.GetData(0, ref ts))
                return;

            DA.SetData(0, ts.TrackSectionPlane);
            DA.SetData(1, ts.GetTrackSectionString());
            DA.SetData(2, ts.StartVal);
        }
    }
}
