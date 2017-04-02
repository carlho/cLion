using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;

namespace CIFem_grasshopper
{
    public class MaterialParam : GH_Param<MaterialGoo>
    {

        public MaterialParam() :base(new GH_InstanceDescription("Material", "M", "Material to use in structures","cLion", "Parameters"))
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("c60fcea3-189d-416e-ace3-53d4d7cb7950");
            }
        }

    }
}
