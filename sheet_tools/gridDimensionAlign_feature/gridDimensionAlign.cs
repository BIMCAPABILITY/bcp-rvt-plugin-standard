using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace bimkit.sheet_tools.gridDimensionAlign_feature
{
    [Transaction(TransactionMode.Manual)]
    public class gridDimensionAlign : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Selector for user to only select grids
                IList<Reference> selectedGrids = uidoc.Selection.PickObjects(ObjectType.Element, new GridSelectionFilter(), "Select Grids");

                // Group grids by orientation and parallelism
                var gridGroups = GroupGridsByOrientationAndParallelism(selectedGrids, doc);

                // Handle each group based on its orientation
                foreach (var group in gridGroups)
                {
                    HandleGridGroup(group.Key, group.Value, uidoc);
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Handle cancellation
                TaskDialog.Show("Info", "Operation cancelled.");
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
                return Result.Failed;
            }
        }

        private Dictionary<string, List<List<Grid>>> GroupGridsByOrientationAndParallelism(IList<Reference> selectedGrids, Document doc)
        {
            Dictionary<string, List<List<Grid>>> gridGroups = new Dictionary<string, List<List<Grid>>>
        {
            { "Vertical", new List<List<Grid>>() },
            { "Horizontal", new List<List<Grid>>() },
            { "Positive Slope", new List<List<Grid>>() },
            { "Negative Slope", new List<List<Grid>>() }
        };

            List<Grid> grids = selectedGrids.Select(gridRef => doc.GetElement(gridRef) as Grid).Where(grid => grid != null).ToList();

            foreach (var grid in grids)
            {
                string orientation = GetGridOrientation(grid);

                if (orientation == "Vertical" || orientation == "Horizontal")
                {
                    // Group all vertical or horizontal grids together
                    if (!gridGroups[orientation].Any())
                    {
                        gridGroups[orientation].Add(new List<Grid>());
                    }
                    gridGroups[orientation][0].Add(grid);
                }
                else
                {
                    bool addedToGroup = false;
                    foreach (var group in gridGroups[orientation])
                    {
                        if (AreGridsParallel(group.First(), grid))
                        {
                            group.Add(grid);
                            addedToGroup = true;
                            break;
                        }
                    }
                    if (!addedToGroup)
                    {
                        gridGroups[orientation].Add(new List<Grid> { grid });
                    }
                }
            }

            return gridGroups;
        }

        private string GetGridOrientation(Grid grid)
        {
            XYZ start = grid.Curve.GetEndPoint(0);
            XYZ end = grid.Curve.GetEndPoint(1);

            if (Math.Abs(start.X - end.X) < 1e-10)
            {
                return "Vertical";
            }
            else if (Math.Abs(start.Y - end.Y) < 1e-10)
            {
                return "Horizontal";
            }
            else
            {
                double slope = (end.Y - start.Y) / (end.X - start.X);
                return slope > 0 ? "Positive Slope" : "Negative Slope";
            }
        }

        private double GetGridSlope(Grid grid)
        {
            XYZ start = grid.Curve.GetEndPoint(0);
            XYZ end = grid.Curve.GetEndPoint(1);

            if (Math.Abs(start.X - end.X) < 1e-10)
            {
                return double.PositiveInfinity; // Vertical
            }
            else
            {
                return (end.Y - start.Y) / (end.X - start.X);
            }
        }

        private bool AreGridsParallel(Grid grid1, Grid grid2)
        {
            double slope1 = GetGridSlope(grid1);
            double slope2 = GetGridSlope(grid2);

            return Math.Abs(slope1 - slope2) < 1e-10;
        }

        private void HandleGridGroup(string orientation, List<List<Grid>> gridGroups, UIDocument uidoc)
        {
            foreach (var grids in gridGroups)
            {
                string gridNames = string.Join(", ", grids.Select(g => g.Name));

                switch (orientation)
                {
                    case "Vertical":
                        TaskDialog.Show("Vertical Grids", $"The grids {gridNames} are vertical. Please select a reference point on one of the grids to create dimensions.");

                        // Prompt user to select a reference point
                        XYZ referencePoint = uidoc.Selection.PickPoint("Select a reference point on the grid");

                        if (referencePoint != null)
                        {
                            // Assuming the reference point is valid, create dimensions
                            CreateDimensionsForVerticalGrids(grids, referencePoint, uidoc.Document);
                            //TaskDialog.Show("Result", $"Dimensions created for vertical grids {gridNames}.");
                        }
                        break;

                    case "Horizontal":
                        TaskDialog.Show("Horizontal Grids", $"The grids {gridNames} are horizontal. Please select a reference point on one of the grids to create dimensions.");

                        // Prompt user to select a reference point
                        XYZ referencePointHorizontal = uidoc.Selection.PickPoint("Select a reference point on the grid");

                        if (referencePointHorizontal != null)
                        {
                            // Assuming the reference point is valid, create dimensions
                            CreateDimensionsForHorizontalGrids(grids, referencePointHorizontal, uidoc.Document);
                            //TaskDialog.Show("Result", $"Dimensions created for horizontal grids {gridNames}.");
                        }
                        break;

                    case "Positive Slope":
                        TaskDialog.Show("Positive Slope Grids", $"The grids {gridNames} have a positive slope. Please select a reference point on one of the grids to create dimensions.");

                        // Prompt user to select a reference point
                        XYZ referencePointPositive = uidoc.Selection.PickPoint("Select a reference point on the grid");

                        if (referencePointPositive != null)
                        {
                            // Assuming the reference point is valid, create dimensions
                            CreateDimensionsForDiagonalGrids(grids, referencePointPositive, uidoc.Document);
                            //TaskDialog.Show("Result", $"Dimensions created for positive slope grids {gridNames}.");
                        }
                        break;

                    case "Negative Slope":
                        TaskDialog.Show("Negative Slope Grids", $"The grids {gridNames} have a negative slope. Please select a reference point on one of the grids to create dimensions.");

                        // Prompt user to select a reference point
                        XYZ referencePointNegative = uidoc.Selection.PickPoint("Select a reference point on the grid");

                        if (referencePointNegative != null)
                        {
                            // Assuming the reference point is valid, create dimensions
                            CreateDimensionsForDiagonalGrids(grids, referencePointNegative, uidoc.Document);
                            //TaskDialog.Show("Result", $"Dimensions created for negative slope grids {gridNames}.");
                        }
                        break;
                }
            }
        }

        private void CreateDimensionsForVerticalGrids(List<Grid> grids, XYZ referencePoint, Document doc)
        {
            using (Transaction trans = new Transaction(doc, "Create Dimensions for Vertical Grids"))
            {
                trans.Start();

                // Sort grids by their X position to ensure dimensions are created in order
                grids.Sort((g1, g2) => g1.Curve.GetEndPoint(0).X.CompareTo(g2.Curve.GetEndPoint(0).X));

                // Create a list to hold the references for dimension creation
                List<Reference> gridReferences = new List<Reference>();

                foreach (var grid in grids)
                {
                    gridReferences.Add(new Reference(grid));
                }

                // Create dimensions between each pair of grids
                for (int i = 0; i < gridReferences.Count - 1; i++)
                {
                    ReferenceArray referenceArray = new ReferenceArray();
                    referenceArray.Append(gridReferences[i]);
                    referenceArray.Append(gridReferences[i + 1]);

                    // Create the dimension line perpendicular to the grids
                    XYZ gridDirection = (grids[i].Curve as Line).Direction;
                    XYZ dimensionLineDirection = new XYZ(-gridDirection.Y, gridDirection.X, 0); // Perpendicular direction
                    XYZ dimensionLineStart = referencePoint;
                    XYZ dimensionLineEnd = dimensionLineStart + dimensionLineDirection * 10; // Extend the line for dimension placement

                    Line dimensionLine = Line.CreateBound(dimensionLineStart, dimensionLineEnd);

                    // Create the dimension
                    doc.Create.NewDimension(doc.ActiveView, dimensionLine, referenceArray);
                }

                trans.Commit();
            }
        }

        private void CreateDimensionsForHorizontalGrids(List<Grid> grids, XYZ referencePoint, Document doc)
        {
            using (Transaction trans = new Transaction(doc, "Create Dimensions for Horizontal Grids"))
            {
                trans.Start();

                // Sort grids by their Y position to ensure dimensions are created in order
                grids.Sort((g1, g2) => g1.Curve.GetEndPoint(0).Y.CompareTo(g2.Curve.GetEndPoint(0).Y));

                // Create a list to hold the references for dimension creation
                List<Reference> gridReferences = new List<Reference>();

                foreach (var grid in grids)
                {
                    gridReferences.Add(new Reference(grid));
                }

                // Create dimensions between each pair of grids
                for (int i = 0; i < gridReferences.Count - 1; i++)
                {
                    ReferenceArray referenceArray = new ReferenceArray();
                    referenceArray.Append(gridReferences[i]);
                    referenceArray.Append(gridReferences[i + 1]);

                    // Create the dimension line perpendicular to the grids
                    XYZ gridDirection = (grids[i].Curve as Line).Direction;
                    XYZ dimensionLineDirection = new XYZ(gridDirection.Y, -gridDirection.X, 0); // Perpendicular direction
                    XYZ dimensionLineStart = referencePoint;
                    XYZ dimensionLineEnd = dimensionLineStart + dimensionLineDirection * 10; // Extend the line for dimension placement

                    Line dimensionLine = Line.CreateBound(dimensionLineStart, dimensionLineEnd);

                    // Create the dimension
                    doc.Create.NewDimension(doc.ActiveView, dimensionLine, referenceArray);
                }

                trans.Commit();
            }
        }

        private (double, double, string) GetKeyPositionsAndName(Grid grid)
        {
            Line line = grid.Curve as Line;
            if (line != null)
            {
                XYZ start = line.GetEndPoint(0);
                XYZ end = line.GetEndPoint(1);
                double slope = (end.Y - start.Y) / (end.X - start.X);

                double topY = Math.Max(start.Y, end.Y);// top most Y value within grid
                double keyX = slope < 0 ? Math.Min(start.X, end.X) : Math.Max(start.X, end.X); //left most or right most x value

                string gridName = grid.Name;

                return (topY, keyX, gridName);
            }
            return (double.MinValue, double.MinValue, string.Empty);
        }

        private void CreateDimensionsForDiagonalGrids(List<Grid> grids, XYZ referencePoint, Document doc)
        {
            // Ask the user for sorting preference
            TaskDialog sortingDialog = new TaskDialog("Sorting Preference");
            sortingDialog.MainInstruction = "Choose how you want to sort the grids:";
            sortingDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Sort by Name");
            sortingDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Sort by Position");
            TaskDialogResult sortingResult = sortingDialog.Show();

            // Ask the user for dimension alignment preference
            TaskDialog dimensionDialog = new TaskDialog("Dimension Alignment");
            dimensionDialog.MainInstruction = "Choose how you want to align the dimension lines:";
            dimensionDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Perpendicular");
            dimensionDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Horizontal");
            dimensionDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Vertical");
            TaskDialogResult dimensionResult = dimensionDialog.Show();

            using (Transaction trans = new Transaction(doc, "Create Dimensions for Diagonal Grids"))
            {
                trans.Start();

                // Apply sorting based on user preference
                if (sortingResult == TaskDialogResult.CommandLink1) // Sort by Name
                {
                    grids.Sort((g1, g2) =>
                    {
                        var keyPos1 = GetKeyPositionsAndName(g1);
                        var keyPos2 = GetKeyPositionsAndName(g2);
                        return string.Compare(keyPos1.Item3, keyPos2.Item3, StringComparison.Ordinal);
                    });
                }
                else if (sortingResult == TaskDialogResult.CommandLink2) // Sort by Position
                {
                    grids.Sort((g1, g2) =>
                    {
                        var keyPos1 = GetKeyPositionsAndName(g1);
                        var keyPos2 = GetKeyPositionsAndName(g2);
                        int compareY = keyPos2.Item1.CompareTo(keyPos1.Item1);
                        if (compareY != 0) return compareY;

                        int compareX = keyPos1.Item2.CompareTo(keyPos2.Item2);
                        if (compareX != 0) return compareX;

                        return string.Compare(keyPos1.Item3, keyPos2.Item3, StringComparison.Ordinal);
                    });
                }

                List<Reference> gridReferences = new List<Reference>();

                foreach (var grid in grids)
                {
                    gridReferences.Add(new Reference(grid));
                }

                for (int i = 0; i < gridReferences.Count - 1; i++)
                {
                    ReferenceArray referenceArray = new ReferenceArray();
                    referenceArray.Append(gridReferences[i]);
                    referenceArray.Append(gridReferences[i + 1]);

                    XYZ gridDirection = (grids[i].Curve as Line).Direction;
                    XYZ dimensionLineDirection = new XYZ(-gridDirection.Y, gridDirection.X, 0);

                    XYZ dimensionLineStart = referencePoint;

                    Line gridLine = grids[i].Curve as Line;

                    if (dimensionResult == TaskDialogResult.CommandLink2) // Horizontal
                    {
                        // Find intersection using the Y-coordinate of the reference point
                        double y = referencePoint.Y;
                        double x = (y - gridLine.GetEndPoint(0).Y) / gridDirection.Y * gridDirection.X + gridLine.GetEndPoint(0).X;
                        dimensionLineStart = new XYZ(x, y, referencePoint.Z);
                    }
                    else if (dimensionResult == TaskDialogResult.CommandLink3) // Vertical
                    {
                        // Find intersection using the X-coordinate of the reference point
                        double x = referencePoint.X;
                        double y = (x - gridLine.GetEndPoint(0).X) / gridDirection.X * gridDirection.Y + gridLine.GetEndPoint(0).Y;
                        dimensionLineStart = new XYZ(x, y, referencePoint.Z);
                    }

                    XYZ dimensionLineEnd = dimensionLineStart + dimensionLineDirection * 10;

                    Line dimensionLine = Line.CreateBound(dimensionLineStart, dimensionLineEnd);

                    doc.Create.NewDimension(doc.ActiveView, dimensionLine, referenceArray);
                }

                trans.Commit();
            }
        }
        public class GridSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                return element is Grid;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
