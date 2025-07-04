using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace bimkit.sheet_tools.titleBlockEditor_feature
{
    public partial class MainWindow : Window
    {
        private UIDocument _uidoc;
        private Document _doc;
        private List<SheetViewModel> _sheets = new List<SheetViewModel>();
        public MainWindow(UIDocument uidoc)
        {
            InitializeComponent();
            if (uidoc == null || uidoc.Document == null)
            {
                throw new ArgumentNullException("UIDocument or its Document property cannot be null.");
            }

            _uidoc = uidoc;
            _doc = uidoc.Document;
            LoadSheetsWithTitleBlocks();
            //SetTitleBarImage();
            SheetListBox.PreviewMouseWheel += SheetListBox_PreviewMouseWheel;
        }
        private void SheetListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Find the parent ScrollViewer
            var scrollViewer = FindVisualParent<ScrollViewer>(SheetListBox);
            if (scrollViewer != null)
            {
                // Scroll the ScrollViewer
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }
        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            T parent = parentObject as T;
            return parent ?? FindVisualParent<T>(parentObject);
        }
        //public void SetTitleBarImage()
        //{
        //    string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "BIMKIT.bundle", "Contents", "Resources");
        //    string imagePath = Path.Combine(basePath, "bimcap2.png");

        //    if (File.Exists(imagePath))
        //    {
        //        TitleBarImage.Source = new BitmapImage(new Uri(imagePath));
        //    }
        //    else
        //    {
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
        private void LoadSheetsWithTitleBlocks()
        {
            MessageBox.Show(
                "About to fetch some sheets, this process might take a few seconds.",
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            try
            {
                // Fetch sheets with title blocks
                var sheetsWithTitleBlocks = new FilteredElementCollector(_doc)
                    .OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .Select(sheet => new
                    {
                        Sheet = sheet,
                        TitleBlocks = new FilteredElementCollector(_doc, sheet.Id)
                            .OfCategory(BuiltInCategory.OST_TitleBlocks)
                            .WhereElementIsNotElementType()
                            .ToList()
                    })
                    .Where(x => x.TitleBlocks.Any())
                    .Select(x => new SheetViewModel
                    {
                        Id = x.Sheet.Id,
                        Name = x.Sheet.Name,
                        SheetNumber = x.Sheet.SheetNumber,
                        TitleBlockIds = x.TitleBlocks.Select(tb => tb.Id).ToList()
                    })
                    .OrderBy(x => x.SheetNumber)
                    .ToList();

                _sheets = sheetsWithTitleBlocks;

                // Populate ListBox with all sheets (ungrouped)
                SheetListBox.ItemsSource = _sheets;

                // Group sheets by the first letter of SheetNumber for dynamic buttons
                var groupedSheets = _sheets
                    .GroupBy(sheet => sheet.SheetNumber.Substring(0, 1))
                    .Select(group => new
                    {
                        Key = group.Key,
                        Sheets = group.ToList()
                    })
                    .ToList();

                // Create dynamic buttons for group selection
                CreateGroupButtons(groupedSheets);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CreateGroupButtons(IEnumerable<dynamic> groups)
        {
            GroupButtonsPanel.Children.Clear();

            foreach (var group in groups)
            {
                var button = new Button
                {
                    Content = $"Toggle Group {group.Key}",
                    Tag = group,
                    Margin = new Thickness(5, 0, 5, 0)
                };

                button.Click += (sender, e) =>
                {
                    var clickedGroup = (sender as Button)?.Tag;

                    if (clickedGroup != null)
                    {
                        // Explicitly cast the group to its concrete type
                        var sheets = (List<SheetViewModel>)clickedGroup.GetType().GetProperty("Sheets")?.GetValue(clickedGroup);

                        if (sheets != null)
                        {
                            // Check if all sheets in the group are selected
                            bool allSelected = sheets.All(sheet => sheet.IsSelected);

                            // Toggle the selection
                            foreach (var sheet in sheets)
                            {
                                sheet.IsSelected = !allSelected;
                            }

                            // Refresh the ListBox to reflect changes
                            SheetListBox.Items.Refresh();
                        }
                    }
                };

                GroupButtonsPanel.Children.Add(button);
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var sheet in _sheets)
            {
                sheet.IsSelected = true;
            }
            SheetListBox.Items.Refresh();
        }
        private void SelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var sheet in _sheets)
            {
                sheet.IsSelected = false;
            }
            SheetListBox.Items.Refresh();
        }
        private void ApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            var selectedTitleBlocks = _sheets
                .Where(sheet => sheet.IsSelected)
                .SelectMany(sheet => sheet.TitleBlockIds)
                .ToList();

            if (selectedTitleBlocks.Count == 0)
            {
                MessageBox.Show("Please select at least one title block.");
                return;
            }

            // Open the SheetPropertiesWindow to set common properties
            SheetPropertiesWindow propertiesWindow = new SheetPropertiesWindow(_doc, selectedTitleBlocks);
            propertiesWindow.ShowDialog();
            this.Close();
        }
    }

    public class SheetViewModel
    {
        public ElementId Id { get; set; } // Revit element ID
        public string Name { get; set; } // Sheet name
        public string SheetNumber { get; set; } // Sheet number
        public List<ElementId> TitleBlockIds { get; set; } // Title blocks
        public bool IsSelected { get; set; } // For individual selection
        public string DisplayName => $"{SheetNumber} - {Name}";
    }

    public class SheetGroupViewModel
    {
        public string Key { get; set; } // Group key (e.g., A, B, C)
        public List<SheetViewModel> Sheets { get; set; } // Sheets in the group

        public void SelectGroup(bool isSelected)
        {
            foreach (var sheet in Sheets)
            {
                sheet.IsSelected = isSelected;
            }
        }
    }

}