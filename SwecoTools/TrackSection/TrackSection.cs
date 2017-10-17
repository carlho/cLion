using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace SwecoTools
{
    public class TrackSection : ICloneable
    {
        public Plane TrackSectionPlane { get; protected set; }
        protected double Value { get; set; }

        public double StartVal
        {
            get
            {
                return Value;
            }
        }
        public virtual double EndVal
        {
            get
            {
                return Value;
            }
        }


        ///////////////// CONSTRUCTORS /////////////////

        protected TrackSection(Plane p)
        {
            TrackSectionPlane = p;
        }

        public TrackSection(Plane p, double val) : this(p)
        {
            Value = val;
        }

        public TrackSection(Plane p, string sectionValueString) : this(p)
        {
            Value = GetValueFromSectionString(sectionValueString);
        }


        ///////////////// METHODS /////////////////

        public virtual object Clone()
        {
            return new TrackSection(TrackSectionPlane, Value);
        }

        public void Move(Vector3d v)
        {
            TrackSectionPlane.Translate(v);
        }


        /// <summary>
        /// Does not work if intended to return in format KMXXX+ YYYY.ZZZ
        /// </summary>
        /// <returns></returns>
        public string GetTrackSectionString()
        {
            return GetTrackSectionString(this.Value);
        }

        public string GetTrackSectionString(double value)
        {
            string[] parts = value.ToString().Split('.');

            string s = "KM";
            if (parts[0].Length > 5)
                s = s + parts[0].Remove(parts[0].Length - 3, 3);
            else if (parts[0].Length > 4)
                s = s + "0" + parts[0].Remove(parts[0].Length - 3, 3);
            else if (parts[0].Length > 3)
                s = s + "00" + parts[0].Remove(parts[0].Length - 3, 3);
            else
                s = s + "000";

            s += "+ ";

            if (parts[0].Length < 1)
                s += "000";
            else if (parts[0].Length < 2)
                s += "00" + parts[0];
            else if (parts[0].Length < 3)
                s += "0" + parts[0];
            else
                s += parts[0].Remove(0, 3);

            if (parts.Length > 1)
                s += "." + parts[1];
            else
                s += ".000";

            return s;
        }


        public bool Transform(Transform t)
        {
            throw new NotImplementedException();
            //return TrackSectionPlane.Transform(t);
        }

        public bool Translate(Vector3d v)
        {
            Plane p = TrackSectionPlane;
            p.Translate(v);

            TrackSectionPlane = p;
            return true;
        }

        public void RotatePlane(double angle, Vector3d axis)
        {
            Vector3d tangent = TrackSectionPlane.ZAxis;
            tangent.Rotate(angle, axis);

            Vector3d perp = Vector3d.CrossProduct(Vector3d.ZAxis, tangent);

            TrackSectionPlane = new Plane(TrackSectionPlane.Origin, perp, Vector3d.ZAxis);
        }


        //////////////////////////////////// STATIC METHODS ////////////////////////////////////


        public static TrackSection FindTrackSectionOnCurve(Curve c, Point3d pt, double val)
        {
            Plane p = FindTrackPlaneOnCurve(c, pt);

            return new TrackSection(p, val);
        }


        public static Plane FindTrackPlaneOnCurve(Curve c, Point3d pt)
        {
            double t;       // Parameter

            if (!c.ClosestPoint(pt, out t, 0.1))
                throw new Exception("Unable to find track plane, submitted point not on curve.");

            Vector3d tangent = c.TangentAt(t);
            Vector3d perp = Vector3d.CrossProduct(Vector3d.ZAxis, tangent);

            return new Plane(c.PointAt(t), perp, Vector3d.ZAxis);
        }


        public static double GetValueFromSectionString(string s)
        {
            try
            {
                if (!s.StartsWith("KM"))
                    throw new Exception();
            }
            catch (Exception e)
            {
                throw new Exception("Section " + s + "does not start with KM (as expected).\nCall stack:\n" + e.StackTrace);
            }

            s = s.Remove(0, 2);
            string[] splitStr = s.Split('+');

            double d1, d2 = -1.0;
            ExcelTools.Utilities.SafeParseToDouble(splitStr[0], out d1);
            ExcelTools.Utilities.SafeParseToDouble(splitStr[1], out d2);

            return d1 * 1000 + d2;
        }
    }
}
