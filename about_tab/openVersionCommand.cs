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
    public class OpenVersionCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Define the plugin version and release date
            string pluginVersion = "1.0.0"; // Replace with your version
            string releaseDate = "08-07-2024"; // Replace with your release date

            // Display the dialog box with the version and release date
            TaskDialog.Show("Plugin Information", $"Plugin Version: {pluginVersion}\nRelease Date: {releaseDate}");

            return Result.Succeeded;
        }
    }
}
