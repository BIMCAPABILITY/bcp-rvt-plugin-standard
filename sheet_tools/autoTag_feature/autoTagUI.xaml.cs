using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace bimkit.sheet_tools.autoTag_feature
{
    public partial class autoTagUI : Window
    {
        private bool _allSelected = true;
        public List<string> SelectedCategories { get; private set; } = new List<string>();
        public int IterationCount => (int)IterationSlider.Value;

        public autoTagUI()
        {
            InitializeComponent();

            // List of default taggable categories
            List<string> defaultCategories = new List<string>
            {
                "Walls",
                "Doors",
                "Windows",
                "Floors",
                "Ceilings",
                "Structural Columns",
                "Structural Framing",
                "Pipes",
                "Ducts",
                "Cable Trays",
                "Conduits",
                "Rooms",
                "Spaces",
                "Areas"
            };

            // Populate ListBox
            foreach (string category in defaultCategories)
            {
                var checkbox = new CheckBox
                {
                    Content = category,
                    Margin = new Thickness(5, 2, 5, 2),
                    IsChecked = true
                };

                CategoryListBox.Items.Add(checkbox);
            }
            // Initial state: all selected
            _allSelected = true;
            SelectAllButton.Content = "Deselect All";
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            _allSelected = !_allSelected; // Toggle state

            foreach (var item in CategoryListBox.Items)
            {
                if (item is CheckBox checkbox)
                {
                    checkbox.IsChecked = _allSelected;
                }
            }

            // Update button label accordingly
            SelectAllButton.Content = _allSelected ? "Deselect All" : "Select All";
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void FixButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedCategories.Clear();

            foreach (var item in CategoryListBox.Items)
            {
                if (item is CheckBox checkbox && checkbox.IsChecked == true)
                {
                    SelectedCategories.Add(checkbox.Content.ToString());
                }
            }

            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
