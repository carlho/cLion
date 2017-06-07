using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;

namespace SwecoTools
{
    public class ReadExcelComponent : GH_Component
    {
        // Class variables
        string _allSheets = "all sheets";

        public ReadExcelComponent() : base("ExcelRead", "xlRead", "Reads an excel file", "SwecoTools", "xlUtilities")
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("634dbc96-af20-4f90-bb80-2827e31c170b");
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path to the excel file", GH_ParamAccess.item);
            pManager.AddTextParameter("Sheet", "S", "The name of the sheet(s) to extract (optional)", GH_ParamAccess.list, _allSheets);
            pManager.AddBooleanParameter("BooleanRead", "B", "A boolean controlling if the component should read the file. Could be used to suspend reading if the file is large", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Contents", "C", "Contents of the excel file", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string p = "";
            bool b = false;
            List<string> sheets = new List<string>();

            if (!DA.GetData(0, ref p)) { return; }
            if (!DA.GetDataList(1, sheets)) { return; }
            if (!DA.GetData(2, ref b)) { return; }

            if (b)
            {
                // Get raw data
                List<string[,]> data = new List<string[,]>();

                if (sheets.Count == 1 && sheets[0] == _allSheets)
                    data = ExcelTools.ExcelUtilities.GetAllData2(p);
                else
                    data = ExcelTools.ExcelUtilities.GetAllData2(p, sheets);

                // Sort data into tree structure
                Grasshopper.DataTree<string> dt = new Grasshopper.DataTree<string>();
                Grasshopper.Kernel.Data.GH_Path pth;

                int pCount = 0;
                foreach (string[,] dataSheet in data)
                {
                    for (int i = 0; i < dataSheet.GetLength(0); i++)
                    {
                        pth = new Grasshopper.Kernel.Data.GH_Path(pCount, i);

                        for (int j = 0; j < dataSheet.GetLength(1); j++)
                        {
                            dt.Add(dataSheet[i, j], pth);
                        }
                    }

                    pCount++;
                }

                DA.SetDataTree(0, dt);
            }
        }
    }
}
