using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using bimkit.sheet_tools.alignTags_feature;
using bimkit.sheet_tools.gridBubbleToggle_feature;
using bimkit.sheet_tools.titleBlockEditor_feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace bimkit
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

    [Transaction(TransactionMode.Manual)]
    public class ShowAlignTagUI : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                // Create and show the alignTagUI window
                alignTagUI view = new alignTagUI(commandData);
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

    [Transaction(TransactionMode.Manual)]
    public class ShowTitleBlockUI : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                // Create and show the MainWindow
                MainWindow view = new MainWindow(commandData.Application.ActiveUIDocument);
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

    [Transaction(TransactionMode.Manual)]
    public class ShowGridBubbleUI : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                // Get the active UIDocument
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;

                // Prompt user to select grids
                IList<Reference> selectedGrids = uidoc.Selection.PickObjects(ObjectType.Element, new GridSelectionFilterTwo(), "Select grids");

                // Convert references to grid elements
                List<Grid> grids = selectedGrids.Select(r => doc.GetElement(r) as Grid).ToList();

                // Create and show the GridActionWindow with selected grids
                GridActionWindow view = new GridActionWindow(grids, doc);
                view.ShowDialog();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Handle cancellation
                TaskDialog.Show("Info", "Operation cancelled.");
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }

    public class GridSelectionFilterTwo : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Grid;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}