using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.IO;
using Autodesk.Revit.Attributes;

namespace Metamorphosis
{
    [Transaction(TransactionMode.Manual), Journaling(JournalingMode.UsingCommandData)]
    public class Snapshot : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
           try
            {

                ExternalApp.FirstTimeRun(); // analytics
                Document doc = commandData.Application.ActiveUIDocument.Document;

                IList<Document> inMemory = Utilities.RevitUtils.GetProjectsInMemory(commandData.Application.Application);

                UI.ExportSelectionForm form = new UI.ExportSelectionForm(inMemory, doc);
                if (form.ShowDialog() != DialogResult.OK) return Result.Cancelled;

                string filename = form.Filename;

                //store for the future? seems like there are suddenly problems with reading this info back...
                commandData.JournalData.Add("DocumentName", form.SelectedDocument.Title);
                commandData.JournalData.Add("Filename", form.Filename);

                SnapshotMaker maker = new SnapshotMaker(form.SelectedDocument, form.Filename);
                maker.Export();

                TaskDialog td = new TaskDialog("Fingerprint");
                td.MainContent = "The snapshot file has been created.";
                td.ExpandedContent = "File: " + filename + Environment.NewLine + "Duration: " + maker.Duration.TotalMinutes.ToString("F2") + " minutes.";
                td.Show();

                

                return Result.Succeeded;
            }            
            catch (ApplicationException aex)
            {
                MessageBox.Show(aex.Message);
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error! " + ex);
                return Result.Failed;
            }
            
        }

        /// <summary>
        /// Batch version, so that others can call it.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="filename"></param>
        public static void Export(Document doc, string filename)
        {
            doc.Application.WriteJournalComment("Launching Batch Metamorphosis Snapshot...", false);
            doc.Application.WriteJournalComment("  Filename: " + filename, false);
            SnapshotMaker maker = new SnapshotMaker(doc, filename);
            maker.Export();

            doc.Application.WriteJournalComment("Snapshot completed. Duration:  " + maker.Duration, false);

        }
    }
}
