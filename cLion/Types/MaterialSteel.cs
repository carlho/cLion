using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cLion.Types
{
    public class MaterialSteel : Material
    {
        public double E { get; private set; }
        public double PoissonRatio { get; private set; }
        public double Fy { get; private set; }
        public double Fu { get; private set; }

        public MaterialSteel(double _E, double p, double rho, double fy, double fu) : base(rho)
        {
            E = _E;
            PoissonRatio = p;
            Fy = fy;
            Fu = fu;
        }

        public override object Clone()
        {
            return new MaterialSteel(E, PoissonRatio, Density, Fy, Fu);
        }
    }
}
