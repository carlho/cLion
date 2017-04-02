using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using cLion.Types;
using Grasshopper.Kernel.Types;

namespace CIFem_grasshopper
{
    public class MaterialGoo : GH_Goo<Material>
    {

        public MaterialGoo()
        {
            Value = new Material();
        }

        public MaterialGoo(Material mat)
        {
            if (mat == null)
                Value = new Material();
            else
                Value = mat;

        }

        public override bool IsValid
        {
            get
            {
                //TODO: More Checks!

                if (Value == null)
                    return false;
                else if (double.IsNaN(Value.Density))
                    return false;

                return true;
            }
        }

        public override string TypeDescription
        {
            get
            {
                return "A material";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Material";
            }
        }

        public override IGH_Goo Duplicate()
        {
            if (Value == null)
                return new MaterialGoo();

            return new MaterialGoo((Material)Value.Clone());
        }

        public override string ToString()
        {
            return Value.ToString();
        }


        #region casting methods

        public override bool CastTo<Q>(ref Q target)
        {
            //Cast to WR_Material.
            if (typeof(Q).IsAssignableFrom(typeof(Material)))
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

            //Cast from WR_Material
            if (typeof(Material).IsAssignableFrom(source.GetType()))
            {
                Value = (Material)source;
                return true;
            }

            return false;
        }

        #endregion
    }
}
