using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Text;
using System.IO;
using Autodesk.Revit.DB.Architecture;
using System.Linq;
using System.Diagnostics;
using Autodesk.Revit.DB.Mechanical;

namespace bimkit.sheet_tools.autoTag_feature
{
    [Transaction(TransactionMode.Manual)]
    public class autoTag : IExternalCommand
    {
        // Store the active view and scale for future reference
        private static View _storedView;
        // Store the scale of the active view
        private static double _storedScale;
        // Store tag element IDs
        private List<ElementId> _tagElementIds = new List<ElementId>();
        // Store tag locations
        private List<double> _xPoints = new List<double>();
        private List<double> _yPoints = new List<double>();
        private List<double> _zPoints = new List<double>();
        // Store tag types and their IDs
        private List<string> _allTagTypes = new List<string>();
        private List<ElementId> _allTagTypeIds = new List<ElementId>();
        private List<string> _uniqueTagTypes = new List<string>();
        private List<ElementId> _uniqueTagTypeIds = new List<ElementId>();
        // Predefined list to store tag head shapes
        private Dictionary<string, List<Dictionary<string, object>>> _storedTagHeadShapes = new Dictionary<string, List<Dictionary<string, object>>>();
        // Predefined list to store scaled tag shapes
        private Dictionary<string, List<CurveLoop>> _storedScaledTagShapes = new Dictionary<string, List<CurveLoop>>();
        // Predefined list to store translated tag shapes
        private List<CurveLoop> _storedTranslatedTagShapes = new List<CurveLoop>();
        // Predefined list to store tag clash results
        private Dictionary<int, List<bool>> _storedTagClashResults = new Dictionary<int, List<bool>>();
        // Predefined list to store tag clash results
        private List<int> _filteredTagIndexes = new List<int>();
        // Predefined list to store clash pairs
        private List<List<int>> filteredTagClashPairs = new List<List<int>>();
        // Store picked ElementIds from the first item of each clash pair
        private List<ElementId> pickedClashingTagElementIds = new List<ElementId>();
        // Store all tag shapes involved in clash pairs
        private List<CurveLoop> _clashingTranslatedTagShapes = new List<CurveLoop>();
        // New fields to store the separated clashing shapes
        private List<CurveLoop> _clashingShape1List = new List<CurveLoop>();
        private List<CurveLoop> _clashingShape2List = new List<CurveLoop>();
        // Store scaled clashing shapes
        private List<CurveLoop> _scaledClashingShape1Loops = new List<CurveLoop>();

        public Result Execute(ExternalCommandData cData, ref string message, ElementSet elems)
        {
            UIDocument uiDoc = cData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            View activeView = uiDoc.ActiveView;

            // Show the UI
            autoTagUI ui = new autoTagUI();
            bool? result = ui.ShowDialog();

            if (result != true)
                return Result.Cancelled;

            List<string> selectedCategories = ui.SelectedCategories;
            int maxPasses = ui.IterationCount;

            StoreActiveViewAndScale(activeView);

            // STEP 1 — Tag once initially
            using (Transaction t = new Transaction(doc, "Initial Auto‑Tagging"))
            {
                t.Start();
                TagAllElements(doc, activeView, selectedCategories);
                t.Commit();
            }

            // STEP 2 — Iterative clash resolution
            int previousClashCount = int.MaxValue;

            for (int pass = 1; pass <= maxPasses; pass++)
            {
                int remaining = RunClashResolutionPass(doc, activeView, selectedCategories);

                TaskDialog.Show("Auto‑Tag Pass", $"Pass {pass}: tag clash removed = {remaining/2}");

                if (remaining == 0 || remaining >= previousClashCount)
                    break;

                previousClashCount = remaining;
            }

            return Result.Succeeded;
        }

        // Stores the active view and its scale for future reference.
        private void StoreActiveViewAndScale(View view)
        {
            _storedView = view;
            _storedScale = view.Scale;
        }

        // Tags all eligible elements in the active view.
        private void TagAllElements(Document doc, View view, List<string> selectedCategoryNames)
        {
            BuiltInCategory[] categoriesToTag = new BuiltInCategory[]
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Windows,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Ceilings,
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_CableTray,
                BuiltInCategory.OST_Conduit
            };

            List<Element> elementsToTag = new List<Element>();

            foreach (BuiltInCategory category in categoriesToTag)
            {
                Category cat = Category.GetCategory(doc, category);
                if (cat != null && selectedCategoryNames.Contains(cat.Name))
                {
                    FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id)
                        .OfCategory(category)
                        .WhereElementIsNotElementType();

                    elementsToTag.AddRange(collector);
                }
            }

            // --- Rooms
            List<Room> roomsToTag = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<Room>()
                .ToList();

