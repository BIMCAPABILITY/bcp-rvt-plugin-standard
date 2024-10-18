using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Text;

namespace bimkit
{
    public partial class isolateUI : Window, INotifyPropertyChanged
    {
        public void SetTitleBarImage()
        {
            // Define the base path for the plugin resources
            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "BIMKIT.bundle", "Contents", "Resources");
            string imagePath = Path.Combine(basePath, "bimcap2.png");

            if (File.Exists(imagePath))
            {
                TitleBarImage.Source = new BitmapImage(new Uri(imagePath));
            }
            else
            {
                // Handle the case where the image file does not exist
                MessageBox.Show("Image file not found: " + imagePath);
            }
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private ExternalCommandData _commandData;
        public List<WarningInfo> Warnings { get; set; }
        private List<int> selectedElementIds;

        private int _totalElementsToIsolate;
        private int _totalElementsToIsolateMulti;
        public int TotalElementsToIsolate
        {
            get { return _totalElementsToIsolate; }
            set
            {
                _totalElementsToIsolate = value;
                OnPropertyChanged(nameof(TotalElementsToIsolate));
            }
        }
        public int TotalElementsToIsolateMulti
        {
            get { return _totalElementsToIsolateMulti; }
            set
            {
                _totalElementsToIsolateMulti = value;
                OnPropertyChanged(nameof(TotalElementsToIsolateMulti));
            }
        }

        public isolateUI(ExternalCommandData commandData)
        {
            InitializeComponent();
            DataContext = new ViewModel(); // Set the DataContext for data binding
            _commandData = commandData;
            FetchWarnings();
            PopulateWarningTypes();
        }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FetchWarnings()
        {
            // Get the current document and UI document
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = uidoc.ActiveView;

            // Fetch warnings from the current view
            Warnings = GetWarningsFromView(doc, activeView);

            // Populate the CheckBoxes for warning types
            PopulateWarningTypes();
        }

        private List<WarningInfo> GetWarningsFromView(Document doc, View view)
        {
            List<WarningInfo> warningInfos = new List<WarningInfo>();
            int warningCounter = 1;

            // Get all warnings in the document
            IList<FailureMessage> warnings = doc.GetWarnings();

            foreach (FailureMessage warning in warnings)
            {
                ICollection<ElementId> failingElements = warning.GetFailingElements();
                string warningType = warning.GetDescriptionText();
                string warningId = WarningInfo.GenerateWarningId(warningType, failingElements);
                string warningNumber = $"Warning {warningCounter++}";

                foreach (ElementId elementId in failingElements)
                {
                    Element element = doc.GetElement(elementId);
                    if (element != null && (element.OwnerViewId == view.Id || element.OwnerViewId == ElementId.InvalidElementId))
                    {
                        string category = element.Category?.Name ?? "No Category";
                        warningInfos.Add(new WarningInfo
                        {
                            WarningType = warningType,
                            ElementId = elementId.IntegerValue,
                            Category = category,
                            WarningId = warningId,
                            WarningNumber = warningNumber
                        });
                    }
                }
            }

            return warningInfos;
        }

        private void PopulateWarningTypes()
        {
            var warningTypes = Warnings.GroupBy(w => w.WarningType)
                                     .Select(g => new { WarningType = g.Key, Count = g.Count() })
                                     .ToList();

            // Clear existing items
            WarningsListBox.ItemsSource = null;

            // Add items for each warning type without the count of warnings
            WarningsListBox.ItemsSource = warningTypes.Select(wt => new WarningInfo
            {
                WarningType = wt.WarningType, // Removed the count from the front
                IsChecked = false
            }).ToList();
        }

