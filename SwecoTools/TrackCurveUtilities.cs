using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;



namespace SwecoTools
{
    public static class TrackCurveUtilities
    {

        /// <summary>
        /// Joins a planar xy track curve and a planar height curve
        /// </summary>
        /// <param name="xyTrackCurve"></param>
        /// <param name="hTrackCurve"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static TrackCurve JoinTrackCurves(TrackCurve xyTrackCurve, TrackCurve hTrackCurve, double tolerance)
        {
            // Find common start and end point
            double startVal = FindHighestStartVal(xyTrackCurve, hTrackCurve);
            double endVal = FindLowestEndVal(xyTrackCurve, hTrackCurve);

            // Divide both curves so that they start and stop at same section
            xyTrackCurve = xyTrackCurve.RemoveStartSegment(startVal);
            xyTrackCurve = xyTrackCurve.RemoveEndSegment(endVal);
            hTrackCurve = hTrackCurve.RemoveStartSegment(startVal);
            hTrackCurve = hTrackCurve.RemoveEndSegment(endVal);

            // Divide into normalised parameters 
            // Check so that tolerance is ok

            // Create new points
            int nSegments = (int)Math.Ceiling((xyTrackCurve.TrackSections[xyTrackCurve.TrackSections.Count-1].EndVal - 
                xyTrackCurve.TrackSections[0].StartVal)/tolerance);
            List<Point3d> hPts = SplitHTrackCurve(hTrackCurve.Curve, nSegments);
            Point3d[] oPts;
            xyTrackCurve.Curve.DivideByCount(nSegments, true, out oPts);
            List<Point3d> xyPts = oPts.ToList();

            List<Point3d> combinedPts = new List<Point3d>();
            for (int i = 0; i < hPts.Count; i++)
                combinedPts.Add(new Point3d(xyPts[i].X, xyPts[i].Y, hPts[i].Z));

            // Interpolate curve
            Curve c = Curve.CreateInterpolatedCurve(combinedPts, 3);

            // Add all track sections
            TrackCurve oTC = new TrackCurve(c);
            foreach (TrackSection ts in xyTrackCurve.TrackSections)
            {
                UpdateXYTrackSection(ts, c, tolerance);
                oTC.AddTrackSection(ts);
            }
            foreach (TrackSection ts in hTrackCurve.TrackSections)
            {
                UpdateHTrackSection(ts, hTrackCurve.Curve, c, tolerance);
                oTC.AddTrackSection(ts);
            }

            // Update section directions
            oTC.UpdateSectionDirection();

            return oTC;
        }


        private static double FindHighestStartVal(TrackCurve tc1, TrackCurve tc2)
        {
            double startVal = -1;
            if (tc1.TrackSections[0].StartVal < tc2.TrackSections[0].StartVal)
                startVal = tc2.TrackSections[0].StartVal;
            else
                startVal = tc1.TrackSections[0].StartVal;

            return startVal;
        }

        private static double FindLowestEndVal(TrackCurve tc1, TrackCurve tc2)
        {
            double endVal = -1;
            if (tc1.TrackSections[tc1.TrackSections.Count-1].EndVal > tc2.TrackSections[tc2.TrackSections.Count - 1].EndVal)
                endVal = tc2.TrackSections[tc2.TrackSections.Count - 1].EndVal;
            else
                endVal = tc1.TrackSections[tc1.TrackSections.Count - 1].EndVal;

            return endVal;
        }



        /// <summary>
        /// N.B. Only works if the curve is planar in the xz plane
        /// </summary>
        /// <param name="c"></param>
        /// <param name="n">Number of segments</param>
        /// <returns></returns>
        private static List<Point3d> SplitHTrackCurve(Curve c, int n)
        {
            List<Point3d> pts = new List<Point3d>();

            // Here it is assumed that the curve is planar in the xz plane.

            double sX = c.PointAtStart.X;
            double eX = c.PointAtEnd.X;
            double d = ((eX - sX) / n);
            Vector3d planeOffset = new Vector3d(d, 0, 0);

            Plane cutplane = Plane.WorldYZ;
            cutplane.Translate(new Vector3d(sX, 0, 0));

            for (int i = 0; i <= n; i++)
            {
                var events = Rhino.Geometry.Intersect.Intersection.CurvePlane(c, cutplane, d / 10000);

                if (events != null)
                {
                    if (events.Count != 1)
                        throw new Exception(
                            "Curve/plane intersection failed. Only one intersection point expected, number of intersection points found: " + events.Count);
                    else
                    {
                        if (events[0].IsPoint)
                            pts.Add(events[0].PointA);
                        else
                            throw new NotImplementedException();
                    }
                }

                cutplane.Translate(planeOffset);
            }

            return pts;
        }


        /// <summary>
        /// Updates an xy track section to a new 3d curve. For the method to work as intended the xy curve should lie directly above the origin of the track section plane 
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private static void UpdateXYTrackSection(TrackSection ts, Curve c, double tolerance)
        {
            var events = Rhino.Geometry.Intersect.Intersection.CurvePlane(c, ts.TrackSectionPlane, tolerance);

            if (events != null)
            {
                if (events.Count != 1)
                    throw new Exception(
                        "Curve/plane intersection failed. Only one intersection point expected, number of intersection points found: " + events.Count);
                else
                {
                    if (events[0].IsPoint)
                        ts.Translate(events[0].PointA - ts.TrackSectionPlane.Origin);
                    else if (events[0].IsOverlap)
                        ts.Translate(((events[0].PointA + events[0].PointB)/2) - ts.TrackSectionPlane.Origin);
                }
            }
        }


        /// <summary>
        /// Updates the height curve sections to a new 3d curve
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="c1">The original height curve</param>
        /// <param name="c2">The new 3d curve</param>
        private static void UpdateHTrackSection(TrackSection ts, Curve c1, Curve c2, double tolerance)
        {
            double t1;
            c1.ClosestPoint(ts.TrackSectionPlane.Origin, out t1);
            Interval iv = new Interval(c1.Domain.T0, t1);
            double length = c1.GetLength(iv);

            // Make projection of xy curve for length to be correct
            Curve c3 = c2.DuplicateCurve();
            c3.Transform(Rhino.Geometry.Transform.PlanarProjection(Plane.WorldXY));

            ts.Translate(c3.PointAtLength(length) - ts.TrackSectionPlane.Origin);
            UpdateXYTrackSection(ts, c2, tolerance);
        }

    }
}
