using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

namespace SwecoTools
{
    public class TrackCurveGoo : GH_Goo<TrackCurve>
    {
        public TrackCurveGoo()
        {
            Value = new TrackCurve(null);
        }

        public TrackCurveGoo(TrackCurve ts)
        {
            if (ts == null)
                Value = new TrackCurve(null);
            else
                Value = ts;
        }

        public override bool IsValid
        {
            get
            {
                return Value.IsValid;
            }
        }

        public override string TypeDescription
        {
            get
            {
                return "A 3d curve describing a railway track";
            }
        }

        public override string TypeName
        {
            get
            {
                return "TrackCurve";
            }
        }

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new TrackCurveGoo();

            return new TrackCurveGoo((TrackCurve)Value.Clone());
        }

        public override string ToString()
        {
            return "TrackCurve";
        }



        #region casting methods

        public override bool CastTo<Q>(ref Q target)
        {
            //Cast to TrackSection
            if (typeof(Q).IsAssignableFrom(typeof(TrackCurve)))
            {
                if (Value == null)
                    target = default(Q);
                else
                    target = (Q)(object)Value;
                return true;
            }
            //Cast to Curve
            if (typeof(Q).IsAssignableFrom(typeof(Curve)))
            {
                if (Value == null)
                    target = default(Q);
                else
                    target = (Q)(object)Value.Curve;
                return true;
            }

            target = default(Q);
            return false;
        }



        public override bool CastFrom(object source)
        {
            if (source == null) { return false; }

            //Cast from TrackSection
            if (typeof(TrackCurve).IsAssignableFrom(source.GetType()))
            {
                Value = (TrackCurve)source;
                return true;
            }

            return false;
        }

        #endregion
    }
}
