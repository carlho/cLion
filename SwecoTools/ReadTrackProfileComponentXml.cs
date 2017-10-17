using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Xml;

namespace SwecoTools
{
    public class ReadTrackProfileComponentXml : GH_Component
    {
        /*       Work in progress
         *       
        public ReadTrackProfileComponentXml() : base("TrackProfile", "TP", "Reads a custom Bentley Rail Track format track profile curve", "SwecoTools", "Track")
        {
        }
        */
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("811b2941-c208-48d2-9562-80cf4bc428c8");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path to the xml file", GH_ParamAccess.item);
            pManager.AddPointParameter("xyStartPoint", "sP", "A point where the first xy curve segment begins", GH_ParamAccess.item);
            pManager.AddBooleanParameter("BooleanRead", "B", "A boolean controlling if the component should read the file. Could be used to suspend reading if the file is large", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Tolerance", "t", "A tolerance used for the creation of clothoids and the interpolated curve. Submit in Rhino units, conversion is made in component.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new TrackCurveParameter(), "xyProfile", "xyP", "XY profile curves created from the input file", GH_ParamAccess.list);
            pManager.AddParameter(new TrackCurveParameter(), "hProfile", "hP", "Height profile track curves created from the input file", GH_ParamAccess.item);
            pManager.AddParameter(new TrackCurveParameter(), "TrackCurve", "TC", "Combined track curve", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string p = "";
            Point3d sPt = Point3d.Unset;
            bool b = false;
            double tol = 1.0;

            if (!DA.GetData(0, ref p)) { return; }
            if (!DA.GetData(1, ref sPt)) { return; }
            if (!DA.GetData(2, ref b)) { return; }
            DA.GetData(3, ref tol);

            // Recalculate tolerance to be in correct (local) units
            tol *= RhinoUtilities.ScalingFactor();

            if (b)
            {
                string command;
                string arguments = "";

                try
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(p);
                    XmlElement rootElement = document.DocumentElement;
                    XmlNodeList nodes = rootElement.ChildNodes;
                    foreach (XmlNode node in nodes)
                    {
                        if (node.Name.Equals("command"))
                        {
                            command = node.InnerText;
                        }
                        else
                        {
                            if (node.Name.Equals("arg"))
                            {
                                if (node.Attributes.Count == 1)
                                {
                                    if (node.Attributes.Item(0).Name.Equals("value"))
                                    {
                                        arguments = arguments + node.Attributes.Item(0).Value + " ";
                                    }
                                }
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
         }
    }
}