        private void FetchElementsButton_Click(object sender, RoutedEventArgs e)
        {
            // Get selected warning types
            var selectedWarningTypes = WarningsListBox.Items.Cast<WarningInfo>()
                .Where(warning => warning.IsChecked)
                .Select(warning =>
                {
                    var parts = warning.WarningType.Split(new[] { " - " }, StringSplitOptions.None);
                    return parts.Length > 1 ? parts[1] : parts[0]; // Ensure there are at least two parts
                })
                .ToList();

            if (!selectedWarningTypes.Any())
            {
                MessageBox.Show("Please select at least one warning type.");
                return;
            }

            // Clear the list of selected element IDs before each operation
            selectedElementIds = new List<int>();

            foreach (var warningType in selectedWarningTypes)
            {
                selectedElementIds.AddRange(Warnings.Where(w => w.WarningType == warningType).Select(w => w.ElementId));
            }

            // Update the total count of elements to be isolated
            TotalElementsToIsolate = selectedElementIds.Count;

            //MessageBox.Show($"Elements associated with the selected warnings: {string.Join(", ", selectedElementIds)}");

            // Get the ViewModel from the DataContext
            var viewModel = (ViewModel)this.DataContext;

            // Set the ElementCount in the ViewModel
            viewModel.ElementCount = TotalElementsToIsolate;

            // Perform the action based on the selected checkbox
            if (IsolateInCurrentViewCheckBox.IsChecked == true)
            {
                IsolateElementsInCurrentView();
            }
            else if (CreateSingleViewCheckBox.IsChecked == true)
            {
                CreateNewSingleViewAndIsolateElements();
            }
            else if (CreateMultipleViewsCheckBox.IsChecked == true)
            {
                CreateMultipleViewsAndIsolateElements();
            }
            //else if (ColorOverrideCheckBox.IsChecked == true)
            //{
            //    ColorOverrideElementsInNewView();
            //}
            else
            {
                MessageBox.Show("Please select an action to perform.");
            }
        }

