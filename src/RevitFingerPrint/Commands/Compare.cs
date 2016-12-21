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
    [Transaction(TransactionMode.Manual)]
    public class Compare : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
           try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;

                string filename = String.Empty;
                string folder = String.Empty; 
                if (String.IsNullOrEmpty(doc.PathName) == false)
                {
                    folder = Path.GetDirectoryName(doc.PathName);
                    filename = Path.GetFileNameWithoutExtension(doc.PathName);

                    filename = getLastFilename(folder, filename);
                }

                UI.CompareForm form = new UI.CompareForm(doc, filename);
                if (form.ShowDialog() != DialogResult.OK) return Result.Cancelled;
                filename = form.SelectedFile;
                bool dateStamp = form.DateStamp;

                ComparisonMaker comparison = new ComparisonMaker(doc, filename);
                comparison.AllCategories = form.AllCategories;
                comparison.RequestedCategories = form.SelectedCategories;

                IList<Objects.Change> changes = comparison.Compare();

                if (changes.Count > 0)
                {
                    string jsonFile = Path.Combine(folder, Path.GetFileNameWithoutExtension(doc.PathName) + "-Changes-Latest.json");
                    comparison.Serialize(jsonFile, changes);
                    if (dateStamp)
                    {
                        jsonFile = Path.Combine(folder, Path.GetFileNameWithoutExtension(doc.PathName) + "-Changes-" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".json");
                        comparison.Serialize(jsonFile, changes);
                    }


                    UI.CompareResultsForm results = new UI.CompareResultsForm(new UIDocument(doc), changes);
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

        private string getLastFilename(string folder, string filename)
        {
            string[] files = Directory.GetFiles(folder, filename + "*.sdb");

            if (files.Length == 0) return String.Empty;

            return files.Last(); // TEMP
        }
    }
}