            // --- Spaces
            List<Space> spacesToTag = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_MEPSpaces)
                .WhereElementIsNotElementType()
                .Cast<Space>()
                .ToList();

            // --- Areas
            List<Area> areasToTag = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType()
                .Cast<Area>()
                .ToList();

            // --- Tag general elements with type check
            foreach (Element elem in elementsToTag)
            {
                if (!IndependentTagAlreadyExists(doc, view, elem))
                {
                    // Check if a valid tag type exists for this element category
                    var hasTagType = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_Tags)
                        .Cast<FamilySymbol>()
                        .Any(sym => sym.Category != null && sym.Category.Id == elem.Category.Id);

                    if (!hasTagType)
                        continue; // Skip tagging this element

                    Reference elemRef = new Reference(elem);
                    XYZ tagPoint = GetElementTagPoint(elem);
                    IndependentTag.Create(doc, view.Id, elemRef, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, tagPoint);
                }
            }

            // --- Room tags
            if (selectedCategoryNames.Contains("Rooms") && HasLoadedTagType(doc, BuiltInCategory.OST_RoomTags))
            {
                foreach (Room room in roomsToTag)
                {
                    if (!RoomTagAlreadyExists(doc, view, room))
                    {
                        XYZ location = GetRoomCenter(room);
                        if (location != null)
                        {
                            doc.Create.NewRoomTag(new LinkElementId(room.Id), new UV(location.X, location.Y), view.Id);
                        }
                    }
                }
            }

            // --- Space tags
            if (selectedCategoryNames.Contains("Spaces") && HasLoadedTagType(doc, BuiltInCategory.OST_MEPSpaces))
            {
                foreach (Space space in spacesToTag)
                {
                    if (!SpaceTagAlreadyExists(doc, view, space))
                    {
                        XYZ location = GetSpaceCenter(space);
                        if (location != null)
                        {
                            doc.Create.NewSpaceTag(space, new UV(location.X, location.Y), view);
                        }
                    }
                }
            }

            // --- Area tags
            if (selectedCategoryNames.Contains("Areas") && HasLoadedTagType(doc, BuiltInCategory.OST_AreaTags))
            {
                foreach (Area area in areasToTag)
                {
                    if (!AreaTagAlreadyExists(doc, view, area))
                    {
                        XYZ location = GetAreaCenter(area);
                        if (location != null)
                        {
                            doc.Create.NewAreaTag((ViewPlan)view, area, new UV(location.X, location.Y));
                        }
                    }
                }
            }
        }

        // Retrieves all tags in the given view and their locations.
        private void GetAllTagsAndLocations(Document doc, View view, List<string> selectedCategoryNames)
        {
            // Mapping between categories and their tag categories
            Dictionary<string, BuiltInCategory> tagCategoryMap = new Dictionary<string, BuiltInCategory>
            {
                { "Walls", BuiltInCategory.OST_WallTags },
                { "Doors", BuiltInCategory.OST_DoorTags },
                { "Windows", BuiltInCategory.OST_WindowTags },
                { "Floors", BuiltInCategory.OST_FloorTags },
                { "Ceilings", BuiltInCategory.OST_CeilingTags },
                { "Structural Columns", BuiltInCategory.OST_StructuralColumnTags },
                { "Structural Framing", BuiltInCategory.OST_StructuralFramingTags },
                { "Pipes", BuiltInCategory.OST_PipeTags },
                { "Ducts", BuiltInCategory.OST_DuctTags },
                { "Cable Trays", BuiltInCategory.OST_CableTrayTags },
                { "Conduits", BuiltInCategory.OST_ConduitTags },
                { "Rooms", BuiltInCategory.OST_RoomTags },
                { "Spaces", BuiltInCategory.OST_MEPSpaces },      // Will use Location.Point
                { "Areas", BuiltInCategory.OST_AreaTags }
            };

            _tagElementIds.Clear();
            _xPoints.Clear();
            _yPoints.Clear();
            _zPoints.Clear();

            using (Transaction tx = new Transaction(doc, "Disable Tag Leaders"))
            {
                tx.Start();

                foreach (var entry in tagCategoryMap)
                {
                    string categoryName = entry.Key;
                    BuiltInCategory tagCategory = entry.Value;

                    if (!selectedCategoryNames.Contains(categoryName))
                        continue;

                    FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id)
                        .OfCategory(tagCategory)
                        .WhereElementIsNotElementType();

                    foreach (Element tag in collector)
                    {
                        XYZ tagLocation = null;

                        if (tag is IndependentTag independentTag)
                        {
                            tagLocation = independentTag.TagHeadPosition;
                        }
                        else if (tag is SpatialElementTag spatialTag && spatialTag.Location is LocationPoint locationPoint)
                        {
                            if (spatialTag.HasLeader)
                                spatialTag.HasLeader = false; // Temporarily disable for clash detection

                            tagLocation = locationPoint.Point;
                        }

                        if (tagLocation != null)
                        {
                            _tagElementIds.Add(tag.Id);
                            _xPoints.Add(tagLocation.X);
                            _yPoints.Add(tagLocation.Y);
                            _zPoints.Add(tagLocation.Z);
                        }
                    }
                }

                tx.Commit();
            }
        }

        // Retrieves all tag types in the given view.
        private void GetAllTagTypes(Document doc)
        {
            _allTagTypes.Clear();
            _uniqueTagTypes.Clear();
            _allTagTypeIds.Clear();
            _uniqueTagTypeIds.Clear();

            Dictionary<string, ElementId> uniqueTypeMapping = new Dictionary<string, ElementId>();

            foreach (ElementId tagId in _tagElementIds)
            {
                Element tagElement = doc.GetElement(tagId);
                if (tagElement != null)
                {
                    string familyName = "Unknown Family";
                    string typeName = "Unknown Type";
                    ElementId typeId = ElementId.InvalidElementId;

                    // Get the Element Type (Symbol)
                    Element typeElement = doc.GetElement(tagElement.GetTypeId());

                    if (typeElement is FamilySymbol symbol)
                    {
                        // Retrieve Family Name and Type
                        familyName = symbol.Family.Name;
                        typeName = symbol.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM)?.AsString() ?? "Unknown Type";
                        typeId = symbol.Id;
                    }
                    else if (typeElement != null)
                    {
                        // Fallback if it's not a FamilySymbol
                        typeName = typeElement.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM)?.AsString() ?? "Unknown Type";
                        familyName = typeElement.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME)?.AsString() ?? "Unknown Family";
                        typeId = typeElement.Id;
                    }

                    string tagTypeInfo = $"Family Type: {typeName}, Family: {familyName}";
                    _allTagTypes.Add(tagTypeInfo);
                    _allTagTypeIds.Add(typeId); // Store ElementId directly in the list

                    // Store only unique values
                    if (!uniqueTypeMapping.ContainsKey(tagTypeInfo))
                    {
                        uniqueTypeMapping[tagTypeInfo] = typeId;
                    }
                }
            }

            // Extract unique lists
            _uniqueTagTypes = uniqueTypeMapping.Keys.ToList();
            _uniqueTagTypeIds = uniqueTypeMapping.Values.ToList();
        }

        // Store extracted tag head shapes
        private void GetTagHeadShapes(List<string> uniqueTagTypeIds, Document doc)
        {
            const double FEET_TO_MM = 304.8; // Unit conversion factor
            _storedTagHeadShapes.Clear(); // Clear previous data

            foreach (string tagTypeId in uniqueTagTypeIds)
            {
                ElementId typeId = new ElementId((int)long.Parse(tagTypeId));
                FamilySymbol tagSymbol = doc.GetElement(typeId) as FamilySymbol;

                if (tagSymbol == null) continue;
                Family family = tagSymbol.Family;
                if (family == null || !family.IsEditable) continue;

                try
                {
                    Document familyDoc = doc.EditFamily(family);
                    FilteredElementCollector collector = new FilteredElementCollector(familyDoc)
                                                            .OfClass(typeof(CurveElement));

                    HashSet<XYZ> uniquePoints = new HashSet<XYZ>(new XYZComparer());

                    // Extract unique points
                    foreach (CurveElement curveElement in collector)
                    {
                        Curve curve = curveElement.GeometryCurve;
                        if (curve != null && curve is Line line)
                        {
                            uniquePoints.Add(line.GetEndPoint(0));
                            uniquePoints.Add(line.GetEndPoint(1));
                        }
                    }

                    familyDoc.Close(false);

                    List<XYZ> points = uniquePoints.ToList();

                    if (points.Count < 3) continue; // A loop needs at least 3 points

                    // Find the top-rightmost point
                    XYZ startPoint = points.OrderByDescending(p => p.Y).ThenByDescending(p => p.X).First();

                    // Calculate centroid
                    XYZ center = new XYZ(points.Average(p => p.X), points.Average(p => p.Y), 0);

                    // Sort points in anticlockwise order
                    points = points.OrderBy(p => Math.Atan2(p.Y - center.Y, p.X - center.X)).ToList();

                    // Ensure the starting point is first
                    int startIndex = points.IndexOf(startPoint);
                    if (startIndex > 0)
                    {
                        List<XYZ> sortedPoints = points.Skip(startIndex).ToList();
                        sortedPoints.AddRange(points.Take(startIndex));
                        points = sortedPoints;
                    }

                    // Ensure the loop is closed
                    if (!points.First().IsAlmostEqualTo(points.Last()))
                    {
                        points.Add(points.First());
                    }

                    // Convert points to dictionary format
                    List<Dictionary<string, object>> shapeData = new List<Dictionary<string, object>>();

                    for (int i = 0; i < points.Count - 1; i++) // -1 to avoid duplicate last line
                    {
                        XYZ start = points[i];
                        XYZ end = points[i + 1];

                        if (start.IsAlmostEqualTo(end)) continue; // Skip zero-length lines

                        XYZ direction = end - start;
                        double magnitude = direction.GetLength();

                        Dictionary<string, object> lineData = new Dictionary<string, object>
                {
                    { "Start", new { X = start.X * FEET_TO_MM, Y = start.Y * FEET_TO_MM, Z = start.Z * FEET_TO_MM } },
                    { "End", new { X = end.X * FEET_TO_MM, Y = end.Y * FEET_TO_MM, Z = end.Z * FEET_TO_MM } },
                    { "Direction", new { X = direction.X * FEET_TO_MM, Y = direction.Y * FEET_TO_MM, Z = direction.Z * FEET_TO_MM, Length = magnitude * FEET_TO_MM } }
                };

                        shapeData.Add(lineData);
                    }

                    if (shapeData.Count > 0)
                    {
                        _storedTagHeadShapes[tagTypeId] = shapeData;
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", $"Failed to extract tag shape for {tagTypeId}: {ex.Message}");
                }
            }
        }

        // Custom comparer to handle XYZ precision
        class XYZComparer : IEqualityComparer<XYZ>
        {
            private const double Tolerance = 0.0001;

            public bool Equals(XYZ a, XYZ b)
            {
                return a.IsAlmostEqualTo(b, Tolerance);
            }

            public int GetHashCode(XYZ obj)
            {
                return (obj.X, obj.Y, obj.Z).GetHashCode();
            }
        }

        // Process and scale the tag head shapes
        private void ProcessAndScaleTagHeadShapes()
        {
            if (_storedView == null || _storedTagHeadShapes.Count == 0)
                return;

            double scaleFactor = _storedScale; // Use stored view scale
            _storedScaledTagShapes.Clear(); // Clear previous data

            foreach (var tagShape in _storedTagHeadShapes)
            {
                string tagTypeId = tagShape.Key;
                List<Dictionary<string, object>> shapeData = tagShape.Value;

                List<Curve> curves = new List<Curve>();

                foreach (var lineData in shapeData)
                {
                    dynamic startData = lineData["Start"];
                    dynamic endData = lineData["End"];

                    XYZ start = new XYZ(startData.X / 304.8, startData.Y / 304.8, startData.Z / 304.8);
                    XYZ end = new XYZ(endData.X / 304.8, endData.Y / 304.8, startData.Z / 304.8);

                    //TaskDialog.Show("Debug", $"Room Tag Debug - Start: ({start.X}, {start.Y}, {start.Z}) -> End: ({end.X}, {end.Y}, {end.Z})");

                    curves.Add(Line.CreateBound(start, end));
                }

                // Convert to PolyCurve
                List<CurveLoop> polyCurves = ConvertToCurveLoops(curves);

                // Scale the PolyCurve 
                if (polyCurves != null && polyCurves.Count > 0)
                {
                    List<CurveLoop> scaledCurves = ScaleCurveLoops(polyCurves, scaleFactor);

                    // Store the scaled curves without overwriting existing data
                    if (scaledCurves.Count > 0)
                    {
                        if (!_storedScaledTagShapes.ContainsKey(tagTypeId))
                        {
                            _storedScaledTagShapes[tagTypeId] = new List<CurveLoop>();
                        }
                        _storedScaledTagShapes[tagTypeId].AddRange(scaledCurves);
                        //TaskDialog.Show("Debug", $"Tag {tagTypeId} -> {scaledCurves.Count} scaled curves added. Total stored: {_storedScaledTagShapes[tagTypeId].Count}");
                    }
                }
            }
        }

        // Convert separate curves into closed CurveLoops
        private List<CurveLoop> ConvertToCurveLoops(List<Curve> curves)
        {
            List<CurveLoop> curveLoops = new List<CurveLoop>();
            if (curves == null || curves.Count == 0) return curveLoops; // Return empty list

            // Sort curves into a continuous sequence
            List<Curve> sortedCurves = SortCurvesIntoLoop(curves);

            CurveLoop loop = new CurveLoop();

            foreach (Curve curve in sortedCurves)
            {
                try
                {
                    loop.Append(curve);
                }
                catch (Exception ex)
                {
                    //TaskDialog.Show("Error", $"Failed to append curve: {curve.GetEndPoint(0)} -> {curve.GetEndPoint(1)}\nError: {ex.Message}");
                    return null; // If one curve fails, the loop is broken.
                }
            }

            //TaskDialog.Show("Debug", $"Attempting to create CurveLoop with {sortedCurves.Count} curves.");

            // Ensure it's a closed loop
            if (loop.IsOpen()) return null;

            curveLoops.Add(loop);
            return curveLoops;
        }

        // Sort curves so they form a continuous loop in an anticlockwise direction
        private List<Curve> SortCurvesIntoLoop(List<Curve> curves)
        {
            List<Curve> sortedCurves = new List<Curve>();
            if (curves.Count == 0) return sortedCurves;

            // Find the curve with the highest positive X coordinate to start
            Curve startCurve = curves.OrderByDescending(c => Math.Max(c.GetEndPoint(0).X, c.GetEndPoint(1).X)).First();
            sortedCurves.Add(startCurve);
            curves.Remove(startCurve);

            XYZ lastEndPoint = startCurve.GetEndPoint(1);

            while (curves.Count > 0)
            {
                // Find the next curve that continues the loop in an anticlockwise manner
                Curve nextCurve = curves
                    .OrderByDescending(c =>
                        (c.GetEndPoint(0).X >= 0 && c.GetEndPoint(0).Y >= 0) ? 4 :
                        (c.GetEndPoint(0).X < 0 && c.GetEndPoint(0).Y >= 0) ? 3 :
                        (c.GetEndPoint(0).X < 0 && c.GetEndPoint(0).Y < 0) ? 2 : 1) // Assign priority based on quadrant
                    .ThenBy(c => c.GetEndPoint(0).DistanceTo(lastEndPoint))
                    .First();

                // Ensure continuity
                if (!lastEndPoint.IsAlmostEqualTo(nextCurve.GetEndPoint(0), 0.0001))
                {
                    //TaskDialog.Show("Error", "Curve continuity issue detected!");
                    return new List<Curve>(); // Return empty if discontinuity is found
                }

                sortedCurves.Add(nextCurve);
                curves.Remove(nextCurve);
                lastEndPoint = nextCurve.GetEndPoint(1);
            }
            return sortedCurves;
        }

        // Scale the curve loops using the view scale
        private List<CurveLoop> ScaleCurveLoops(List<CurveLoop> loops, double scaleFactor)
        {
            List<CurveLoop> scaledLoops = new List<CurveLoop>();

            foreach (CurveLoop loop in loops)
            {
                XYZ centroid = ComputeCentroid(loop);

                CurveLoop scaledLoop = new CurveLoop();
                foreach (Curve curve in loop)
                {
                    XYZ start = curve.GetEndPoint(0);
                    XYZ end = curve.GetEndPoint(1);

                    XYZ newStart = ScalePoint(start, centroid, scaleFactor);
                    XYZ newEnd = ScalePoint(end, centroid, scaleFactor);

                    scaledLoop.Append(Line.CreateBound(newStart, newEnd));
                }
                scaledLoops.Add(scaledLoop);
            }

            return scaledLoops;
        }

        // Compute the centroid of a curve loop
        private XYZ ComputeCentroid(CurveLoop loop)
        {
            double sumX = 0, sumY = 0, sumZ = 0;
            int count = 0;

            foreach (Curve curve in loop)
            {
                XYZ start = curve.GetEndPoint(0);
                XYZ end = curve.GetEndPoint(1);

                sumX += start.X + end.X;
                sumY += start.Y + end.Y;
                sumZ += start.Z + end.Z;
                count += 2;
            }

            return new XYZ(sumX / count, sumY / count, sumZ / count);
        }

        // Scale a point relative to the centroid
        private XYZ ScalePoint(XYZ point, XYZ centroid, double scaleFactor)
        {
            XYZ direction = point - centroid;
            return centroid + (direction * scaleFactor);
        }

        // Get the mapped tag shapes for all tags in the view
        private List<object> GetMappedTagShapes()
        {
            // Step 1: Create a mapping of uniqueTagTypeIds to their index
            Dictionary<ElementId, int> tagTypeToIndex = new Dictionary<ElementId, int>();
            for (int i = 0; i < _uniqueTagTypeIds.Count; i++)
            {
                tagTypeToIndex[_uniqueTagTypeIds[i]] = i;
            }

            // Step 2: Find the index of each tag type in _allTagTypeIds
            List<int> indexedValues = new List<int>();
            foreach (ElementId tagId in _allTagTypeIds)
            {
                indexedValues.Add(tagTypeToIndex.ContainsKey(tagId) ? tagTypeToIndex[tagId] : -1);
            }

            // Step 3: Replace index values with corresponding scaled tag shapes
            List<object> outputValues = new List<object>();
            foreach (int index in indexedValues)
            {
                if (index != -1 && index < _uniqueTagTypeIds.Count)
                {
                    string tagKey = _uniqueTagTypeIds[index].IntegerValue.ToString();
                    if (_storedScaledTagShapes.ContainsKey(tagKey))
                    {
                        outputValues.Add(_storedScaledTagShapes[tagKey]); // Return the scaled shapes
                    }
                    else
                    {
                        outputValues.Add("Unknown");
                    }
                }
                else
                {
                    outputValues.Add("Unknown");
                }
            }

            return outputValues;
        }

        // Translates the tag shapes to their respective locations
        private List<CurveLoop> TranslateTagShapes(List<CurveLoop> mappedTagShapes)
        {
            // Clear previously stored data to avoid stale values
            _storedTranslatedTagShapes.Clear();

            List<CurveLoop> translatedShapes = new List<CurveLoop>();

            for (int i = 0; i < mappedTagShapes.Count; i++)
            {
                CurveLoop shape = mappedTagShapes[i];

                if (shape == null) continue;

                double x = _xPoints[i];
                double y = _yPoints[i];
                double z = 0; // Always zero for this case

                // Translate the shape to the correct position
                Transform translation = Transform.CreateTranslation(new XYZ(x, y, z));
                CurveLoop translatedShape = CurveLoop.CreateViaTransform(shape, translation);

                translatedShapes.Add(translatedShape);
            }

            // Store translated shapes for future use
            _storedTranslatedTagShapes.AddRange(translatedShapes);

            return translatedShapes;
        }

        // Compute tag clashes between translated shapes
        private void ComputeTagClashes()
        {
            _storedTagClashResults.Clear(); // Clear previous results
            Dictionary<int, int> clashCounts = new Dictionary<int, int>();

            for (int i = 0; i < _storedTranslatedTagShapes.Count; i++)
            {
                List<bool> clashes = new List<bool>();
                int clashCount = 0; // Track the number of clashes for this tag

                for (int j = 0; j < _storedTranslatedTagShapes.Count; j++)
                {
                    // Now include self-intersection (like Dynamo does)
                    bool doesIntersect = DoShapesIntersect(_storedTranslatedTagShapes[i], _storedTranslatedTagShapes[j]);
                    clashes.Add(doesIntersect);

                    if (doesIntersect)
                        clashCount++; // Count every intersect, including self
                }

                _storedTagClashResults[i] = clashes;
                clashCounts[i] = clashCount; // Store count for debugging
            }

            // Debug: Display how many clashes each tag has
            string debugMessage = "Clash Counts Per Tag:\n";
            int countAboveTwo = 0;
            foreach (var kvp in clashCounts)
            {
                debugMessage += $"Tag {kvp.Key}: {kvp.Value} clashes\n";
                if (kvp.Value >= 2) countAboveTwo++;
            }

            debugMessage += $"\n\nTotal Tags with ≥2 Clashes: {countAboveTwo}";
            //TaskDialog.Show("SheetTools - Debug", debugMessage);
        }

        // Check if two CurveLoops (tag shapes) intersect by extruding them into solids and checking volume overlap
        private bool DoShapesIntersect(CurveLoop shape1, CurveLoop shape2)
        {
            try
            {
                Solid solid1 = CreateSolidFromTagShape(shape1);
                Solid solid2 = CreateSolidFromTagShape(shape2);

                if (solid1 == null || solid2 == null)
                    return false;

                Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                    solid1, solid2, BooleanOperationsType.Intersect);

                return intersection != null && intersection.Volume > 0;
            }
            catch
            {
                return false;
            }
        }

        // Create a thin extrusion from a flat CurveLoop (tag shape) for solid clash detection
        private Solid CreateSolidFromTagShape(CurveLoop loop)
        {
            try
            {
                List<CurveLoop> loops = new List<CurveLoop> { loop };
                return GeometryCreationUtilities.CreateExtrusionGeometry(
                    loops, XYZ.BasisZ, 1.0); // Extrude 1 unit in Z (sheet view assumed flat)
            }
            catch
            {
                return null;
            }
        }

        // Filter the tag clashes to only keep those that occur at least twice
        private void FilterClashingTags()
        {
            // Step 1: Count the number of clashes (true values) for each tag
            Dictionary<int, int> clashCounts = new Dictionary<int, int>();

            foreach (var kvp in _storedTagClashResults)
            {
                int clashCount = kvp.Value.Count(b => b); // Count number of 'true' values
                clashCounts[kvp.Key] = clashCount;
            }

            // Step 2: Filter tags with 2 or more clashes
            _filteredTagIndexes.Clear(); // Ensure list is empty before adding new values
            foreach (var kvp in clashCounts)
            {
                if (kvp.Value >= 2) // If a tag has 2 or more clashes
                {
                    _filteredTagIndexes.Add(kvp.Key);
                }
            }
        }

        // Compute and store filtered tag clash pairs
        private void ComputeFilteredTagClashPairs()
        {
            filteredTagClashPairs.Clear();
            var seenPairs = new HashSet<string>(); // To avoid duplicate pairs like [1,2] and [2,1]

            for (int i = 0; i < _filteredTagIndexes.Count; i++)
            {
                for (int j = i + 1; j < _filteredTagIndexes.Count; j++)
                {
                    int indexA = _filteredTagIndexes[i];
                    int indexB = _filteredTagIndexes[j];

                    if (indexA == indexB) continue;

                    string pairKey = indexA < indexB ? $"{indexA}-{indexB}" : $"{indexB}-{indexA}";
                    if (seenPairs.Contains(pairKey)) continue;

                    if (DoShapesIntersect(_storedTranslatedTagShapes[indexA], _storedTranslatedTagShapes[indexB]))
                    {
                        filteredTagClashPairs.Add(new List<int> { indexA, indexB });
                        seenPairs.Add(pairKey);
                    }
                }
            }
        }

        // Compute picked ElementIds from the filtered tag clash pairs
        private void ComputePickedTagElementIdsFromClashes()
        {
            pickedClashingTagElementIds.Clear();

            foreach (var pair in filteredTagClashPairs)
            {
                if (pair.Count > 0)
                {
                    int firstIndex = pair[0];

                    // Make sure the index is valid
                    if (firstIndex >= 0 && firstIndex < _tagElementIds.Count)
                    {
                        pickedClashingTagElementIds.Add(_tagElementIds[firstIndex]);
                    }
                }
            }
        }

        // Compute all clashing tag shapes based on filtered tag clash pairs
        private void ComputeAllClashingTagShapes()
        {
            _clashingTranslatedTagShapes.Clear();

            foreach (var pair in filteredTagClashPairs)
            {
                if (pair.Count >= 2)
                {
                    int indexA = pair[0];
                    int indexB = pair[1];

                    // Safety checks
                    if (indexA >= 0 && indexA < _storedTranslatedTagShapes.Count)
                    {
                        _clashingTranslatedTagShapes.Add(_storedTranslatedTagShapes[indexA]);
                    }

                    if (indexB >= 0 && indexB < _storedTranslatedTagShapes.Count)
                    {
                        _clashingTranslatedTagShapes.Add(_storedTranslatedTagShapes[indexB]);
                    }
                }
            }
        }

        // Call this method after _clashingTranslatedTagShapes has been populated
        private void SeparateClashingShapes()
        {
            _clashingShape1List.Clear();
            _clashingShape2List.Clear();

            for (int i = 0; i < _clashingTranslatedTagShapes.Count; i++)
            {
                // Even index: first shape in the pair
                // Odd index: second shape in the pair
                if (i % 2 == 0)
                    _clashingShape1List.Add(_clashingTranslatedTagShapes[i]);
                else
                    _clashingShape2List.Add(_clashingTranslatedTagShapes[i]);
            }
        }

        // Scale the clashing shapes
        private void ScaleClashingShape1Loops(double scaleFactor = 2.5)
        {
            _scaledClashingShape1Loops.Clear();

            foreach (var loop in _clashingShape1List)
            {
                if (loop == null || loop.Count() == 0)
                {
                    _scaledClashingShape1Loops.Add(null);
                    continue;
                }

                List<XYZ> originalPoints = new List<XYZ>();

                // Collect points from each curve (start points + final endpoint)
                foreach (var curve in loop)
                {
                    originalPoints.Add(curve.GetEndPoint(0));
                }
                originalPoints.Add(loop.Last().GetEndPoint(1)); // Add last endpoint

                // Remove duplicate points with rounding
                HashSet<string> seen = new HashSet<string>();
                List<XYZ> uniquePoints = new List<XYZ>();
                foreach (var pt in originalPoints)
                {
                    string key = $"{Math.Round(pt.X, 6)}_{Math.Round(pt.Y, 6)}_{Math.Round(pt.Z, 6)}";
                    if (!seen.Contains(key))
                    {
                        seen.Add(key);
                        uniquePoints.Add(pt);
                    }
                }

                if (uniquePoints.Count == 0)
                {
                    _scaledClashingShape1Loops.Add(null);
                    continue;
                }

                // Compute centroid
                double avgX = uniquePoints.Average(p => p.X);
                double avgY = uniquePoints.Average(p => p.Y);
                double avgZ = uniquePoints.Average(p => p.Z);
                XYZ centroid = new XYZ(avgX, avgY, avgZ);

                // Scale each point outward from centroid
                List<XYZ> scaledPoints = new List<XYZ>();
                foreach (var pt in uniquePoints)
                {
                    XYZ direction = (pt - centroid).Normalize();
                    double originalDistance = pt.DistanceTo(centroid);
                    double scaledDistance = originalDistance * scaleFactor;
                    XYZ scaledPt = centroid + (direction * scaledDistance);
                    scaledPoints.Add(scaledPt);
                }

                // Rebuild scaled CurveLoop
                CurveLoop scaledLoop = new CurveLoop();
                for (int i = 0; i < scaledPoints.Count - 1; i++)
                {
                    Line seg = Line.CreateBound(scaledPoints[i], scaledPoints[i + 1]);
                    scaledLoop.Append(seg);
                }
                // Close the loop
                Line closingLine = Line.CreateBound(scaledPoints.Last(), scaledPoints.First());
                scaledLoop.Append(closingLine);

                _scaledClashingShape1Loops.Add(scaledLoop);
            }
        }

        // Create patched surfaces from the scaled loops
        private List<Solid> CreatePatchedSurfacesFromLoops(List<CurveLoop> inputLoops)
        {
            List<Solid> surfaces = new List<Solid>();

            foreach (CurveLoop loop in inputLoops)
            {
                if (loop == null || loop.Count() < 3)
                {
                    surfaces.Add(null); // Skip invalid loops
                    continue;
                }

                try
                {
                    // Calculate extrusion direction based on loop plane (assume Z if not known)
                    XYZ normal = XYZ.BasisZ;

                    // Try to compute the plane of the loop
                    Plane plane = null;
                    try
                    {
                        plane = CurveLoop.CreateViaOffset(loop, 0.0, XYZ.BasisZ).GetPlane();
                        normal = plane.Normal;
                    }
                    catch
                    {
                        // fallback if plane detection fails
                        normal = XYZ.BasisZ;
                    }

                    // Extrude a very thin solid from the loop (similar to Curve.Patch)
                    Solid extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(
                        new List<CurveLoop> { loop },
                        normal,
                        0.01 // small thickness
                    );

                    surfaces.Add(extrusion);
                }
                catch
                {
                    surfaces.Add(null); // Skip failed cases
                }
            }

            return surfaces;
        }

        // Get the difference of two lists of solids
        public List<Solid> GetDifferenceOfSurfaces(List<Solid> primary, List<Solid> toSubtract)
        {
            List<Solid> result = new List<Solid>();

            foreach (var primarySolid in primary)
            {
                Solid current = primarySolid;

                foreach (var subtractSolid in toSubtract)
                {
                    try
                    {
                        current = BooleanOperationsUtils.ExecuteBooleanOperation(current, subtractSolid, BooleanOperationsType.Difference);
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Boolean Error", $"Failed to subtract solid: {ex.Message}");
                        continue;
                    }
                }

                if (current != null && current.Volume > 0)
                {
                    result.Add(current);
                }
            }

            return result;
        }

        // Get the farthest midpoints from the centroid of each solid surface
        private List<XYZ> GetFarthestMidpoints(List<Solid> solidSurfaces)
        {
            List<XYZ> farthestMidpoints = new List<XYZ>();

            foreach (Solid solid in solidSurfaces)
            {
                List<XYZ> allPoints = new List<XYZ>();

                foreach (Face face in solid.Faces)
                {
                    EdgeArrayArray loops = face.EdgeLoops;
                    foreach (EdgeArray loop in loops)
                    {
                        foreach (Edge edge in loop)
                        {
                            IList<XYZ> tess = edge.Tessellate();
                            if (tess.Count > 0)
                            {
                                allPoints.Add(tess.First());
                                allPoints.Add(tess.Last());
                            }
                        }
                    }
                }

                if (allPoints.Count == 0)
                {
                    farthestMidpoints.Add(null);
                    continue;
                }

                // Step 2: Calculate centroid of all points from all faces
                double avgX = allPoints.Average(p => p.X);
                double avgY = allPoints.Average(p => p.Y);
                double avgZ = allPoints.Average(p => p.Z);
                XYZ centroid = new XYZ(avgX, avgY, avgZ);

                // Step 3: Find farthest point and its midpoint with centroid
                XYZ farthestPoint = null;
                double maxDist = 0;

                foreach (XYZ pt in allPoints)
                {
                    double dist = pt.DistanceTo(centroid);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        farthestPoint = pt;
                    }
                }

                if (farthestPoint != null)
                {
                    XYZ midpoint = new XYZ(
                        (farthestPoint.X + centroid.X) / 2,
                        (farthestPoint.Y + centroid.Y) / 2,
                        (farthestPoint.Z + centroid.Z) / 2
                    );

                    farthestMidpoints.Add(midpoint);
                }
            }

            return farthestMidpoints;
        }

        // Relocate tags to the farthest midpoints
        private void RelocatePickedTagsToFarthestPoints(Document doc, List<ElementId> pickedTagIds, List<XYZ> farthestPoints)
        {
            if (pickedTagIds.Count != farthestPoints.Count)
            {
                TaskDialog.Show("Mismatch", "Tag count does not match farthest point count.");
                return;
            }

            using (Transaction tx = new Transaction(doc, "Relocate Clashing Tags"))
            {
                tx.Start();

                for (int i = 0; i < pickedTagIds.Count; i++)
                {
                    Element tagElem = doc.GetElement(pickedTagIds[i]);
                    XYZ targetPoint = farthestPoints[i];

                    if (tagElem is IndependentTag indepTag)
                    {
                        XYZ currentHead = indepTag.TagHeadPosition;
                        XYZ moveVec = targetPoint - currentHead;
                        indepTag.TagHeadPosition = currentHead + moveVec;
                    }
                    else if (tagElem is RoomTag roomTag)
                    {
                        // Turn off leader to allow free move
                        if (roomTag.HasLeader)
                            roomTag.HasLeader = false;

                        Location loc = roomTag.Location;
                        if (loc is LocationPoint locPoint)
                        {
                            XYZ current = locPoint.Point;
                            XYZ moveVec = targetPoint - current;
                            locPoint.Point = current + moveVec;
                        }
                    }
                    else if (tagElem is SpaceTag spaceTag)
                    {
                        // Turn off leader to allow free move
                        if (spaceTag.HasLeader)
                            spaceTag.HasLeader = false;

                        Location loc = spaceTag.Location;
                        if (loc is LocationPoint locPoint)
                        {
                            XYZ current = locPoint.Point;
                            XYZ moveVec = targetPoint - current;
                            locPoint.Point = current + moveVec;
                        }
                    }
                    else if (tagElem is AreaTag areaTag)
                    {
                        // Turn off leader to allow free move
                        if (areaTag.HasLeader)
                            areaTag.HasLeader = false;

                        Location loc = areaTag.Location;
                        if (loc is LocationPoint locPoint)
                        {
                            XYZ current = locPoint.Point;
                            XYZ moveVec = targetPoint - current;
                            locPoint.Point = current + moveVec;
                        }
                    }
                }

                tx.Commit();
            }
        }

        //-------------------Helper classes--------------------------
        // Checks if an independent tag already exists for an element.
        private bool IndependentTagAlreadyExists(Document doc, View view, Element element)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType();

            foreach (Element tag in collector)
            {
                if (tag is IndependentTag independentTag &&
                    independentTag.GetTaggedLocalElementIds().Contains(element.Id))
                {
                    return true;
                }
            }

            return false;
        }

        // Checks if a RoomTag already exists for a room.
        private bool RoomTagAlreadyExists(Document doc, View view, Room room)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .WhereElementIsNotElementType();

            foreach (Element tag in collector)
            {
                if (tag is RoomTag roomTag && roomTag.Room.Id == room.Id)
                {
                    return true;
                }
            }

            return false;
        }

        // Gets the center of an element's bounding box.
        private XYZ GetElementTagPoint(Element element)
        {
            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            return bbox != null ? (bbox.Min + bbox.Max) / 2 : XYZ.Zero;
        }

        // Gets the center location of a room.
        private XYZ GetRoomCenter(Room room)
        {
            return (room.Location as LocationPoint)?.Point ?? XYZ.Zero;
        }
        // Checks if an AreaTag already exists for an area.
        private bool AreaTagAlreadyExists(Document doc, View view, Area area)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_AreaTags)
                .WhereElementIsNotElementType();

            foreach (Element tag in collector)
            {
                if (tag is AreaTag areaTag && areaTag.Area.Id == area.Id)
                {
                    return true;
                }
            }

            return false;
        }

        // Checks if a SpaceTag already exists for a space.
        private bool SpaceTagAlreadyExists(Document doc, View view, Space space)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_MEPSpaces)
                .WhereElementIsNotElementType();

            foreach (Element tag in collector)
            {
                if (tag is SpaceTag spaceTag && spaceTag.Space.Id == space.Id)
                {
                    return true;
                }
            }

            return false;
        }

        // Gets the center location of an area.
        private XYZ GetAreaCenter(Area area)
        {
            return (area.Location as LocationPoint)?.Point ?? XYZ.Zero;
        }

        // Gets the center location of a space.
        private XYZ GetSpaceCenter(Space space)
        {
            return (space.Location as LocationPoint)?.Point ?? XYZ.Zero;
        }

        // Runs steps 2 → 18 once and returns the number of tags that still have ≥2 clashes
        private int RunClashResolutionPass(Document doc, View activeView, List<string> selectedCategoryNames)
        {
            // 2 Get locations                          
            GetAllTagsAndLocations(doc, activeView, selectedCategoryNames);

            // 3 Get tag types                          
            GetAllTagTypes(doc);

            // 4 → 8 build translated shapes & clash map
            GetTagHeadShapes(_uniqueTagTypeIds.Select(id => id.IntegerValue.ToString()).ToList(), doc);
            ProcessAndScaleTagHeadShapes();

            var mappedLoops = GetMappedTagShapes()
            .OfType<List<CurveLoop>>()
            .SelectMany(x => x)
            .ToList();
            TranslateTagShapes(mappedLoops);
            ComputeTagClashes();

            // 9 → 14 filter + scale problem shapes
            FilterClashingTags();                        // fills _filteredTagIndexes
            ComputeFilteredTagClashPairs();              // fills filteredTagClashPairs
            ComputePickedTagElementIdsFromClashes();     // fills pickedClashingTagElementIds
            ComputeAllClashingTagShapes();
            SeparateClashingShapes();
            ScaleClashingShape1Loops();

            // 15 → 16 create solids & boolean diff
            var shape1 = CreatePatchedSurfacesFromLoops(_clashingShape1List);
            var scaled = CreatePatchedSurfacesFromLoops(_scaledClashingShape1Loops);
            var shape2 = CreatePatchedSurfacesFromLoops(_clashingShape2List);

            List<Solid> step1 = GetDifferenceOfSurfaces(scaled, shape1);
            List<Solid> final = GetDifferenceOfSurfaces(step1, shape2);

            // 17 farthest points
            List<XYZ> farthest = GetFarthestMidpoints(final);

            // 18 relocate first‑tags of each pair                             
            RelocatePickedTagsToFarthestPoints(doc, pickedClashingTagElementIds, farthest);

            // remaining tags that still clash (≥2 true values)
            return _filteredTagIndexes.Count;
        }

        // Check if a tag type has been loaded in the document
        private bool HasLoadedTagType(Document doc, BuiltInCategory tagCategory)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(tagCategory)
                .Cast<FamilySymbol>()
                .Any(sym => sym.IsActive);
        }


    }
}