        private void IsolateElementsInCurrentView()
        {
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = uidoc.ActiveView;

            try
            {
                using (Transaction trans = new Transaction(doc, "Isolate Elements in Current View"))
                {
                    trans.Start();

                    // Reset the view's isolation state
                    ResetViewIsolation(activeView);

                    // Isolate the elements in the current view
                    IsolateElementsInView(doc, activeView, selectedElementIds);

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void CreateNewSingleViewAndIsolateElements()
        {
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = uidoc.ActiveView;

            try
            {
                using (Transaction trans = new Transaction(doc, "Create New Single View and Hide Other Elements"))
                {
                    trans.Start();

                    // Create a new view (e.g., a duplicate of the current view)
                    View newView = DuplicateView(doc, activeView);

                    // Get the ViewModel from the DataContext
                    var viewModel = (ViewModel)this.DataContext;

                    // Rename the new view according to the format "prefix Isolated Elements suffix elementCount"
                    string newViewName = viewModel.GenerateViewName("Isolated", "Elements");
                    newView.Name = newViewName;

                    // Hide elements that do not have the specified warning
                    HideOtherElementsInView(doc, newView, selectedElementIds);

                    trans.Commit();

                    // Switch to the new view
                    uidoc.ActiveView = newView;
                }
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException ex) when (ex.Message.Contains("Name must be unique"))
            {
                MessageBox.Show("A view with the same name already exists. Please try again with a different prefix or suffix to create a unique view name.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }

        private void HideOtherElementsInView(Document doc, View view, List<int> elementIdsToKeep)
        {
            // Get all elements in the view
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id);
            ICollection<ElementId> allElementIds = collector.ToElementIds();

            // Determine which elements to hide
            ICollection<ElementId> idsToHide = allElementIds
                .Except(elementIdsToKeep.Select(id => new ElementId(id)))
                .ToList();

            // Hide the elements in the view
            foreach (ElementId id in idsToHide)
            {
                try
                {
                    view.HideElements(new List<ElementId> { id });
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    Debug.WriteLine($"Could not hide element with ID {id}: {ex.Message}");
                }
            }
        }

        private void CreateMultipleViewsAndIsolateElements()
        {
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = uidoc.ActiveView;

            // Get selected warning types
            var selectedWarningTypes = WarningsListBox.Items.Cast<WarningInfo>()
                .Where(warning => warning.IsChecked)
                .Select(warning => warning.WarningType) // Use the exact warning type
                .ToList();

            try
            {
                using (Transaction trans = new Transaction(doc, "Create Multiple Views and Hide Other Elements"))
                {
                    trans.Start();

                    // Group elements by warning type
                    var elementsGroupedByWarningType = selectedElementIds
                        .GroupBy(id => GetWarningTypeForElement(doc, id))
                        .ToDictionary(g => g.Key, g => g.ToList());

                    // Create views for each selected warning type
                    foreach (var warningType in selectedWarningTypes)
                    {

                        // Check if there are elements for the current warning type
                        if (!elementsGroupedByWarningType.TryGetValue(warningType, out var elements))
                        {
                            MessageBox.Show($"No elements found for warning type: {warningType}");
                            continue; // Skip to the next warning type
                        }

                        // Clear the list of selected element IDs before each operation
                        selectedElementIds = new List<int>();

                        selectedElementIds.AddRange(Warnings.Where(w => w.WarningType == warningType).Select(w => w.ElementId));

                        // Update the total count of elements to be isolated
                        TotalElementsToIsolateMulti = selectedElementIds.Count;



                        // Create a new view (e.g., a duplicate of the current view)
                        View newView = DuplicateView(doc, activeView);

                        // Get the ViewModel from the DataContext
                        var viewModel = (ViewModel)this.DataContext;

                        // Set the ElementCount in the ViewModel
                        viewModel.ElementCountMulti = TotalElementsToIsolateMulti;
                        if (newView != null)
                        {
                            // Rename the new view according to the warning type name, keeping only the first line up to the first period
                            string newViewName = $"{viewModel.GenerateViewNameMultiPre()}{GetFormattedWarningTypeName(warningType)}{viewModel.GenerateViewNameMultiSuf()}";
                            newView.Name = newViewName;

                            // Hide elements that are not associated with the warning type
                            HideOtherElementsInView(doc, newView, elements);
                        }
                        else
                        {
                            MessageBox.Show($"Failed to duplicate the view for warning type: {warningType}");
                        }
                    }

                    trans.Commit();
                }
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException ex) when (ex.Message.Contains("Name must be unique"))
            {
                MessageBox.Show("A view with the same name already exists. Please try again with a different prefix or suffix to create a unique view name.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }

        private string GetWarningTypeForElement(Document doc, int elementId)
        {
            // Find the warning type for the given element from the Warnings list
            var warning = Warnings.FirstOrDefault(w => w.ElementId == elementId);
            return warning?.WarningType ?? "Unknown";
        }

        private string GetFormattedWarningTypeName(string warningType)
        {
            // Keep only the first line up to the first period
            int periodIndex = warningType.IndexOf('.');
            if (periodIndex != -1)
            {
                return warningType.Substring(0, periodIndex);
            }
            return warningType;
        }

        private View DuplicateView(Document doc, View view)
        {
            // Duplicate the current view
            ElementId duplicatedViewId = view.Duplicate(ViewDuplicateOption.Duplicate);
            return doc.GetElement(duplicatedViewId) as View;
        }

        private void IsolateElementsInView(Document doc, View view, List<int> elementIds)
        {
            ICollection<ElementId> idsToIsolate = elementIds.Select(id => new ElementId(id)).ToList();

            // Temporarily isolate the elements in the view
            view.IsolateElementsTemporary(idsToIsolate);
        }

        private void ResetViewIsolation(View view)
        {
            // Reset the view's isolation state
            view.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
        }

        private void ColorOverrideElementsInNewView()
        {
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = uidoc.ActiveView;

            try
            {
                using (Transaction trans = new Transaction(doc, "Create New View and Color Override Elements"))
                {
                    trans.Start();


                    // Create a new view (e.g., a duplicate of the current view)
                    View newView = DuplicateView(doc, activeView);

                    // Get the ViewModel from the DataContext
                    var viewModel = (ViewModel)this.DataContext;

                    // Rename the new view according to the format "prefix Color Override Elements suffix elementCount"
                    string newViewName = viewModel.GenerateViewName("Colour", "Override");
                    newView.Name = newViewName;

                    if (newView == null)
                    {
                        throw new InvalidOperationException("Failed to create a new view.");
                    }

                    // Apply color override to the selected elements in the new view
                    OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();
                    Color redColor = new Color(255, 0, 0); // Red color

                    // Set the projection line color
                    overrideSettings.SetProjectionLineColor(redColor);

                    // Set the surface foreground pattern color and pattern id for solid fill
                    overrideSettings.SetSurfaceForegroundPatternColor(redColor);
                    overrideSettings.SetSurfaceForegroundPatternId(GetSolidFillPatternId(doc));

                    foreach (int elementId in selectedElementIds)
                    {
                        ElementId id = new ElementId(elementId);
                        newView.SetElementOverrides(id, overrideSettings);
                    }

                    trans.Commit();

                    // Switch to the new view
                    uidoc.ActiveView = newView;
                }

                MessageBox.Show("New view created and color overrides applied.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private ElementId GetSolidFillPatternId(Document doc)
        {
            // Get the solid fill pattern id
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FillPatternElement solidFillPattern = collector.OfClass(typeof(FillPatternElement))
                                                         .Cast<FillPatternElement>()
                                                         .FirstOrDefault(fp => fp.GetFillPattern().IsSolidFill);

            return solidFillPattern?.Id ?? ElementId.InvalidElementId;
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (WarningInfo warning in WarningsListBox.Items)
            {
                warning.IsChecked = true;
            }
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (WarningInfo warning in WarningsListBox.Items)
            {
                warning.IsChecked = false;
            }
        }

        private void SetupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Ensure only one checkbox is checked at a time
            if (sender is CheckBox checkBox)
            {
                if (checkBox == IsolateInCurrentViewCheckBox)
                {
                    CreateSingleViewCheckBox.IsChecked = false;
                    CreateMultipleViewsCheckBox.IsChecked = false;
                    //ColorOverrideCheckBox.IsChecked = false;
                }
                else if (checkBox == CreateSingleViewCheckBox)
                {
                    IsolateInCurrentViewCheckBox.IsChecked = false;
                    CreateMultipleViewsCheckBox.IsChecked = false;
                    //ColorOverrideCheckBox.IsChecked = false;
                }
                else if (checkBox == CreateMultipleViewsCheckBox)
                {
                    IsolateInCurrentViewCheckBox.IsChecked = false;
                    CreateSingleViewCheckBox.IsChecked = false;
                    //ColorOverrideCheckBox.IsChecked = false;
                }
                //else if (checkBox == ColorOverrideCheckBox)
                //{
                //    IsolateInCurrentViewCheckBox.IsChecked = false;
                //    CreateSingleViewCheckBox.IsChecked = false;
                //    CreateMultipleViewsCheckBox.IsChecked = false;
                //}
            }
        }

        private void SetupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // No specific action needed when a checkbox is unchecked
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

        private void ResetColor_Click(object sender, RoutedEventArgs e)
        {
            UIDocument uidoc = _commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = uidoc.ActiveView;

            try
            {
                using (Transaction trans = new Transaction(doc, "Reset Color Overrides"))
                {
                    trans.Start();

                    // Get all elements in the current view
                    FilteredElementCollector collector = new FilteredElementCollector(doc, activeView.Id);
                    ICollection<ElementId> allElementIds = collector.ToElementIds();

                    // Reset color overrides for all elements in the current view
                    OverrideGraphicSettings resetOverrideSettings = new OverrideGraphicSettings();

                    foreach (ElementId id in allElementIds)
                    {
                        activeView.SetElementOverrides(id, resetOverrideSettings);
                    }

                    trans.Commit();
                }

                MessageBox.Show("Color overrides have been reset for all elements in the current view.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }

    public class WarningInfo : INotifyPropertyChanged
    {
        private bool _isChecked;
        public string WarningType { get; set; }
        public int ElementId { get; set; }
        public string Category { get; set; }
        public string WarningId { get; set; } // Unique identifier for the warning
        public string WarningNumber { get; set; } // Specific warning number (e.g., Warning 1, Warning 2)
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        // Method to generate a unique identifier for the warning
        public static string GenerateWarningId(string warningType, ICollection<ElementId> elementIds)
        {
            var sortedElementIds = elementIds.Select(id => id.IntegerValue).OrderBy(id => id);
            return $"{warningType}_{string.Join("_", sortedElementIds)}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ViewModel : INotifyPropertyChanged
    {
        private string _prefix;
        private string _suffix;
        private string _separator;
        private int _elementCount;
        private int _elementCountMulti;
        private string _singleViewNamePreview="Single View Name Preview";
        private string _multipleViewNamePreview="Multiple View Name Preview";

        public string Prefix
        {
            get => _prefix;
            set
            {
                _prefix = value;
                OnPropertyChanged(nameof(Prefix));
                UpdatePreviews();
            }
        }

        public string Suffix
        {
            get => _suffix;
            set
            {
                _suffix = value;
                OnPropertyChanged(nameof(Suffix));
                UpdatePreviews();
            }
        }

        public string Separator
        {
            get => _separator;
            set
            {
                _separator = value;
                OnPropertyChanged(nameof(Separator));
                UpdatePreviews();
            }
        }

        public int ElementCount
        {
            get => _elementCount;
            set
            {
                _elementCount = value;
                OnPropertyChanged(nameof(ElementCount));
                UpdatePreviews();
            }
        }

        public int ElementCountMulti
        {
            get => _elementCountMulti;
            set
            {
                _elementCountMulti = value;
                OnPropertyChanged(nameof(ElementCountMulti));
                UpdatePreviews();
            }
        }

        public string SingleViewNamePreview
        {
            get => _singleViewNamePreview;
            private set
            {
                _singleViewNamePreview = value;
                OnPropertyChanged(nameof(SingleViewNamePreview));
            }
        }

        public string MultipleViewNamePreview
        {
            get => _multipleViewNamePreview;
            private set
            {
                _multipleViewNamePreview = value;
                OnPropertyChanged(nameof(MultipleViewNamePreview));
            }
        }

        public string GenerateViewName(string description, string description2)
        {
            StringBuilder viewName = new StringBuilder();

            if (!string.IsNullOrEmpty(Prefix))
            {
                viewName.Append(Prefix);
                if (!string.IsNullOrEmpty(Separator))
                {
                    viewName.Append(Separator);
                }else
                {
                    viewName.Append(' ');
                }
            }

            viewName.Append(description);

            if (!string.IsNullOrEmpty(Separator))
            {
                viewName.Append(Separator);
            }
            else
            {
                viewName.Append(' ');
            }

            viewName.Append(description2);

            if (!string.IsNullOrEmpty(Suffix))
            {
                if (!string.IsNullOrEmpty(Separator))
                {
                    viewName.Append(Separator);
                }
                else
                {
                    viewName.Append(' ');
                }
                viewName.Append(Suffix);
            }

            if (!string.IsNullOrEmpty(Separator))
            {
                viewName.Append(Separator);
            }
            else
            {
                viewName.Append(' ');
            }

            viewName.Append(ElementCount);

            return viewName.ToString();
        }

        public string GenerateViewNameMultiPre()
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                if (!string.IsNullOrEmpty(Separator))
                {
                    return $"{Prefix}{Separator}";
                }
                else
                {
                    return $"{Prefix} ";
                }
            }
            return string.Empty;
        }

        public string GenerateViewNameMultiSuf()
        {
            StringBuilder result = new StringBuilder();

            if (!string.IsNullOrEmpty(Suffix))
            {
                if (!string.IsNullOrEmpty(Separator))
                {
                    result.Append($"{Separator}{Suffix}");
                }
                else
                {
                    result.Append($" {Suffix}");
                }
            }

            if (!string.IsNullOrEmpty(Separator))
            {
                result.Append($"{Separator}{ElementCountMulti}");
            }
            else
            {
                result.Append($" {ElementCountMulti}");
            }

            return result.ToString();
        }

        private void UpdatePreviews()
        {
            SingleViewNamePreview = GenerateViewName("Isolated", "Elements");
            MultipleViewNamePreview = $"{GenerateViewNameMultiPre()}WarningName{GenerateViewNameMultiSuf()}";
        }

        public void CreateSingleView()
        {
            string description = "Isolated";
            string description2 = "Elements";
            string viewName = GenerateViewName(description, description2);
            // Logic to create the view with the name `viewName`
        }

        public void UpdateElementCount(int count)
        {
            ElementCount = count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}