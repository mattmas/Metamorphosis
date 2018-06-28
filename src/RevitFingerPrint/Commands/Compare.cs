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
    public class Compare : IExternalCommand, IFilenameHint
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
           try
            {
                ExternalApp.FirstTimeRun(); // analytics
                Document doc = commandData.Application.ActiveUIDocument.Document;

                IList<Document> allDocs = Utilities.RevitUtils.GetCurrentDocumentAndLinks(doc);

                string filename = String.Empty;
               

                UI.CompareForm form = new UI.CompareForm(doc, allDocs, this);
                if (form.ShowDialog() != DialogResult.OK) return Result.Cancelled;
                filename = form.SelectedFile;
                string folder = Path.GetDirectoryName(filename);
                string tmpFile = GetFilenameHint(form.Document);

                bool dateStamp = form.DateStamp;

                Document chosenDoc = form.Document;

                ComparisonMaker comparison = new ComparisonMaker(chosenDoc, filename);
                comparison.AllCategories = form.AllCategories;
                comparison.RequestedCategories = form.SelectedCategories;

                try
                {
                    double moveTol;
                    float angTol;
                    Utilities.Settings.ReadTolerance(out moveTol, out angTol);
                    comparison.MoveTolerance = moveTol;
                    comparison.RotateTolerance = angTol;
                }
                catch (Exception ex)
                {
                    doc.Application.WriteJournalComment("Exception reading tolerances from settings file: " + ex.GetType().Name + ": " + ex.Message,false);
                }
                doc.Application.WriteJournalComment("Tolerances: Distance: " + comparison.MoveTolerance + " Angle: " + comparison.RotateTolerance, false);

                IList<Objects.Change> changes = comparison.Compare();

                if (changes.Count > 0)
                {
                    string jsonFile = Path.Combine(folder, Path.GetFileNameWithoutExtension(tmpFile) + "-Changes-Latest.json");
                    comparison.Serialize(jsonFile, changes);
                    if (dateStamp)
                    {
                        jsonFile = Path.Combine(folder, Path.GetFileNameWithoutExtension(tmpFile) + "-Changes-" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".json");
                        comparison.Serialize(jsonFile, changes);
                    }


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
                    MessageBox.Show("Found no changes between this model and the previous snapshot.");
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
                        if (Directory.Exists(Path.Combine(folder, "Snapshots"))) folder = Path.Combine(folder, "Snapshots");  // encourage this.
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

        /// <summary>
        /// Batch operation from the outside...
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="previousFile"></param>
        /// <param name="targetFolder"></param>
        /// <param name="dateStampResults"></param>
        /// <param name="categoryConfig"></param>
        /// <returns></returns>
        public static string BatchCompare(Document doc, string previousFile, string targetFolder, bool dateStampResults, string categoryConfig, out int numChanges)
        {
            try
            {
                doc.Application.WriteJournalComment("Launching Batch Metamorphosis Compare:", false);
                doc.Application.WriteJournalComment("  Previous File: " + previousFile, false);
                doc.Application.WriteJournalComment("  TargetFolder:  " + targetFolder, false);
                doc.Application.WriteJournalComment("  Date Stamp Results: " + dateStampResults, false);
                doc.Application.WriteJournalComment("  Category Config: " + categoryConfig, false);
                ExternalApp.FirstTimeRun(); // analytics
               


                numChanges = -1;
                if (previousFile == null)
                {
                    Compare c = new Compare();
                    previousFile = c.GetFilenameHint(doc);
                }

                ComparisonMaker comparison = new ComparisonMaker(doc, previousFile);
                comparison.AllCategories = (categoryConfig == null || categoryConfig.ToUpper() == "[ALL]" || categoryConfig.ToUpper() == "ALL");
                if (!comparison.AllCategories)
                {
                    if (categoryConfig.Contains("["))
                    {
                        // figure that it's a special configuration.
                        string typeName = categoryConfig.Trim('[', ']', ' ');
                        CategoryType targetType = CategoryType.Model;

                        if (Enum.TryParse<CategoryType>(typeName, out targetType))
                        {
                            foreach (Category cat in doc.Settings.Categories)
                            {
                                if (cat.CategoryType == CategoryType.Internal) continue;
                                if (cat.CategoryType == CategoryType.Invalid) continue;

                                if (cat.CategoryType == targetType) comparison.RequestedCategories.Add(cat);
                            }
                        }
                        else
                        {
                            throw new ApplicationException("Did not find dynamic Category Filter: " + typeName);

                        }
                    }
                    else  // look for a file-based thing.
                    {
                        // we need to go look for the stored settings with this name...
                        var file = CategorySettingsFile.GetFileByName(categoryConfig);

                        doc.Application.WriteJournalComment("Found CategorySettingsFile: " + file.Filename, false);
                        comparison.RequestedCategories = new List<Category>();
                        foreach (var setting in file.Settings)
                        {
                            if (setting.Enabled)
                            {
                                Category c = doc.Settings.Categories.get_Item((BuiltInCategory)setting.CategoryId);
                                if (c != null)
                                {
                                    if (c.CategoryType == CategoryType.Internal) continue;
                                    if (c.CategoryType == CategoryType.Invalid) continue;

                                    comparison.RequestedCategories.Add(c);
                                }
                            }
                        }
                    }
                    doc.Application.WriteJournalComment("Have " + comparison.RequestedCategories.Count + " categories requested for comparison.", false);

                }
                //comparison tolerances:
                try
                {
                    double moveTol;
                    float angTol;
                    Utilities.Settings.ReadTolerance(out moveTol, out angTol);
                    comparison.MoveTolerance = moveTol;
                    comparison.RotateTolerance = angTol;
                }
                catch (Exception ex)
                {
                    doc.Application.WriteJournalComment("Exception reading tolerances from settings file: " + ex.GetType().Name + ": " + ex.Message, false);
                }
                doc.Application.WriteJournalComment("Tolerances: Distance: " + comparison.MoveTolerance + " Angle: " + comparison.RotateTolerance, false);



                string tmpFile = doc.Title;
                if (tmpFile.ToUpper().Contains("_DETACHED")) tmpFile = tmpFile.ToLower().Replace("_detached", "");

                doc.Application.WriteJournalComment("Starting comparison...", false);

                IList<Objects.Change> changes = comparison.Compare();

                doc.Application.WriteJournalComment("Found " + changes.Count + " change(s).", false);
                numChanges = changes.Count;

                if (changes.Count > 0)
                {
                    string jsonFile = Path.Combine(targetFolder, Path.GetFileNameWithoutExtension(tmpFile) + "-Changes-Latest.json");
                    comparison.Serialize(jsonFile, changes);
                    if (dateStampResults)
                    {
                        jsonFile = Path.Combine(targetFolder, Path.GetFileNameWithoutExtension(tmpFile) + "-Changes-" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".json");
                        comparison.Serialize(jsonFile, changes);
                    }


                    return jsonFile;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                doc.Application.WriteJournalComment("Unexpected exception in Metamorphosis ModelCompare. " + ex.GetType().Name + ": " + ex.Message, false);
                doc.Application.WriteJournalComment(ex.StackTrace, false);

                throw;
            }
        }


        private string getLastFilename(string folder, string filename)
        {
            string[] files = Directory.GetFiles(folder, filename + "*.sdb");

            if (files.Length == 0) return String.Empty;

            return files.Last(); // TEMP
        }
    }
}
