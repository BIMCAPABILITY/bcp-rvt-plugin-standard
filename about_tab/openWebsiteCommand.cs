using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bimcapPlugin.about_tab
{
    [Transaction(TransactionMode.Manual)]
    public class OpenWebsiteCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.bimcap.com/",
                UseShellExecute = true
            });
            return Result.Succeeded;
        }
    }
}
