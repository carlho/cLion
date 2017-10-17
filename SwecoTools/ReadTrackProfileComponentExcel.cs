using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SwecoTools
{
    public class ReadTrackProfileComponentExcel : GH_Component
    {
        // Written by Carl Hoff at Sweco Structures 
        // 2017-05-31

        // This component is written specifically to be custom to a given format 
        // TODO: Write a more generic version


        public ReadTrackProfileComponentExcel() : base("TrackProfile", "TP", "Reads a custom MicroStation format track profile curve", "SwecoTools", "Track")
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("a8b344c1-8819-4c58-850c-37fd6b21f371");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path to the excel file", GH_ParamAccess.item);
            pManager.AddPointParameter("xyStartPoint", "sP", "A point where the first xy curve segment begins", GH_ParamAccess.item);
            pManager.AddBooleanParameter("BooleanRead", "B", "A boolean controlling if the component should read the file. Could be used to suspend reading if the file is large", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Tolerance", "t", "A tolerance used for the creation of clothoids and the interpolated curve. Submit in Rhino units, conversion is made in component.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new TrackCurveParameter(), "xyProfile", "xyP", "XY profile curves created from the input file", GH_ParamAccess.list);
            pManager.AddParameter(new TrackCurveParameter(), "hProfile", "hP", "Height profile track curves created from the input file", GH_ParamAccess.item);
            pManager.AddParameter(new TrackCurveParameter(), "TrackCurve", "TC", "Combined track curve", GH_ParamAccess.item);
            //pManager.AddCurveParameter("DEBUG", "debug", "debug", GH_ParamAccess.item);
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
                // Get raw data
                List<string[,]> data = ExcelTools.ExcelUtilities.GetAllData2(p);
                
                if (data.Count != 2)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Excel file containing " + data.Count + " worksheet(s) rather than the expected 2. File correct?");

                    if (data.Count < 2)
                        return;
                }

                // Get subsets
                string[,] xyProfileData = GetXYProfileData(data[0]);
                string[,] hProfileData = GetHeightProfileData(data[1]);

                //Create plane curve
                TrackCurve xyTC = CreateXYProfileTrackCurve(xyProfileData, tol);
                TrackCurve hTC = CreateHeightProfileCurve(hProfileData);
                TrackCurve combTC = TrackCurveUtilities.JoinTrackCurves(xyTC, hTC, tol);

                xyTC.Translate((Vector3d)sPt);
                hTC.Translate((Vector3d)sPt);
                combTC.Translate((Vector3d)sPt);

                DA.SetData(0, xyTC);
                DA.SetData(1, hTC);
                DA.SetData(2, combTC);
                //DA.SetData(3, CurveDebug);
            }
        }

        ///////////////////////// GEOMETRY GENERATION /////////////////////////

        ///////////////////////////////// XY //////////////////////////////////

        private List<Curve> CreateXYProfileCurve(string[,] xyProfileData, double tol)
        {
            List<Curve> crvs = new List<Curve>();

            for (int i = 0; i < xyProfileData.GetLength(0); i++)
            {
                if (xyProfileData[i, 1] == "RL")
                    crvs.Add(CreateXYLine(xyProfileData, i).ToNurbsCurve());

                else if (xyProfileData[i, 1] == "C")
                    crvs.Add(CreateXYArc(xyProfileData, i).ToNurbsCurve());

                else if (xyProfileData[i, 1] == "ÖK")
                    crvs.Add(CreateXYClothoid(xyProfileData, i, tol).ToNurbsCurve());
            }

            return crvs;
        }


        private TrackCurve CreateXYProfileTrackCurve(string[,] xyProfileData, double tol)
        {
            List<Curve> crvs = new List<Curve>();
            List<TrackSection> tcs = new List<TrackSection>();

            for (int i = 0; i < xyProfileData.GetLength(0); i++)
            {
                if (xyProfileData[i, 1] == "RL")
                    crvs.Add(CreateXYLine(xyProfileData, i).ToNurbsCurve());

                else if (xyProfileData[i, 1] == "C")
                    crvs.Add(CreateXYArc(xyProfileData, i).ToNurbsCurve());

                else if (xyProfileData[i, 1] == "ÖK")
                    crvs.Add(CreateXYClothoid(xyProfileData, i, tol).ToNurbsCurve());

                // Create track sections
                if (xyProfileData[i, 2].StartsWith("KM"))
                {
                    TrackSection tc = null;

                    if (xyProfileData[i, 1] != "")
                        if (CreateTrackSection(xyProfileData[i, 2], crvs[crvs.Count - 1].PointAtStart, crvs[crvs.Count - 1], out tc))
                            tcs.Add(tc);

                    else // Maybe add another check here
                        if (CreateTrackSectionConnection(xyProfileData[i, 2], crvs[crvs.Count - 1].PointAtEnd, crvs[crvs.Count - 1], out tc))
                            tcs.Add(tc);
                }
            }


            Curve[] jCrvs = Curve.JoinCurves(crvs);
            //CurveDebug = jCrvs[0];                      //DEBUG
            if (jCrvs.Length > 1)
                throw new Exception("Unable to join curves");
            else
            {
                // Join and add track sections
                TrackCurve tc = new TrackCurve(jCrvs[0]);
                tc.AddTrackSection(tcs);

                // Vector from start point of curve to origin
                Vector3d v = new Vector3d(jCrvs[0].PointAtStart);
                v.Reverse();
                tc.Translate(v);

                return tc;
            }
        }


        /// <summary>
        /// Creates a line from raw indata (xy format)
        /// </summary>
        /// <param name="xyProfileDataPart">The entire subset of xy profile data</param>
        /// <param name="row">The row on which to operate</param>
        /// <returns></returns>
        private Line CreateXYLine(string[,] xyProfileData, int row)
        {
            double x1 = CastToDouble(xyProfileData, row, 7, "xy data subset array");
            double y1 = CastToDouble(xyProfileData, row, 6, "xy data subset array");
            double x2 = CastToDouble(xyProfileData, row+1, 7, "xy data subset array");
            double y2 = CastToDouble(xyProfileData, row+1, 6, "xy data subset array");

            Point3d p1 = new Point3d(x1, y1, 0);
            Point3d p2 = new Point3d(x2, y2, 0);

            return new Line(p1, p2);
        }


        private Arc CreateXYArc(string[,] xyProfileData, int row)
        {
            double x1 = CastToDouble(xyProfileData, row, 7, "xy data subset array");
            double y1 = CastToDouble(xyProfileData, row, 6, "xy data subset array");
            double x2 = CastToDouble(xyProfileData, row + 1, 7, "xy data subset array");
            double y2 = CastToDouble(xyProfileData, row + 1, 6, "xy data subset array");

            // Start and end point
            Point3d p1 = new Point3d(x1, y1, 0);
            Point3d p2 = new Point3d(x2, y2, 0);

            //// Mid point on arc ////
            // Distances
            double r = CastToDouble(xyProfileData, row, 3, "xy data subset array");
            double d1 = p1.DistanceTo(p2);                      // Length of connection line
            double d2 = Math.Abs(r) - Math.Sqrt(r * r - d1 * d1 / 4);     // Distance from midpoint of connection line to point on arc

            // Vectors
            Vector3d v1 = (Vector3d)p2 - (Vector3d)p1;          // Vector between p1 and p2
            Vector3d z = Vector3d.ZAxis * (r / Math.Abs(r));    // Signed z axis
            Vector3d v2 = Vector3d.CrossProduct(z, v1);         // Vector perpendicular to connection line

            // New point on arc
            v1.Unitize();
            v2.Unitize();
            Point3d p3 = p1 + v1 * (d1 / 2) + v2 * d2;

            return new Arc(p1, p3, p2);
        }


        private Curve CreateXYClothoid(string[,] xyProfileData, int row, double tol)
        {
            double x1 = CastToDouble(xyProfileData, row, 7, "xy data subset array");
            double y1 = CastToDouble(xyProfileData, row, 6, "xy data subset array");
            double x2 = CastToDouble(xyProfileData, row + 1, 7, "xy data subset array");
            double y2 = CastToDouble(xyProfileData, row + 1, 6, "xy data subset array");

            double b1 = CastToDouble(xyProfileData, row, 8, "xy data subset array") * (2*Math.PI/400);  // Direction angle (angle from y axis) in p1
            double b2 = CastToDouble(xyProfileData, row + 1, 8, "xy data subset array") * (2 * Math.PI / 400);  // Direction angle (angle from y axis) in p2
            double L = CastToDouble(xyProfileData, row + 1, 2, "xy data subset array");
            double r1 = CastToDouble(xyProfileData, row, 3, "xy data subset array");
            double r2 = CastToDouble(xyProfileData, row + 1, 3, "xy data subset array");
            double R = Math.Max(Math.Abs(r1), Math.Abs(r2));

            // Start and end point
            Point3d p1 = new Point3d(x1, y1, 0);
            Point3d p2 = new Point3d(x2, y2, 0);

            List<Point3d> pts = new List<Point3d>();
            double x, y, l;
            int iMax = (int)Math.Ceiling(L / tol);
            for (int i = 0; i < iMax; i++)
            {
                l = L * (i / ((double)iMax-1)); // Length parameter

                x = l -
                    Math.Pow(l, 5) / (40 * Math.Pow(R * L, 2)) +
                    Math.Pow(l, 9) / (3456 * Math.Pow(R * L, 4)) -
                    Math.Pow(l, 13) / (599040 * Math.Pow(R * L, 6));

                y = Math.Pow(l, 3) / (6 * R * L) -
                    Math.Pow(l, 7) / (336 * Math.Pow(R * L, 3)) +
                    Math.Pow(l, 11) / (42240 * Math.Pow(R * L, 5)) -
                    Math.Pow(l, 15) / (9676800 * Math.Pow(R * L, 7));

                pts.Add(new Point3d(x, y, 0));
            }

            // Check if the curve begins with non-zero curvature
            bool bwCurvature = false;
            if (R == Math.Abs(r1))
                bwCurvature = true;

            // Check the curve needs to be flipped
            bool flip = true;
            if (Math.Min(r1, r2) < 0)
                flip = false;
            if (bwCurvature)    // If the curve begins with curvature the need for flipping the curve is reversed
                flip = !flip;

            Point3d rPt;
            Vector3d mv;

            // If the clothoid begins with curvature and ends in a line, move it to correct for this
            if (!bwCurvature)
            {
                // If clothoids starts with no curvature
                mv = (Vector3d)p1;
                rPt = p1;
            }
            else
            {
                // If clothoid starts with curvature
                mv = (Vector3d)p2;
                rPt = p2;
                b1 = b2 + Math.PI;
            }


            for (int i = 0; i < pts.Count; i++)
            {
                Point3d p = pts[i];

                // Flip
                if (flip)
                    p.Transform(Transform.Mirror(Plane.WorldZX));

                // Move
                p = p + mv;

                // Rotate
                p.Transform(Transform.Rotation(-(b1-Math.PI/2), Vector3d.ZAxis, rPt));

                pts[i] = p;
            }

            // Swap "end" point
            if (!bwCurvature) // If clothoids starts with no curvature
                pts[pts.Count-1] = p2;
            else // If clothoid starts with curvature
                pts[pts.Count - 1] = p1;

            // Create curve
            Curve c = Curve.CreateInterpolatedCurve(pts, 3);
            
            // Flip reversed curves
            if(bwCurvature)
                c.Reverse();

            return c;
        }


        /////////////////////////////// HEIGHT ///////////////////////////////

        private TrackCurve CreateHeightProfileCurve(string[,] hProfileData)
        {
            List<Curve> crvs = new List<Curve>();
            List<TrackSection> ts = new List<TrackSection>();

            Point3d sPt = CreateFirstHeightPoint(hProfileData);
            Point3d cePt = new Point3d(sPt.X, sPt.Y, sPt.Z);
            bool b;

            for (int i = 0; i < hProfileData.GetLength(0); i++)
            {
                b = false;
                if (hProfileData[i, 0] == "Raklinje")
                {
                    crvs.Add(CreateHeightLine(hProfileData, i, cePt, 1).ToNurbsCurve());       // DEBUG TOLERANCE!
                    b = true;
                }

                else if (hProfileData[i, 0] == "Cirkulär")
                {
                    crvs.Add(CreateHeightArc(hProfileData, i, cePt, 1).ToNurbsCurve());       // DEBUG TOLERANCE!
                    b = true;
                }

                // Update current end point if curve has been added
                if (b)
                    cePt = crvs[crvs.Count - 1].PointAtEnd;


                // Create track section
                if (hProfileData[i, 1].StartsWith("KM"))
                {
                    TrackSection tc = null;

                    if (b || hProfileData[i, 0] == "Slutsektion")
                    {
                        if (CreateTrackSection(hProfileData[i, 1], crvs[crvs.Count - 1].PointAtStart, crvs[crvs.Count - 1], out tc))
                            ts.Add(tc);
                    }

                    else
                    {
                        if (CreateTrackSectionConnection(hProfileData[i, 1], cePt, crvs[crvs.Count - 1], out tc))
                            ts.Add(tc);
                    }

                }
            }

            Curve[] oCrvs = Curve.JoinCurves(crvs);
            if (oCrvs.Length != 1)
            {
                string message = "Error: more than one height curve created.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                throw new Exception(message);
            }
            else
            {
                TrackCurve tc = new TrackCurve(oCrvs[0], ts);
                
                // Move to origo
                Vector3d v = new Vector3d(-crvs[0].PointAtStart.X, 0, 0);
                tc.Translate(v);

                return tc;
            }
        }



        private Point3d CreateFirstHeightPoint(string[,] hProfileData)
        {
            double x1 = GetLengthFromSection(hProfileData, 0, 1, "h data subset array");
            double h1 = CastToDouble(hProfileData, 0, 3, "h data subset array");

            return new Point3d(x1, 0, h1);
        }

        private Line CreateHeightLine(string[,] hProfileData, int row, Point3d sPt, double tol)
        {
            //double x1 = GetLengthFromSection(hProfileData, row, 1, "h data subset array");
            //double h1 = CastToDouble(hProfileData, row, 3, "height data subset array");

            double x1 = sPt.X;
            double h1 = sPt.Z;
            double length = CastToDouble(hProfileData, row, 5, "height data subset array");      // Length

            string slopeStr = hProfileData[row, 4];
            slopeStr = slopeStr.Replace("‰", "");
            double slope = CastToDouble(slopeStr, "height data subset array");
            slope *= 0.001;

            return CreateHeightLine(sPt, slope, length, tol);
        }

        /// <summary>
        /// Creates a line in the xz plane
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="h1"></param>
        /// <param name="sPt"></param>
        /// <param name="slope"></param>
        /// <param name="length"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private Line CreateHeightLine(Point3d sPt, double slope, double length, double tol)
        {
            // Check inputs
            //CheckInputs(x1, h1, sPt, tol);

            double x1 = sPt.X;
            double h1 = sPt.Z;

            double x2 = x1 + length * Math.Cos(Math.Asin(slope));
            double h2 = h1 + length * slope;

            Point3d p1 = new Point3d(x1, 0, h1);
            Point3d p2 = new Point3d(x2, 0, h2);

            return new Line(p1, p2);
        }


        private Arc CreateHeightArc(string[,] hProfileData, int row, Point3d sPt, double tol)
        {
            //double x1 = GetLengthFromSection(hProfileData, row, 1, "h data subset array");
            // x1 = sPt.X;
            
            //double h1 = CastToDouble(hProfileData, row, 3, "h data subset array");
            // h1 = sPt.Z;

            double h2 = CastToDouble(hProfileData, row, 7, "h data subset array");
            double length = CastToDouble(hProfileData, row, 5, "h data subset array");
            double r = CastToDouble(hProfileData, row, 2, "h data subset array");

            r /= 2;     // QUICKFIX / HARDCODED!! radius in excel file really diameter 

            return CreateHeightArc(sPt, h2, length, r, tol);
        }

        private Arc CreateHeightArc(Point3d sPt, double h2, double length, double r, double tol)
        {
            // Check inputs
            //CheckInputs(x1, h1, sPt, tol);

            double x1 = sPt.X;
            double h1 = sPt.Z;

            // Angle and distance
            double alpha = length / (2 * r);            // Angle between start and mid point on arc, at the center of the arc 
            double gamma = alpha / 2;                   // Angle between vector from start to mid point and vector from start to end point.
            double d = 2 * r * Math.Sin(alpha);         // Straight distance between start and end point

            // Find mid point
            double l1 = 2 * r * Math.Sin(alpha / 2);    // Straight length between start and mid point
            double beta = Math.Asin((h2 - h1) / l1);    // Angle from x axis to vector between start and mid point
            double x2 = x1 + l1 * Math.Cos(beta);

            // Start and mid point
            Point3d p1 = new Point3d(x1, 0, h1);
            Point3d p2 = new Point3d(x2, 0, h2);

            // Vectors
            Vector3d v1 = (Vector3d)p2 - (Vector3d)p1;  // Vector between p1 and p2
            v1.Rotate(gamma, Vector3d.YAxis);
            v1.Unitize();

            // New point on arc
            v1.Unitize();
            Point3d p3 = p1 + v1 * d;

            return new Arc(p1, p2, p3);
        }


        private bool CreateTrackSection(string trackSection, Point3d sPt, Curve currCrv, out TrackSection tsc)
        {
            double t;
            if (currCrv.ClosestPoint(sPt, out t, 1 / RhinoUtilities.ScalingFactor()))
            {
                tsc = CreateTrackSection(trackSection, sPt, currCrv);
                return true;
            }
            else
            {
                tsc = null;
                return false;
            }
        }

        private TrackSection CreateTrackSection(string trackSection, Point3d sPt, Curve currCrv)
        {
            try
            {
                return new TrackSection(TrackSection.FindTrackPlaneOnCurve(currCrv, sPt), trackSection);
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unexpected exception found while creating track section " + trackSection +
                    " at " + sPt.ToString());
                throw;
            }
        }

        private bool CreateTrackSectionConnection(string trackSectionConnection, Point3d sPt, Curve currCrv, out TrackSection tsc)
        {
            double t;
            if (currCrv.ClosestPoint(sPt, out t, 1 / RhinoUtilities.ScalingFactor()))
            {
                tsc = CreateTrackSectionConnection(trackSectionConnection, sPt, currCrv);
                return true;
            }
            else
            {
                tsc = null;
                return false;
            }
        }

        private TrackSection CreateTrackSectionConnection(string trackSectionConnection, Point3d sPt, Curve currCrv)
        {
            try
            {
                return new TrackSectionConnection(TrackSection.FindTrackPlaneOnCurve(currCrv, sPt), trackSectionConnection);
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unexpected exception found while creating track section " + trackSectionConnection +
                    " at " + sPt.ToString());
                throw;
            }
        }




        private void CheckInputs(double x1, double h1, Point3d sPt, double tol)
        {
            if (Math.Abs(sPt.X - x1) > tol)
                throw new Exception("Error creating height arc. Start point local x value " + sPt.X + " deviation from given local x value " + x1 + " exceeds tolerance.");
            if (Math.Abs(sPt.Z - h1) > tol)
                throw new Exception("Error creating height arc. Start point height " + sPt.Z + " deviation from given height " + h1 + " exceeds tolerance.");
        }

        /////////////////////////// CASTING METHODS ///////////////////////////

        private double CastToDouble(string[,] data, int i, int j)
        {
            return CastToDouble(data, i, j, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="location">Name of data set (for exception handling).</param>
        /// <returns></returns>
        private double CastToDouble(string[,] data, int i, int j, string location)
        {
            double d;

            try
            {
                if (!ExcelTools.Utilities.SafeParseToDouble(data[i, j], out d))
                    throw new Exception();
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unexpected data found in " + location + "cell [" + i + "," + j + "]\nCall stack:\n" + e.StackTrace);
                throw e;
            }

            return d;
        }

        private double CastToDouble(string s, string location)
        {
            double d;

            try
            {
                if (!ExcelTools.Utilities.SafeParseToDouble(s, out d))
                    throw new Exception();
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unexpected data found in " + location + "\nCall stack:\n" + e.StackTrace);
                throw e;
            }

            return d;
        }


        private double GetLengthFromSection(string[,] data, int i, int j, string location)
        {
            string s = data[i, j];

            try
            {
                if (!s.StartsWith("KM"))
                    throw new Exception();
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unexpected data found in " + location + "cell [" + i + "," + j + "]\n" + 
                    "Section " + data [i,j] + "does not start with KM (as expected).\nCall stack:\n" + e.StackTrace);
                throw e;
            }

            s = s.Remove(0, 2);
            string[] splitStr = s.Split('+');
            double d = CastToDouble(splitStr[0], location) * 1000 + CastToDouble(splitStr[1], location);

            return d;
        }


        /////////////////////////// DATA EXTRACTION ///////////////////////////

        private string[,] GetXYProfileData(string[,] data)
        {
            int sRow = 16;

            string[,] xyProfileData = new string[data.GetLength(0)- sRow, data.GetLength(1)];
            for (int i = 0; i < xyProfileData.GetLength(0); i++)
                for (int j = 0; j < xyProfileData.GetLength(1); j++)
                    xyProfileData[i, j] = data[i + sRow, j];

            return xyProfileData;
        }

        private string[,] GetHeightProfileData(string[,] data)
        {
            int sRow = 23;

            string[,] hProfileData = new string[data.GetLength(0) - sRow, data.GetLength(1)];
            for (int i = 0; i < hProfileData.GetLength(0); i++)
                for (int j = 0; j < hProfileData.GetLength(1); j++)
                    hProfileData[i, j] = data[i + sRow, j];

            return hProfileData;
        }
    }
}
