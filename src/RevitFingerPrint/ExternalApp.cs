using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Metamorphosis
{
    public class ExternalApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                buildUI(application);
                return Result.Succeeded;
            }
            catch (Exception eX)
            {
                TaskDialog td = new TaskDialog("Error in Setup");
                td.ExpandedContent = eX.GetType().Name + ": " + eX.Message + Environment.NewLine + eX.StackTrace;
                td.Show();
                return Result.Failed;
            }
        }

        private void buildUI(UIControlledApplication app)
        {
            var panel = app.CreateRibbonPanel(Tab.AddIns, "Metamorphosis" + Environment.NewLine + "Divergence");

            var snapshot = new PushButtonData("Snapshot", "Snapshot", System.Reflection.Assembly.GetExecutingAssembly().Location, "Metamorphosis.Snapshot");
            snapshot.ToolTip = "Take a snapshot of a model";
            snapshot.LongDescription = "Take a snapshot of a model that can be used for later comparison of this version of the model.";
            snapshot.LargeImage = getImage("Metamorphosis.Images.Export-32.png");
            snapshot.Image = getImage("Metamorphosis.Images.Export-16.png");


            panel.AddItem(snapshot);

            var comp = new PushButtonData("Compare", "Compare", System.Reflection.Assembly.GetExecutingAssembly().Location, "Metamorphosis.Compare");
            comp.ToolTip = "Compare a model against a previous model";
            comp.LongDescription = "Compare a model against a previous snapshot of the model.";
            comp.Image = getImage("Metamorphosis.Images.Compare-16.png");
            comp.LargeImage = getImage("Metamorphosis.Images.Compare-32.png");
            panel.AddItem(comp);

            var prev = new PushButtonData("Previous", "Previous", System.Reflection.Assembly.GetExecutingAssembly().Location, "Metamorphosis.PreviousResults");
            prev.ToolTip = "Load a previous comparison from the saved file.";
            prev.LongDescription = "Load a previous comparison from a saved results file.";
            prev.Image = getImage("Metamorphosis.Images.File-16.png");
            prev.LargeImage = getImage("Metamorphosis.Images.File-32.png");
            panel.AddItem(prev);

            // anything below here on the slideout?
            panel.AddSlideOut();
            var clear = new PushButtonData("ClearAVF", "Clear", System.Reflection.Assembly.GetExecutingAssembly().Location, "Metamorphosis.Commands.ClearAVF");
            clear.ToolTip = "Clear any AVF graphics from the current view.";
            clear.LongDescription = "Clear any Analysis Visualization Framework graphic primitives (faces, boxes, vectors) from the active view.";
            clear.Image = getImage("Metamorphosis.Images.clear-16.png");
            clear.LargeImage = getImage("Metamorphosis.Images.clear-32.png");
            panel.AddItem(clear);

        }

        private System.Windows.Media.ImageSource getImage(string imageFile)
        {
            try
            {
                System.IO.Stream stream = this.GetType().Assembly.GetManifestResourceStream(imageFile);
                if (stream == null) return null;
                PngBitmapDecoder pngDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return pngDecoder.Frames[0];

            }
            catch
            {
                return null; // no image
               

            }
        }
    }
}
