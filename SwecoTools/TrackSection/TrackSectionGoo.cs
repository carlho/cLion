using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace SwecoTools
{
    public class TrackSectionGoo : GH_Goo<TrackSection>
    {
        public TrackSectionGoo()
        {
            Value = new TrackSection(Plane.WorldYZ, -1.0);
        }

        public TrackSectionGoo(TrackSection ts)
        {
            if (ts == null)
                Value = new TrackSection(Plane.WorldYZ, -1.0);
            else
                Value = ts;
        }

        public override bool IsValid
        {
            get
            {
                if (Value == null)
                    return false;
                else
                    return true;
            }
        }

        public override string TypeDescription
        {
            get
            {
                return "A track section containing a position plane and a value";
            }
        }

        public override string TypeName
        {
            get
            {
                return "TrackSection";
            }
        }

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new TrackSectionGoo();

            return new TrackSectionGoo((TrackSection)Value.Clone());
        }

        public override string ToString()
        {
            return Value.TrackSectionPlane.ToString() + ", " + Value.GetTrackSectionString();
        }


        #region casting methods

        public override bool CastTo<Q>(ref Q target)
        {
            //Cast to TrackSection
            if (typeof(Q).IsAssignableFrom(typeof(TrackSection)))
            {
                if (Value == null)
                    target = default(Q);
                else
                    target = (Q)(object)Value;
                return true;
            }

            target = default(Q);
            return false;
        }



        public override bool CastFrom(object source)
        {
            if (source == null) { return false; }

            //Cast from TrackSection
            if (typeof(TrackSection).IsAssignableFrom(source.GetType()))
            {
                Value = (TrackSection)source;
                return true;
            }

            return false;
        }

        #endregion
    }
}
