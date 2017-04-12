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
using Metamorphosis.Utilities;

namespace Metamorphosis
{
    [Transaction(TransactionMode.Manual)]
    public class PreviousResults : IExternalCommand, IFilenameHint
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
           try
            {
                ExternalApp.FirstTimeRun(); // analytics
                Document doc = commandData.Application.ActiveUIDocument.Document;

                IList<Document> allDocs = Utilities.RevitUtils.GetCurrentDocumentAndLinks(doc);

                string filename = String.Empty;
               

                UI.ComparePreviousForm form = new UI.ComparePreviousForm(doc, allDocs, this);
                if (form.ShowDialog() != DialogResult.OK) return Result.Cancelled;
                filename = form.SelectedFile;
                //string folder = Path.GetDirectoryName(filename);

                if (filename.ToUpper().StartsWith("HTTP")) filename = getFile(filename);
              
                Document chosenDoc = form.Document;

                var cs = ComparisonMaker.DeSerialize(filename);

                // check if the filenames match?
                if (String.IsNullOrEmpty(chosenDoc.Title) == false)
                {
                    string f1 = System.IO.Path.GetFileName(cs.ModelName);
                    if (String.IsNullOrEmpty(f1) == false)
                    {
                        if (f1.ToUpper() != chosenDoc.Title.ToUpper())
                        {
                            TaskDialog td = new TaskDialog("Same File?");
                            td.MainContent = "The chosen document and the chosen results file appear to be from different sources?" + Environment.NewLine +
                                             " Document: " + chosenDoc.Title + Environment.NewLine +
                                             " File:     " + f1;

                            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Continue Anyway", "If they really don't match, the highlighting might be suspect.");
                            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Cancel This", "I'll look for a better match.");

                            var result = td.Show();

                            if (result != TaskDialogResult.CommandLink1) return Result.Cancelled;
                        }
                    }
                }


                IList<Objects.Change> changes = cs.Changes;

                if (changes.Count > 0)
                {
              


                    UI.CompareResultsForm results = new UI.CompareResultsForm(new UIDocument(doc), chosenDoc, changes);
                    int x, y;
                    Utilities.RevitUtils.GetExtents(commandData.Application, out x, out y);
                    results.Location = new System.Drawing.Point(x, y);
                    

                    IntPtr currentRevitWin = Utilities.Utility.GetMainWindowHandle();
                    if (currentRevitWin != null)
                    {
                        Utilities.WindowHandle handle = new Utilities.WindowHandle(currentRevitWin);

                        results.Show(handle);
                    }
                    else
                    {
                        results.Show();
                    }
                }
                else
                {
                    MessageBox.Show("There were no changes in the change summary file?");
                }

                

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

        public string GetFilenameHint(Document doc)
        {
            string filename = "";
            string folder = String.Empty;
            
            if (String.IsNullOrEmpty(doc.PathName) == false)
            {
                filename = Path.GetFileNameWithoutExtension(doc.PathName);
                if (doc.IsWorkshared)
                {
                    folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    ModelPath mp = doc.GetWorksharingCentralModelPath();
                    if (mp is FilePath)
                    {
                        string modelPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(mp);
                        folder = Path.GetDirectoryName(modelPath);
                        filename = Path.GetFileNameWithoutExtension(modelPath);
                    }
                }
                else
                {
                    folder = Path.GetDirectoryName(doc.PathName);
                    filename = Path.GetFileNameWithoutExtension(doc.PathName);
                }
               

                filename = getLastFilename(folder, filename);
            }

            return filename;
        }

   
        private string getFile(string url)
        {
            System.Net.WebClient client = new System.Net.WebClient();

            string filename = Path.GetFileName(url);
            filename = Path.Combine(Path.GetTempPath(), filename);
            client.DownloadFile(url, filename);

            return filename;
        }


        private string getLastFilename(string folder, string filename)
        {
            string[] files = Directory.GetFiles(folder, filename + "*.json");

            if (files.Length == 0) return String.Empty;

            return files.Last(); // TEMP
        }
    }
}
