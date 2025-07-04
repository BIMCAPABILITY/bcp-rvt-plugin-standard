using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Windows;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace bimkit
{
    public class bimkit : IExternalApplication
    {
        private string GetRevitVersion(ControlledApplication app)
        {
            return app.VersionNumber;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "BIMKIT";
            string panelName = "About";
            string panelName2 = "Warning tools";
            string panelName3 = "Sheet tools";
            string revitVersion = GetRevitVersion(application.ControlledApplication);

            // Define the base path for the plugin resources
            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autodesk", "ApplicationPlugins", "BIMKIT.bundle", "Contents", "Resources");

            // Load the images using the new base path
            BitmapImage btn1image = new BitmapImage(new Uri(Path.Combine(basePath, "plugin.png")));
            BitmapImage btn2image = new BitmapImage(new Uri(Path.Combine(basePath, "info.png")));
            BitmapImage btn3image = new BitmapImage(new Uri(Path.Combine(basePath, "bimcap.jpg")));
            BitmapImage btn4image = new BitmapImage(new Uri(Path.Combine(basePath, "plugin2.png")));
            BitmapImage btn5image = new BitmapImage(new Uri(Path.Combine(basePath, "TitleBlockEditor.png")));
            BitmapImage btn6image = new BitmapImage(new Uri(Path.Combine(basePath, "GridBubbleToggle.png")));
            BitmapImage btn7image = new BitmapImage(new Uri(Path.Combine(basePath, "GridDimensionAlign.png")));
            BitmapImage btn8image = new BitmapImage(new Uri(Path.Combine(basePath, "autoTag.png")));

            // tab ->
            application.CreateRibbonTab(tabName);
            // panel ->
            var pluginPanel = application.CreateRibbonPanel(tabName, panelName);
            var pluginPanel2 = application.CreateRibbonPanel(tabName, panelName2);
            var pluginPanel3 = application.CreateRibbonPanel(tabName, panelName3);

            // plugin button ->
            var pluginBtn = new PushButtonData("Isolate Warning Elements", "Isolate\nWarnings", Assembly.GetExecutingAssembly().Location, "bimkit.ShowIsolateUI");
            pluginBtn.ToolTip = "Isolate Warning Elements";
            pluginBtn.LongDescription = "Isolate elements in your Revit model that have specific warnings. This tool helps you quickly identify and focus on problematic areas, allowing for efficient troubleshooting and resolution of issues.";
            pluginBtn.LargeImage = btn1image;
            pluginBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/RVT/en/Detail/HelpDoc?appId=7415951906221934357&appLang=en&os=Win64&mode=preview"));

            var pluginBtn2 = new PushButtonData("Align Tags", "Align\n     Tags     ", Assembly.GetExecutingAssembly().Location, "bimkit.ShowAlignTagUI");
            pluginBtn2.ToolTip = "Align Tags";
            pluginBtn2.LongDescription = "Align tags in your Revit model to improve clarity and presentation. This tool ensures that tags are neatly arranged, enhancing the visual organization of your drawings and documentation.";
            pluginBtn2.LargeImage = btn4image;
            pluginBtn2.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/RVT/en/Detail/HelpDoc?appId=7415951906221934357&appLang=en&os=Win64&mode=preview"));

            var pluginBtn3 = new PushButtonData("Select Title-blocks", "Select\nTitle-blocks", Assembly.GetExecutingAssembly().Location, "bimkit.ShowTitleBlockUI");
            pluginBtn3.ToolTip = "Select Title-blocks";
            pluginBtn3.LongDescription = "Quickly select title blocks in your Revit project. This tool simplifies workflows by allowing easy selection and management of multiple title blocks at once.";
            pluginBtn3.LargeImage = btn5image;
            pluginBtn3.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/RVT/en/Detail/HelpDoc?appId=7415951906221934357&appLang=en&os=Win64&mode=preview"));

            var pluginBtn4 = new PushButtonData("Toggle Grid Bubbles", "Toggle\nGrid Bubbles", Assembly.GetExecutingAssembly().Location, "bimkit.ShowGridBubbleUI");
            pluginBtn4.ToolTip = "Toggle Grid Bubbles";
            pluginBtn4.LongDescription = "Toggle the visibility of grid bubbles in your project. This tool helps maintain a cleaner and more organized view during documentation.";
            pluginBtn4.LargeImage = btn6image;
            pluginBtn4.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/RVT/en/Detail/HelpDoc?appId=7415951906221934357&appLang=en&os=Win64&mode=preview"));

            var pluginBtn5 = new PushButtonData("Align Grid Dimension", "Align Grid\nDimensions", Assembly.GetExecutingAssembly().Location, "bimkit.sheet_tools.gridDimensionAlign_feature.gridDimensionAlign");
            pluginBtn5.ToolTip = "Align Grid Dimensions";
            pluginBtn5.LongDescription = "Align grid dimensions in your Revit model for improved accuracy and consistent presentation of drawings.";
            pluginBtn5.LargeImage = btn7image;
            pluginBtn5.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/RVT/en/Detail/HelpDoc?appId=7415951906221934357&appLang=en&os=Win64&mode=preview"));

            var pluginBtn6 = new PushButtonData("Auto Tag", "Auto\n     Tags     ", Assembly.GetExecutingAssembly().Location, "bimkit.sheet_tools.autoTag_feature.autoTag");
            pluginBtn6.ToolTip = "Auto Tag";
            pluginBtn6.LongDescription = "Automatically generate annotation tags for supported categories and resolve 2D tag clashes in plan views. This tool intelligently detects overlapping tag positions and repositions them with minimal conflict. Currently supports 2D tag placement only — 3D tag clash resolution is under development.";
            pluginBtn6.LargeImage = btn8image;
            pluginBtn6.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/RVT/en/Detail/HelpDoc?appId=7415951906221934357&appLang=en&os=Win64&mode=preview"));

            // add button to panel ->
            var btn1 = pluginPanel2.AddItem(pluginBtn) as PushButton;
            var btn2 = pluginPanel3.AddItem(pluginBtn2) as PushButton;
            var btn6 = pluginPanel3.AddItem(pluginBtn6) as PushButton;
            var btn3 = pluginPanel3.AddItem(pluginBtn3) as PushButton;
            var btn4 = pluginPanel3.AddItem(pluginBtn4) as PushButton;
            var btn5 = pluginPanel3.AddItem(pluginBtn5) as PushButton;

            // stacked buttons ->
            var aboutBimcapBtn = new PushButtonData("aboutBimcapBtn", "BIMCAP", Assembly.GetExecutingAssembly().Location, "bimkit.about_tab.OpenWebsiteCommand");
            aboutBimcapBtn.ToolTip = "Bimcap";
            aboutBimcapBtn.LongDescription = "Visit the BIMCAP website to learn more about our services, solutions, and how we can assist with your BIM projects. Click this button to open the BIMCAP website in your default web browser.";
            aboutBimcapBtn.Image = btn3image;
            var aboutPluginBtn = new PushButtonData("aboutPluginBtn", "Version", Assembly.GetExecutingAssembly().Location, "bimkit.about_tab.OpenVersionCommand");
            aboutPluginBtn.ToolTip = "Plugin Version";
            aboutPluginBtn.LongDescription = "View detailed information about the current version of the BIMKIT plugin. This includes version number, release notes, and any updates or changes made in this version. Click this button to access version details.";
            aboutPluginBtn.Image = btn2image;

            pluginPanel.AddStackedItems(aboutBimcapBtn, aboutPluginBtn);

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
