using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SwecoTools
{
    public class ReadTunnelProfileComponent : GH_Component
    {
        // Written by Carl Hoff at Sweco Structures 
        // 2017-05-31

        // This component is written specifically to be custom to a given format 
        // TODO: Write a more generic version

        // Class variables
        bool _b = false;
        List<Point3d> _pts;


        public ReadTunnelProfileComponent() : base("TunnelProfile", "TP", "Reads a custom MicroStation format tunnel profile curve", "SwecoTools", "Tunnel")
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
            pManager.AddBooleanParameter("BooleanRead", "B", "A boolean controlling if the component should read the file. Could be used to suspend reading if the file is large", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("ClothoidTolerance", "ct", "An optional length parameter for evaluation points of the clothoids", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("xyProfile", "xyP", "XY profile curves created from the input file", GH_ParamAccess.list);
            pManager.AddPointParameter("xyProfile", "xyPts", "XY profile curves created from the input file", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string p = "";
            bool b = false;
            double tol = 1.0;

            if (!DA.GetData(0, ref p)) { return; }
            if (!DA.GetData(1, ref b)) { return; }
            DA.GetData(2, ref tol);

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
                List<Curve> xyCrvs = CreateXYProfileCurve(xyProfileData, tol);

                DA.SetDataList(0, xyCrvs);
                DA.SetDataList(1, _pts);
            }
        }

        ///////////////////////// GEOMETRY GENERATION /////////////////////////

        private List<Curve> CreateXYProfileCurve(string[,] xyProfileData, double tol)
        {
            List<Curve> crvs = new List<Curve>();
            List<Point3d> pts = new List<Point3d>();

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

        /// <summary>
        /// Creates a line from raw indata (xy format)
        /// </summary>
        /// <param name="xyProfileDataPart">The entire subset of xy profile data</param>
        /// <param name="row">The row on which to operate</param>
        /// <returns></returns>
        private Line CreateXYLine(string[,] xyProfileData, int row)
        {
            double x1 = CastToDouble(xyProfileData, row, 6, "xy data subset array");
            double y1 = CastToDouble(xyProfileData, row, 7, "xy data subset array");
            double x2 = CastToDouble(xyProfileData, row+1, 6, "xy data subset array");
            double y2 = CastToDouble(xyProfileData, row+1, 7, "xy data subset array");

            Point3d p1 = new Point3d(x1, y1, 0);
            Point3d p2 = new Point3d(x2, y2, 0);

            return new Line(p1, p2);
        }


        private Arc CreateXYArc(string[,] xyProfileData, int row)
        {
            double x1 = CastToDouble(xyProfileData, row, 6, "xy data subset array");
            double y1 = CastToDouble(xyProfileData, row, 7, "xy data subset array");
            double x2 = CastToDouble(xyProfileData, row + 1, 6, "xy data subset array");
            double y2 = CastToDouble(xyProfileData, row + 1, 7, "xy data subset array");

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
            Vector3d v2 = Vector3d.CrossProduct(v1, z);         // Vector perpendicular to connection line

            // New point on arc
            v1.Unitize();
            v2.Unitize();
            Point3d p3 = p1 + v1 * (d1 / 2) + v2 * d2;

            return new Arc(p1, p3, p2);
        }


        private Curve CreateXYClothoid(string[,] xyProfileData, int row, double tol)
        {
            double x1 = CastToDouble(xyProfileData, row, 6, "xy data subset array");
            double y1 = CastToDouble(xyProfileData, row, 7, "xy data subset array");
            double x2 = CastToDouble(xyProfileData, row + 1, 6, "xy data subset array");
            double y2 = CastToDouble(xyProfileData, row + 1, 7, "xy data subset array");

            double b1 = CastToDouble(xyProfileData, row, 8, "xy data subset array") * (2*Math.PI/400);  // Direction angle in p1
            double b2 = CastToDouble(xyProfileData, row + 1, 8, "xy data subset array") * (2 * Math.PI / 400);  // Direction angle in p2
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

            if (!_b)
            {
                _pts = new List<Point3d>();
                foreach (Point3d p in pts)
                {
                    _pts.Add(new Point3d(p.X, p.Y, p.Z));
                }
                _b = true;
            }

            // Check if the curve begins with non-zero curvature
            bool bwCurvature = false;
            if (R == Math.Abs(r1))
                bwCurvature = true;

            // Check the curve needs to be flipped
            bool flip = false;
            if (Math.Min(r1, r2) < 0)
                flip = true;
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
                p.Transform(Transform.Rotation(b1, Vector3d.ZAxis, rPt));

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
