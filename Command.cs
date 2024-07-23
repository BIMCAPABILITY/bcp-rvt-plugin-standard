using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bimcapPlugin
{
    internal class Command
    {
        public static List<string> GetAllWarningTypes(Document doc)
        {
            List<string> warningTypes = new List<string>();

            // Get all warnings in the document
            IList<FailureMessage> warnings = doc.GetWarnings();

            foreach (FailureMessage warning in warnings)
            {
                string warningType = warning.GetDescriptionText();
                if (!warningTypes.Contains(warningType))
                {
                    warningTypes.Add(warningType);
                }
            }

            return warningTypes;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class ShowIsolateUI : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
             try
            {
                // Retrieve the Revit version
                string revitVersion = commandData.Application.Application.VersionNumber;

                // Create and show the isolateUI window
                isolateUI view = new isolateUI(commandData);
                view.SetTitleBarImage(revitVersion); // Pass the Revit version to the method
                view.ShowDialog();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}