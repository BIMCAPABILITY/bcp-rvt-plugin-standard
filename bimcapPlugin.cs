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

namespace bimcapPlugin
{
    public class bimcapPlugin : IExternalApplication
    {
        private string GetRevitVersion(ControlledApplication app)
        {
            return app.VersionNumber;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "BIMCAP";
            string panelName = "About";
            string panelName2 = "Management";
            string revitVersion = GetRevitVersion(application.ControlledApplication);

            // bitimages ->
            BitmapImage btn1image = new BitmapImage(new Uri(Path.Combine("C:\\", "ProgramData", "Autodesk", "Revit", "Addins", revitVersion, "Resources", "plugin.png")));
            BitmapImage btn2image = new BitmapImage(new Uri(Path.Combine("C:\\", "ProgramData", "Autodesk", "Revit", "Addins", revitVersion, "Resources", "info.png")));
            BitmapImage btn3image = new BitmapImage(new Uri(Path.Combine("C:\\", "ProgramData", "Autodesk", "Revit", "Addins", revitVersion, "Resources", "bimcap.jpg")));

            // tab ->
            application.CreateRibbonTab(tabName);
            // panel ->
            var pluginPanel = application.CreateRibbonPanel(tabName, panelName);
            var pluginPanel2 = application.CreateRibbonPanel(tabName, panelName2);
            // plugin button ->
            var pluginBtn = new PushButtonData("Isolate Elements", "Isolate by Warnings", Assembly.GetExecutingAssembly().Location, "bimcapPlugin.ShowIsolateUI");
            pluginBtn.ToolTip = "Isolate Elements";
            pluginBtn.LongDescription = "This is the main action button of this plugin, which launches a user interface to operate the plugin.";
            pluginBtn.LargeImage = btn1image;

            // add button to panel ->
            var btn1 = pluginPanel2.AddItem(pluginBtn) as PushButton;           


            // stacked buttons ->
            var aboutBimcapBtn = new PushButtonData("aboutBimcapBtn", "BIMCAP", Assembly.GetExecutingAssembly().Location, "bimcapPlugin.about_tab.OpenWebsiteCommand");
            aboutBimcapBtn.ToolTip = "Bimcap"; 
            aboutBimcapBtn.LongDescription = "This button provides detailed information about the creator of the plugin, BIMCAP";
            aboutBimcapBtn.Image = btn3image;
            var aboutPluginBtn = new PushButtonData("aboutPluginBtn", "Version", Assembly.GetExecutingAssembly().Location, "bimcapPlugin.about_tab.OpenVersionCommand");
            aboutPluginBtn.ToolTip = "Plugin Version";
            aboutPluginBtn.LongDescription = "Version and Released date of the plugin you are currently using";
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
