using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace SwecoTools
{
    public class TrackSectionConnection : TrackSection
    {
        protected double Value2 { get; set; }

        public override double EndVal
        {
            get
            {
                return Value2;
            }
        }

        public TrackSectionConnection(Plane p, double val1, double val2) : base(p, val1)
        {
            Value2 = val2;
        }


        public TrackSectionConnection(Plane p, string sectionConnectionValueString) : base (p)
        {
            string[] sections = SplitTrackSectionConnectionString(sectionConnectionValueString);

            Value = GetValueFromSectionString(sections[0]);
            Value2 = GetValueFromSectionString(sections[1]);
        }


        public override object Clone()
        {
            return new TrackSectionConnection(TrackSectionPlane, Value, Value2);
        }

        private string[] SplitTrackSectionConnectionString(string tsc)
        {
                if (!tsc.StartsWith("KM"))
                    throw new Exception("Attempt to split track section " + tsc + "failed. Track section has unrecognised format");

            tsc = tsc.Replace(" ", "");         // Remove all blank spaces
            return tsc.Split('=');              // Split into two sections
        }

        /// <summary>
        /// Gets first track section as a string
        /// </summary>
        /// <returns></returns>
        public string GetTrackSectionString1()
        {
            return base.GetTrackSectionString(Value);
        }

        /// <summary>
        /// Gets second track section as a string
        /// </summary>
        /// <returns></returns>
        public string GetTrackSectionString2()
        {
            return base.GetTrackSectionString(Value2);
        }
    }
}
