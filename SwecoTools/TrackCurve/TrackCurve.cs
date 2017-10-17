using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace SwecoTools
{
    public class TrackCurve : ICloneable
    {
        public Curve Curve { get; private set; }
        public List<TrackSection> TrackSections { get; private set; }

        public bool IsValid
        {
            get
            {
                if (Curve == null)
                    return false;

                else
                    return true;
            }
        }


        public TrackCurve(Curve c)
        {
            Curve = c;
            TrackSections = new List<TrackSection>();
        }

        public TrackCurve(Curve c, List<TrackSection> ts) : this(c)
        {
            for (int i = 0; i < ts.Count; i++)
                AddTrackSection(ts[i]);
        }

        

        /// <summary>
        /// Adds the section in the correct place, i.e. sorts the sections from lowest to highest
        /// </summary>
        /// <param name="ts"></param>
        public void AddTrackSection(TrackSection ts)
        {
            TrackSections.Add(ts);
            TrackSections = TrackSections.OrderBy(t => t.StartVal).ToList();
        }

        public void AddTrackSection(List<TrackSection> tss)
        {
            foreach (TrackSection ts in tss)
                this.AddTrackSection(ts);
        }



        /// <summary>
        /// Splits the curve at a specified track section value
        /// </summary>
        /// <param name="trackSectionValue"></param>
        /// <returns></returns>
        public TrackCurve[] Split(double trackSectionValue)
        {
            TrackCurve[] tcs = new TrackCurve[2];

            double t; TrackSection splitTS;
            if (!FindSection(trackSectionValue, out splitTS))
                throw new Exception("Track curve split attempt failed. Track section value does not exist within track curve");

            Curve.ClosestPoint(splitTS.TrackSectionPlane.Origin, out t);
            Curve[] crvs = Curve.Split(t);

            tcs[0] = new TrackCurve(crvs[0]);
            tcs[1] = new TrackCurve(crvs[1]);

            foreach (TrackSection tc in TrackSections)
            {
                if (tc.StartVal <= trackSectionValue)       // For a trackSectionValue that exists in a track section, add the track section to both parts 
                    tcs[0].AddTrackSection(tc);
                if (tc.StartVal >= trackSectionValue)
                    tcs[1].AddTrackSection(tc);
            }

            return tcs;
        }

        public TrackCurve RemoveStartSegment(double trackSectionValue)
        {
            if (TrackSectionValueOnEdge(trackSectionValue))
                return this;
            else
                return Split(trackSectionValue)[1];
        }

        public TrackCurve RemoveEndSegment(double trackSectionValue)
        {
            if (TrackSectionValueOnEdge(trackSectionValue))
                return this;
            else
                return Split(trackSectionValue)[0];
        }



        /// <summary>
        /// Checks if a submitted track section value is on the edge of the curve or if it is within the span
        /// </summary>
        /// <param name="tsv"></param>
        /// <returns>True if on the edge, false if in the span. Throws exception otherwise.</returns>
        private bool TrackSectionValueOnEdge(double tsv)
        {
            if (tsv < TrackSections[0].StartVal || tsv > TrackSections[TrackSections.Count - 1].EndVal)
                throw new Exception("Track section value " + tsv + "outside range of curve");

            else if (tsv == TrackSections[0].StartVal || tsv == TrackSections[TrackSections.Count - 1].EndVal)
                return true;

            else
                return false;
        }



        public bool FindSection(string s, out TrackSection ts)
        {
            ts = null;

            return FindSection(TrackSection.GetValueFromSectionString(s), out ts);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="val">Track section value</param>
        /// <param name="ts">Output section</param>
        /// <returns>True if submitted value is in range, false otherwise</returns>
        public bool FindSection(double val, out TrackSection ts)
        {
            ts = null;

            // Check whether value is within range
            if (val < TrackSections[0].StartVal || val>TrackSections[TrackSections.Count-1].EndVal)
                return false;

            // Find first section before point
            TrackSection tsBefore = null;
            for (int i = 0; i < TrackSections.Count - 1; i++)
                if (TrackSections[i].StartVal <= val && TrackSections[i + 1].EndVal >= val)
                    tsBefore = TrackSections[i];

            if (tsBefore == null)
                throw new Exception("Track section not found");


            // Find distance between sections
            double distance = val - tsBefore.StartVal;

            double t1, t2;
            Curve.ClosestPoint(tsBefore.TrackSectionPlane.Origin, out t1);
            Curve c2;
            if (Curve.Domain.T0==t1 || Curve.Domain.T1 == t1)    // If attempting to split at start or end point, use entire curve
                c2 = Curve;
            else
                c2 = Curve.Split(t1)[1];

            c2.LengthParameter(distance, out t2);

            // Return the track section on the first curve
            ts = TrackSection.FindTrackSectionOnCurve(Curve, c2.PointAt(t2), val);

            return true;
        }


        public void Transform(Transform t)
        {
            Curve.Transform(t);
            foreach (TrackSection ts in TrackSections)
                ts.Transform(t);
        }

        public void Translate(Vector3d v)
        {
            Curve.Translate(v);
            for (int i = 0; i < TrackSections.Count; i++)
                TrackSections[i].Translate(v);
        }


        public void UpdateSectionDirection()
        {
            for (int i = 0; i < TrackSections.Count; i++)
            {
                TrackSection ts = TrackSections[i];

                double t;
                Curve.ClosestPoint(ts.TrackSectionPlane.Origin, out t);

                Vector3d tangent = Curve.TangentAt(t);
                double angle = Vector3d.VectorAngle(ts.TrackSectionPlane.Normal, tangent, Plane.WorldXY);

                ts.RotatePlane(angle, Vector3d.ZAxis);

                TrackSections[i] = ts;
            }
        }


        public object Clone()
        {
            return new TrackCurve(Curve, TrackSections);
        }
    }
}
