using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;

namespace bimkit.sheet_tools.titleBlockEditor_feature
{
    public partial class SheetPropertiesWindow : Window
    {
        private Document _doc;
        private List<ElementId> _selectedTitleBlockIds;
        private List<SheetProperty> _titleBlockProperties;

        public SheetPropertiesWindow(Document doc, List<ElementId> selectedTitleBlockIds)
        {
            InitializeComponent();
            _doc = doc;
            _selectedTitleBlockIds = selectedTitleBlockIds;
            LoadCommonProperties();
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

        private void LoadCommonProperties()
        {
            // Get the first title block to determine common parameters
            var firstTitleBlock = _doc.GetElement(_selectedTitleBlockIds.First()) as FamilyInstance;
            if (firstTitleBlock != null)
            {
                _titleBlockProperties = new List<SheetProperty>();

                foreach (Parameter param in firstTitleBlock.Parameters)
                {
                    // Check if the parameter is editable and common across all title blocks
                    if (IsParameterEditableAndCommon(param))
                    {
                        _titleBlockProperties.Add(new SheetProperty
                        {
                            Name = param.Definition.Name,
                            Value = param.AsValueString() ?? param.AsString() ?? string.Empty
                        });
                    }
                }

                PropertiesDataGrid.ItemsSource = _titleBlockProperties;
            }
        }

        private bool IsParameterEditableAndCommon(Parameter param)
        {
            // Exclude parameters that are not editable or are unique identifiers
            if (param.IsReadOnly || param.Definition.Name.Equals("Sheet Name") || param.Definition.Name.Equals("Sheet Number"))
            {
                return false;
            }

            // Check if the parameter is common across all selected title blocks
            foreach (var titleBlockId in _selectedTitleBlockIds)
            {
                var titleBlock = _doc.GetElement(titleBlockId) as FamilyInstance;
                if (titleBlock != null)
                {
                    var titleBlockParam = titleBlock.LookupParameter(param.Definition.Name);
                    if (titleBlockParam == null || titleBlockParam.IsReadOnly)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            using (Transaction t = new Transaction(_doc, "Update Title Block Properties"))
            {
                t.Start();

                foreach (var titleBlockId in _selectedTitleBlockIds)
                {
                    var titleBlock = _doc.GetElement(titleBlockId) as FamilyInstance;
                    if (titleBlock != null)
                    {
                        foreach (var titleBlockProperty in _titleBlockProperties)
                        {
                            Parameter param = titleBlock.LookupParameter(titleBlockProperty.Name);
                            if (param != null && !param.IsReadOnly)
                            {
                                try
                                {
                                    switch (param.StorageType)
                                    {
                                        case StorageType.String:
                                            param.Set(titleBlockProperty.Value);
                                            break;
                                        case StorageType.Integer:
                                            if (int.TryParse(titleBlockProperty.Value, out int intValue))
                                            {
                                                param.Set(intValue);
                                            }
                                            break;
                                        case StorageType.Double:
                                            if (double.TryParse(titleBlockProperty.Value, out double doubleValue))
                                            {
                                                param.Set(doubleValue);
                                            }
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Failed to update property '{titleBlockProperty.Name}' on title block: {ex.Message}");
                                }
                            }
                        }
                    }
                }

                t.Commit();
            }

            MessageBox.Show("Properties updated successfully.");
            this.Close();
        }
    }

    public class SheetProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}