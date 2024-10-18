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

            // tab ->
            application.CreateRibbonTab(tabName);
            // panel ->
            var pluginPanel = application.CreateRibbonPanel(tabName, panelName);
            var pluginPanel2 = application.CreateRibbonPanel(tabName, panelName2);
            var pluginPanel3 = application.CreateRibbonPanel(tabName, panelName3);

            // plugin button ->
            var pluginBtn = new PushButtonData("Isolate Warning Elements", "Isolate Warnings", Assembly.GetExecutingAssembly().Location, "bimkit.ShowIsolateUI");
            pluginBtn.ToolTip = "Isolate Warning Elements";
            pluginBtn.LongDescription = "Isolate elements in your Revit model that have specific warnings. This tool helps you quickly identify and focus on problematic areas, allowing for efficient troubleshooting and resolution of issues.";
            pluginBtn.LargeImage = btn1image;
            pluginBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/RVT/en/Detail/HelpDoc?appId=7415951906221934357&appLang=en&os=Win64&mode=preview"));

            var pluginBtn2 = new PushButtonData("Align Tags", "Align Tags", Assembly.GetExecutingAssembly().Location, "bimkit.ShowAlignTagUI");
            pluginBtn2.ToolTip = "Align Tags";
            pluginBtn2.LongDescription = "Align tags in your Revit model to improve clarity and presentation. This tool ensures that tags are neatly arranged, enhancing the visual organization of your drawings and documentation.";
            pluginBtn2.LargeImage = btn4image;
            pluginBtn2.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/RVT/en/Detail/HelpDoc?appId=7415951906221934357&appLang=en&os=Win64&mode=preview"));

            // add button to panel ->
            var btn1 = pluginPanel2.AddItem(pluginBtn) as PushButton;
            var btn2 = pluginPanel3.AddItem(pluginBtn2) as PushButton;

            // stacked buttons ->

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
