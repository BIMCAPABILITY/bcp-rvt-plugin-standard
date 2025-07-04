using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Navigation;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;

namespace bimkit.sheet_tools.alignTags_feature
{
    public partial class alignTagUI : Window, INotifyPropertyChanged
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        public double Offset { get; private set; }
        public double SelectedAngle { get; private set; }
        public alignTagUI(ExternalCommandData commandData)
        {
            InitializeComponent();

            // Initialize the Revit API context
            UIApplication uiapp = commandData.Application;
            _uidoc = uiapp.ActiveUIDocument;
            _doc = _uidoc.Document;

            Offset = 900; // Default offset
            SelectedAngle = 90.0; // Default angle
            OffsetTextBox.Text = Offset.ToString("F0");
            // Set the slider's initial value to match the default offset
            OffsetSlider.Value = Offset;
            //SetTitleBarImage(); // Set the title bar image on initialization

            // Add event handler for text box changes
            OffsetTextBox.TextChanged += OffsetTextBox_TextChanged;
        }
        private void OffsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Offset = e.NewValue;
            OffsetTextBox.Text = Offset.ToString("F0"); // Update the text box with the slider value
        }
        private void OffsetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(OffsetTextBox.Text, out double newOffset))
            {
                Offset = newOffset;
                OffsetSlider.Value = Offset; // Update the slider with the text box value
            }
        }
        // Conversion from mm to feet
        public double GetOffsetInFeet()
        {
            return Offset / 304.8;
        }
        private void AlignButton_Click(object sender, RoutedEventArgs e)
        {
            // Determine the selected angle
            SelectedAngle = (bool)Angle90RadioButton.IsChecked ? 90.0 : 45.0;

            // Convert offset to feet
            double offsetInFeet = GetOffsetInFeet();

            this.Close();

            // Trigger the alignment process
            AlignTags(SelectedAngle, offsetInFeet);

            //// Close the dialog
            //this.DialogResult = true;
            //this.Close();
        }
        //public void SetTitleBarImage()
        //{
        //    // Define the base path for the plugin resources
        //    string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "BIMKIT.bundle", "Contents", "Resources");
        //    string imagePath = Path.Combine(basePath, "bimcap2.png");

        //    if (File.Exists(imagePath))
        //    {
        //        TitleBarImage.Source = new BitmapImage(new Uri(imagePath));
        //    }
        //    else
        //    {
        //        // Handle the case where the image file does not exist
        //        MessageBox.Show("Image file not found: " + imagePath);
        //    }
        //}

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AlignTags(double selectedAngle, double offset)
        {
            // Use the UIDocument and Document objects initialized in the constructor
            UIDocument uidoc = _uidoc;
            Document doc = _doc;

            try
            {
                // Prompt the user to select elements using the custom filter
                TagSelectionFilter filter = new TagSelectionFilter(
                  BuiltInCategory.OST_TextNotes,
                  BuiltInCategory.OST_RoomTags,
                  BuiltInCategory.OST_AreaTags,
                  BuiltInCategory.OST_WallTags,
                  BuiltInCategory.OST_DoorTags,
                  BuiltInCategory.OST_CeilingTags,
                  BuiltInCategory.OST_FloorTags,
                  BuiltInCategory.OST_WindowTags,
                  BuiltInCategory.OST_GenericModelTags,
                  BuiltInCategory.OST_AbutmentFoundationTags,
                  BuiltInCategory.OST_AbutmentPileTags,
                  BuiltInCategory.OST_AbutmentWallTags,
                  BuiltInCategory.OST_AlignmentsTags,
                  BuiltInCategory.OST_AssemblyTags,
                  BuiltInCategory.OST_AudioVisualDeviceTags,
                  BuiltInCategory.OST_BeamSystemTags,
                  BuiltInCategory.OST_CableTrayFittingTags,
                  BuiltInCategory.OST_CaseworkTags,
                  BuiltInCategory.OST_ColumnTags,
                  BuiltInCategory.OST_CommunicationDeviceTags,
                  BuiltInCategory.OST_ConduitFittingTags,
                  BuiltInCategory.OST_ConduitTags,
                  BuiltInCategory.OST_CouplerTags,
                  BuiltInCategory.OST_CurtainWallMullionTags,
                  BuiltInCategory.OST_CurtainWallPanelTags,
                  BuiltInCategory.OST_CurtaSystemTags,
                  BuiltInCategory.OST_DataDeviceTags,
                  BuiltInCategory.OST_DetailComponentTags,
                  BuiltInCategory.OST_DuctAccessoryTags,
                  BuiltInCategory.OST_DuctFittingTags,
                  BuiltInCategory.OST_DuctInsulationsTags,
                  BuiltInCategory.OST_DuctTags,
                  BuiltInCategory.OST_ElectricalCircuitTags,
                  BuiltInCategory.OST_ElectricalEquipmentTags,
                  BuiltInCategory.OST_ElectricalFixtureTags,
                  BuiltInCategory.OST_EntourageTags,
                  BuiltInCategory.OST_ExpansionJointTags,
                  BuiltInCategory.OST_FasciaTags,
                  BuiltInCategory.OST_FireAlarmDeviceTags,
                  BuiltInCategory.OST_FireProtectionTags,
                  BuiltInCategory.OST_FlexDuctTags,
                  BuiltInCategory.OST_FlexPipeTags,
                  BuiltInCategory.OST_FloorTags,
                  BuiltInCategory.OST_FurnitureTags,
                  BuiltInCategory.OST_GenericModelTags,
                  BuiltInCategory.OST_HandrailTags,
                  BuiltInCategory.OST_KeynoteTags,
                  BuiltInCategory.OST_LightingDeviceTags,
                  BuiltInCategory.OST_LightingFixtureTags,
                  BuiltInCategory.OST_MassTags,
                  BuiltInCategory.OST_MaterialTags,
                  BuiltInCategory.OST_MechanicalControlDeviceTags,
                  BuiltInCategory.OST_MechanicalEquipmentSetTags,
                  BuiltInCategory.OST_MechanicalEquipmentTags,
                  BuiltInCategory.OST_MedicalEquipmentTags,
                  BuiltInCategory.OST_ModelGroupTags,
                  BuiltInCategory.OST_MultiCategoryTags,
                  BuiltInCategory.OST_PadTags,
                  BuiltInCategory.OST_ParkingTags,
                  BuiltInCategory.OST_PartTags,
                  BuiltInCategory.OST_PierCapTags,
                  BuiltInCategory.OST_PierColumnTags,
                  BuiltInCategory.OST_PierPileTags,
                  BuiltInCategory.OST_PierWallTags,
                  BuiltInCategory.OST_PipeAccessoryTags,
                  BuiltInCategory.OST_PipeFittingTags,
                  BuiltInCategory.OST_PipeInsulationsTags,
                  BuiltInCategory.OST_PipeTags,
                  BuiltInCategory.OST_PlantingTags,
                  BuiltInCategory.OST_PlumbingEquipmentTags,
                  BuiltInCategory.OST_PlumbingFixtureTags,
                  BuiltInCategory.OST_RampTags,
                  BuiltInCategory.OST_RebarTags,
                  BuiltInCategory.OST_RoofTags,
                  BuiltInCategory.OST_RoomTags,
                  BuiltInCategory.OST_RvtLinksTags,
                  BuiltInCategory.OST_SecurityDeviceTags,
                  BuiltInCategory.OST_SignageTags,
                  BuiltInCategory.OST_SitePropertyLineSegmentTags,
                  BuiltInCategory.OST_SitePropertyTags,
                  BuiltInCategory.OST_SiteTags,
                  BuiltInCategory.OST_SlabEdgeTags,
                  BuiltInCategory.OST_SpecialityEquipmentTags,
                  BuiltInCategory.OST_SprinklerTags,
                  BuiltInCategory.OST_StairsLandingTags,
                  BuiltInCategory.OST_StairsRailingTags,
                  BuiltInCategory.OST_StairsRunTags,
                  BuiltInCategory.OST_StairsSupportTags,
                  BuiltInCategory.OST_StairsTags,
                  BuiltInCategory.OST_StairsTriserTags,
                  BuiltInCategory.OST_StructConnectionAnchorTags,
                  BuiltInCategory.OST_StructConnectionBoltTags,
                  BuiltInCategory.OST_StructConnectionHoleTags,
                  BuiltInCategory.OST_StructConnectionPlateTags,
                  BuiltInCategory.OST_StructConnectionProfilesTags,
                  BuiltInCategory.OST_StructConnectionShearStudTags,
                  BuiltInCategory.OST_StructConnectionTags,
                  BuiltInCategory.OST_StructConnectionWeldTags,
                  BuiltInCategory.OST_StructuralColumnTags,
                  BuiltInCategory.OST_StructuralFoundationTags,
                  BuiltInCategory.OST_StructuralFramingTags,
                  BuiltInCategory.OST_StructuralStiffenerTags,
                  BuiltInCategory.OST_StructuralTendonTags,
                  BuiltInCategory.OST_TelephoneDeviceTags,
                  BuiltInCategory.OST_TopRailTags,
                  BuiltInCategory.OST_TrussTags,
                  BuiltInCategory.OST_WallSweepTags,
                  BuiltInCategory.OST_WallTags,
                  BuiltInCategory.OST_WindowTags,
                  BuiltInCategory.OST_WireTags,
                  BuiltInCategory.OST_ZoneTags
                );

                IList<Reference> pickedRefs = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Select tags in the view");

                // Retrieve the selected tag IDs
                List<ElementId> tagIds = new List<ElementId>();
                foreach (Reference reference in pickedRefs)
                {
                    tagIds.Add(reference.ElementId);
                }

                if (tagIds.Count == 0)
                {
                    MessageBox.Show("No tags selected. Please select at least one tag.");
                    return;
                }

                // Loop to allow repeated relocation
                XYZ targetPoint;
                do
                {
                    try
                    {
                        // Ask user to specify the destination point for the move operation
                        targetPoint = uidoc.Selection.PickPoint("Pick the destination point for the move (Right-click to finalize)");

                        // Perform alignment and movement
                        AlignAndMoveSelectedElements(doc, tagIds, targetPoint, selectedAngle, offset);
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        // Right-click cancels the operation, finalize the last position
                        break;
                    }
                } while (true);
            }
            catch (Exception ex) when (ex.Message.Contains("The user aborted the pick operation"))
            {
                MessageBox.Show("The user aborted the pick operation. Please click the plugin button again to restart the process.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void AlignAndMoveSelectedElements(Document doc, List<ElementId> elementIds, XYZ targetPoint, double angle, double offset)
        {
            using (Transaction trans = new Transaction(doc, "Align and Move Elements"))
            {
                trans.Start();

                try
                {
                    // Create a combined list of all tags
                    List<Element> allTags = new List<Element>();

                    foreach (ElementId id in elementIds)
                    {
                        Element element = doc.GetElement(id);
                        if (element is IndependentTag || element is SpatialElementTag)
                        {
                            allTags.Add(element);
                        }
                    }

                    // Function to get the leader end point for an independent tag
                    XYZ GetLeaderEndPoint(IndependentTag tag)
                    {
                        SetTagToFreeEnd(doc, tag);
                        Reference tagReference = tag.GetTaggedReferences().FirstOrDefault();
                        if (tagReference != null)
                        {
                            return tag.GetLeaderEnd(tagReference);
                        }
                        return null;
                    }

                    // Function to get the location point for a spatial tag
                    XYZ GetLocationPoint(SpatialElementTag tag)
                    {
                        LocationPoint loc = tag.Location as LocationPoint;
                        return loc?.Point;
                    }

                    XYZ GetLeaderOrLocationPoint(Element tag)
                    {
                        if (tag is IndependentTag independentTag)
                        {
                            return GetLeaderEndPoint(independentTag);
                        }
                        else if (tag is SpatialElementTag spatialTag)
                        {
                            return GetLocationPoint(spatialTag);
                        }
                        return null;
                    }

                    // Sorting function for combined list
                    allTags.Sort((a, b) =>
                    {
                        XYZ pointA = GetLeaderOrLocationPoint(a);
                        XYZ pointB = GetLeaderOrLocationPoint(b);

                        if (pointA == null && pointB == null)
                        {
                            return 0; // Both points are null, consider them equal
                        }
                        if (pointA == null)
                        {
                            return 1; // Consider null greater to push it to the end
                        }
                        if (pointB == null)
                        {
                            return -1; // Consider null greater to push it to the end
                        }

                        return pointB.Y.CompareTo(pointA.Y);
                    });

                    // Calculate new positions based on sorted order
                    double currentY = targetPoint.Y;

                    foreach (Element tag in allTags)
                    {
                        if (tag is IndependentTag independentTag)
                        {
                            SetTagToFreeEnd(doc, independentTag);
                            AlignIndependentTag(doc, independentTag, targetPoint, currentY, offset, angle);
                        }
                        else if (tag is SpatialElementTag spatialTag)
                        {
                            if (!spatialTag.HasLeader)
                            {
                                // Enable the leader line
                                spatialTag.HasLeader = true;
                            }
                            AlignSpatialTag(doc, spatialTag, targetPoint, currentY, offset, angle);
                        }

                        // Update the Y coordinate for the next tag
                        currentY -= offset;
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    MessageBox.Show($"An error occurred while aligning and moving elements: {ex.Message}");
                }
            }
        }

        private void SetTagToFreeEnd(Document doc, IndependentTag tag)
        {
            if (!tag.HasLeader)
            {
                // Enable the leader line
                tag.HasLeader = true;
            }
            // Check if the tag's leader can be set to free end
            if (tag.HasLeader && tag.LeaderEndCondition != LeaderEndCondition.Free)
            {
                // Set the leader end to free end
                tag.LeaderEndCondition = LeaderEndCondition.Free;
            }
        }

        private void AlignIndependentTag(Document doc, IndependentTag tag, XYZ targetPoint, double currentY, double offset, double angle)
        {
            // Check if the tag's leader is visible and set to free end
            if (tag.HasLeader && tag.LeaderEndCondition == LeaderEndCondition.Free)
            {
                // Set new position with offset
                XYZ currentLeaderHead = tag.TagHeadPosition;
                XYZ newLeaderHead = new XYZ(targetPoint.X, currentY, currentLeaderHead.Z);
                tag.TagHeadPosition = newLeaderHead;

                // Adjust the leader elbow based on the user-selected angle
                Reference tagReference = tag.GetTaggedReferences().FirstOrDefault();
                if (tagReference != null)
                {
                    // Retrieve the current leader end position (fixed)
                    XYZ leaderEnd = GetLeaderEndPoint(tag);

                    // Calculate the new elbow position
                    XYZ elbowPosition = CalculateElbowPosition(newLeaderHead, leaderEnd, angle, offset);

                    // Set the leader elbow position
                    SetLeaderElbow(tag, tagReference, elbowPosition);
                }
            }
        }

        private void AlignSpatialTag(Document doc, SpatialElementTag spatialTag, XYZ targetPoint, double currentY, double offset, double angle)
        {
            // Assuming 'spatialTag' is your SpatialElementTag element
            ElementId typeId = spatialTag.GetTypeId();
            FamilySymbol tagType = spatialTag.Document.GetElement(typeId) as FamilySymbol;

            if (tagType != null)
            {
                string tagTypeName = tagType.Name;

                if (spatialTag.Location is LocationPoint locationPoint)
                {
                    // Adjust currentY based on the tag type
                    if (tagTypeName == "Room Tag" ||
            tagTypeName == "Space Tag" ||
            tagTypeName == "Area Tag" ||
            tagTypeName == "Keynote Tag" ||
            tagTypeName == "Material Tag" ||
            tagTypeName == "Multi-Category Tag")
                    {
                        currentY -= 1;
                    }
                    // Move the element to the new position with offset
                    XYZ newPoint = new XYZ(targetPoint.X, currentY, locationPoint.Point.Z);
                    locationPoint.Point = newPoint;
                }
            }
            else if (spatialTag.Location is LocationCurve locationCurve)
            {
                Curve curve = locationCurve.Curve;
                if (curve is Line line)
                {
                    // Move the line by adjusting its start and end points
                    XYZ startPoint = line.GetEndPoint(0);
                    XYZ endPoint = line.GetEndPoint(1);
                    XYZ newStartPoint = new XYZ(targetPoint.X, targetPoint.Y, startPoint.Z);
                    XYZ newEndPoint = new XYZ(targetPoint.X, targetPoint.Y, endPoint.Z);
                    locationCurve.Curve = Line.CreateBound(newStartPoint, newEndPoint);
                }
            }

            // Toggle the leader line visibility and adjust the elbow
            if (spatialTag.HasLeader)
            {
                // Toggle leader visibility off and then on
                spatialTag.HasLeader = false;
                spatialTag.HasLeader = true;

                // Attempt to set the leader elbow
                try
                {
                    // Ensure the tag has a leader
                    if (!spatialTag.HasLeader)
                    {
                        spatialTag.HasLeader = true;
                    }

                    // Retrieve the current leader end position
                    XYZ leaderEnd = spatialTag.LeaderEnd;

                    // Calculate the new elbow position
                    XYZ newLeaderHead = spatialTag.TagHeadPosition;
                    XYZ elbowPosition = CalculateElbowPosition(newLeaderHead, leaderEnd, angle, offset);
                    double modifiedY = elbowPosition.Y;
                    if (tagType != null)
                    {
                        string tagTypeName = tagType.Name;
                        if (tagTypeName == "Room Tag" ||
        tagTypeName == "Space Tag" ||
        tagTypeName == "Area Tag" ||
        tagTypeName == "Keynote Tag" ||
        tagTypeName == "Material Tag" ||
        tagTypeName == "Multi-Category Tag")
                        {
                                modifiedY = elbowPosition.Y+1.25; // Example modification elbowPosition.Y+1.25
                            }
                    }
                    
                    XYZ modifiedElbowPosition = new XYZ(elbowPosition.X, modifiedY, elbowPosition.Z);

                    // Set the leader elbow position
                    spatialTag.LeaderElbow = modifiedElbowPosition;
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private XYZ GetLeaderEndPoint(IndependentTag tag)
        {
            Reference tagReference = tag.GetTaggedReferences().FirstOrDefault();
            if (tagReference != null)
            {
                return tag.GetLeaderEnd(tagReference);
            }
            return XYZ.Zero; // Default value if no leader end is found
        }

        private XYZ CalculateElbowPosition(XYZ leaderHead, XYZ leaderEnd, double angle, double offset)
        {
            // Calculate the new elbow position based on the angle
            double angleInRadians = angle * Math.PI / 180.0;
            double deltaX, deltaY;

            if (angle == 90)
            {
                // Right angle: make the elbow vertical or horizontal
                if (Math.Abs(leaderHead.X - leaderEnd.X) < Math.Abs(leaderHead.Y - leaderEnd.Y))
                {
                    // Horizontal alignment
                    deltaX = leaderEnd.X - leaderHead.X;
                    deltaY = 0;
                }
                else
                {
                    // Horizontal alignment
                    deltaX = leaderEnd.X - leaderHead.X;
                    deltaY = 0;
                }
            }
            else if (angle == 45)
            {
                if ((leaderEnd.X - leaderHead.X) > 0)
                {
                    // Horizontal alignment
                    deltaX = Math.Abs(leaderHead.X - leaderEnd.X) - Math.Abs(leaderHead.Y - leaderEnd.Y);
                    deltaY = 0;
                }
                else
                {
                    // Horizontal alignment
                    deltaX = (leaderEnd.X - leaderHead.X) + Math.Abs(leaderHead.Y - leaderEnd.Y);
                    deltaY = 0;
                }
            }
            else
            {
                // Handle other angles if needed
                deltaX = Math.Cos(angleInRadians) * offset;
                deltaY = Math.Sin(angleInRadians) * offset;
            }

            return new XYZ(leaderHead.X + deltaX, leaderHead.Y + deltaY, leaderHead.Z);
        }

        private void SetLeaderElbow(IndependentTag tag, Reference tagReference, XYZ elbowPosition)
        {
            // Set the leader elbow to the specified position within the existing transaction
            tag.SetLeaderElbow(tagReference, elbowPosition);
        }
    }

    // Custom selection filter to only allow specified tag categories
    public class TagSelectionFilter : ISelectionFilter
    {
        private readonly HashSet<BuiltInCategory> _categories;

        public TagSelectionFilter(params BuiltInCategory[] categories)
        {
            _categories = new HashSet<BuiltInCategory>(categories);
        }

        public bool AllowElement(Element element)
        {
            if (element == null) return false;

            Category category = element.Category;
            return category != null && _categories.Contains((BuiltInCategory)category.Id.IntegerValue);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}