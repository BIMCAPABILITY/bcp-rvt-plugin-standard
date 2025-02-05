using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Autodesk.Revit.DB;

namespace bimkit.sheet_tools.gridBubbleToggle_feature
{
    public partial class GridActionWindow : Window
    {
        private List<Grid> _grids;
        private Document _doc;

        public GridActionWindow(List<Grid> grids, Document doc)
        {
            InitializeComponent();
            _grids = grids;
            _doc = doc;
            TotalGridsTextBlock.Text = $"Total Grids Selected: {_grids.Count}";
            SetTitleBarImage();
        }

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

        // Event handlers for button clicks
        private void ShowBubbles_Click(object sender, RoutedEventArgs e)
        {
            ModifyGridBubbles(DatumEnds.End0, DatumEnds.End1, true, true);
        }
        private void HideBubbles_Click(object sender, RoutedEventArgs e)
        {
            ModifyGridBubbles(DatumEnds.End0, DatumEnds.End1, false, false);
        }
        private void ModifyGridBubbles(DatumEnds end0, DatumEnds end1, bool showEnd0, bool showEnd1)
        {
            using (Transaction t = new Transaction(_doc, "Modify Grid Bubbles"))
            {
                t.Start();

                foreach (var grid in _grids)
                {
                    if (showEnd0)
                        grid.ShowBubbleInView(end0, _doc.ActiveView);
                    else
                        grid.HideBubbleInView(end0, _doc.ActiveView);

                    if (showEnd1)
                        grid.ShowBubbleInView(end1, _doc.ActiveView);
                    else
                        grid.HideBubbleInView(end1, _doc.ActiveView);
                }

                t.Commit();
            }
        }


        private void ShowLeftBottomBubbles_Click(object sender, RoutedEventArgs e)
        {
            ModifyGridBubblesForLeftBottom();
        }

        private void ShowRightTopBubbles_Click(object sender, RoutedEventArgs e)
        {
            ModifyGridBubblesForRightTop();
        }

        private void ModifyGridBubblesForRightTop()
        {
            using (Transaction t = new Transaction(_doc, "Show Right Top Bubbles"))
            {
                t.Start();

                foreach (var grid in _grids)
                {
                    Line gridLine = (grid.Curve as Line);
                    if (gridLine != null)
                    {
                        XYZ startPoint = gridLine.GetEndPoint(0);
                        XYZ endPoint = gridLine.GetEndPoint(1);

                        // Determine which end is right and top
                        //if (startPoint.X > endPoint.X || startPoint.Y > endPoint.Y)
                        if (startPoint.X < endPoint.X)
                        {
                            grid.ShowBubbleInView(DatumEnds.End0, _doc.ActiveView); // Show bubble at End0 (right/top)
                            grid.HideBubbleInView(DatumEnds.End1, _doc.ActiveView);
                        }
                        else
                        {
                            grid.ShowBubbleInView(DatumEnds.End1, _doc.ActiveView); // Show bubble at End1 (right/top)
                            grid.HideBubbleInView(DatumEnds.End0, _doc.ActiveView);
                        }
                    }
                }

                t.Commit();
            }
        }

        private void ModifyGridBubblesForLeftBottom()
        {
            using (Transaction t = new Transaction(_doc, "Show Left Bottom Bubbles"))
            {
                t.Start();

                foreach (var grid in _grids)
                {
                    Line gridLine = grid.Curve as Line;
                    if (gridLine != null)
                    {
                        XYZ startPoint = gridLine.GetEndPoint(0);
                        XYZ endPoint = gridLine.GetEndPoint(1);

                        // Determine which end is left and bottom
                        //if (startPoint.X < endPoint.X || startPoint.Y < endPoint.Y)
                        if (startPoint.X < endPoint.X)
                        {
                            grid.ShowBubbleInView(DatumEnds.End1, _doc.ActiveView); // Show bubble at End0 (left/bottom)
                            grid.HideBubbleInView(DatumEnds.End0, _doc.ActiveView);
                        }
                        else
                        {
                            grid.ShowBubbleInView(DatumEnds.End0, _doc.ActiveView); // Show bubble at End1 (left/bottom)
                            grid.HideBubbleInView(DatumEnds.End1, _doc.ActiveView);
                        }
                    }
                }

                t.Commit();
            }
        }
    }
}