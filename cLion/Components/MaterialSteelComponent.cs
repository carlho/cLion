using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using System.Drawing;
using cLion.Types;

namespace CIFem_grasshopper
{
    public class MaterialSteelComponent : GH_Component
    {
        public MaterialSteelComponent() : base("Material", "Mat", "Set up a material to use i structures", "cLion", "Elements")
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("26bc49d3-142e-431d-987e-74863f70c704");
            }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.tertiary;
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Material Stiffness", "E", "Youngs modulus for the beam, set to steel by deafault", GH_ParamAccess.item, 210e9);
            pManager.AddNumberParameter("Poisons ratio", "p", "Poisons ration for the material of the beam, set to steel by default", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Density", "d", "Density of the material. Given in kg/m3", GH_ParamAccess.item, 7800);
            pManager.AddNumberParameter("Ultimate Stress", "fu",
                "The yield stress for the material, used for utilisation checks. Note that yield stress is normally depending on the section thickness", GH_ParamAccess.item, 275e6);
            pManager.AddNumberParameter("Ultimate Stress", "fu", "The ultimate stress for the material, used for utilisation checks", GH_ParamAccess.item, 410e6); // Guess at the default value
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new MaterialParam(), "Material", "M", "The constructed material", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double E, p, rho, fy, fu;
            E = 0;
            p = 0;
            rho = 0;
            fy = 0;
            fu = 0;

            if (!DA.GetData(0, ref E)) { return; }
            if (!DA.GetData(1, ref p)) { return; }
            if (!DA.GetData(2, ref rho)) { return; }
            if (!DA.GetData(3, ref fy)) { return; }
            if (!DA.GetData(4, ref fu)) { return; }

            MaterialSteel mat = new MaterialSteel(E, p, rho, fy, fu);


            DA.SetData(0, mat);
        }

        /*
        protected override Bitmap Icon
        {
            get
            {
                return Properties.Resources.MaterialIcon;
            }
        }*/
    }
}
