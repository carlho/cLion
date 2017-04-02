using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cLion.Types
{
    public class Material : ICloneable
    {
        public double Density { get; private set; }

        /// <summary>
        /// Should not be used!
        /// </summary>
        public Material()
        {
            Density = double.NaN;
        }

        public Material(double rho)
        {
            Density = rho;
        }

        public virtual object Clone()
        {
            return new Material(Density);
        }


        
    }
}
