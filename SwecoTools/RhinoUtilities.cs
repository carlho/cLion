using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.DocObjects;

namespace SwecoTools
{
    public static class RhinoUtilities
    {

        /// <summary>
        /// Gets the unit system scaling factor from Rhino and returns a factor which when multiplied 
        /// with the current Rhino units gives the corresponding length in SI units. 
        /// General rule: divide when writing output to Rhino, multiply when recieving input from Rhino
        /// </summary>
        /// <returns></returns>
        public static double ScalingFactor()
        {
            Rhino.UnitSystem us = Rhino.RhinoDoc.ActiveDoc.PageUnitSystem;
            switch (us)
            {
                case Rhino.UnitSystem.None:
                    break;
                case Rhino.UnitSystem.Angstroms:
                    break;
                case Rhino.UnitSystem.Nanometers:
                    break;
                case Rhino.UnitSystem.Microns:
                    break;
                case Rhino.UnitSystem.Millimeters:
                    return 0.001;

                case Rhino.UnitSystem.Centimeters:
                    return 0.01;

                case Rhino.UnitSystem.Decimeters:
                    return 0.1;

                case Rhino.UnitSystem.Meters:
                    return 1;

                case Rhino.UnitSystem.Dekameters:
                    break;
                case Rhino.UnitSystem.Hectometers:
                    break;
                case Rhino.UnitSystem.Kilometers:
                    return 1000;

                case Rhino.UnitSystem.Megameters:
                    break;
                case Rhino.UnitSystem.Gigameters:
                    break;
                case Rhino.UnitSystem.Microinches:
                    break;
                case Rhino.UnitSystem.Mils:
                    break;
                case Rhino.UnitSystem.Inches:
                    break;
                case Rhino.UnitSystem.Feet:
                    break;
                case Rhino.UnitSystem.Yards:
                    break;
                case Rhino.UnitSystem.Miles:
                    break;
                case Rhino.UnitSystem.PrinterPoint:
                    break;
                case Rhino.UnitSystem.PrinterPica:
                    break;
                case Rhino.UnitSystem.NauticalMile:
                    break;
                case Rhino.UnitSystem.Astronomical:
                    break;
                case Rhino.UnitSystem.Lightyears:
                    break;
                case Rhino.UnitSystem.Parsecs:
                    break;
                case Rhino.UnitSystem.CustomUnitSystem:
                    break;
                default:
                    break;
            }

            // If unimplemented
            throw new NotImplementedException("Rhino document using unknown unit system not implemented in software. Try using SI units");
        }

    }
}
