using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;
using cLion.Types;

namespace cLion.Goo
{
    public class NodeGoo : GH_Goo<Node>
    {
        public NodeGoo()
        {
            this.Value = new Node();
        }

        public NodeGoo(Node value)
        {
            if (value == null)
                this.Value = new Node();
            else
                this.Value = value;
        }


        public override bool IsValid
        {
            get
            {
                if (Value == null)
                    return false;

                return true;
            }
        }

        public override string TypeDescription
        {
            get
            {
                return "Contains a node";
            }
        }

        public override string TypeName
        {
            get
            {
                return "Node";
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new NodeGoo(Value.Copy());
        }

        public override string ToString()
        {
            return "Node";
        }

        #region casting methods

        public override bool CastTo<Q>(ref Q target)
        {
            //Cast to WR_Node3d.
            if (typeof(Q).IsAssignableFrom(typeof(Node)))
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

            //Cast from WR_Node3d
            if (typeof(Node).IsAssignableFrom(source.GetType()))
            {
                Value = (Node)source;
                return true;
            }

            return false;
        }

        #endregion
    }
}
