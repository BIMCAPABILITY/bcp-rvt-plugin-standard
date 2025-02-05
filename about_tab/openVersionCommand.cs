using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace bimkit.about_tab
{
    [Transaction(TransactionMode.Manual)]
    public class OpenVersionCommand : IExternalCommand
    {
        /// <summary>
        /// Executes the command to display the plugin version details.
        /// </summary>
        /// <param name="commandData">Provides access to the Revit application and active document.</param>
        /// <param name="message">A message that can be set in case of failure.</param>
        /// <param name="elements">A set of elements that can be modified in case of failure.</param>
        /// <returns>Returns success or failure of the command.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Define the plugin version and release date
            string pluginVersion = "1.1.0"; // Update this to the actual plugin version
            string releaseDate = "25-11-2024"; // Today's date, update this for future releases

            // Define additional plugin details
            string developerInfo = "Developed by BIMKIT Team"; // Replace with your team or company name
            string contactInfo = "automation@bimcap.com"; // Replace with actual support contact details

            // Display the dialog box with plugin version details
            TaskDialog.Show("Plugin Information",
                $"Plugin Version: {pluginVersion}\n" +
                $"Release Date: {releaseDate}\n" +
                $"Developer: {developerInfo}\n" +
                $"Contact: {contactInfo}\n\n" +
                "For more details, visit our website or check the Help Document included with the plugin.");

            return Result.Succeeded;
        }
    }
}
