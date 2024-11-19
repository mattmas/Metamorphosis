using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Metamorphosis.UI
{
    public partial class ExportSelectionForm : Form
    {
        public Autodesk.Revit.DB.Document SelectedDocument { get; private set; }
        public String Filename { get; private set; }

        public ExportSelectionForm(IList<Autodesk.Revit.DB.Document> docs, Autodesk.Revit.DB.Document current)
        {
            InitializeComponent();

            cbSelectedModel.DataSource = docs.ToArray();
            cbSelectedModel.Refresh();

            cbSelectedModel.SelectedItem = current;

            this.Text += " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        }

        private void ExportSelectionForm_Load(object sender, EventArgs e)
        {

        }

        private void onModelSelected(object sender, EventArgs e)
        {
            // update the default filename.
            tbFilename.Text = suggestModelName();
        }

        private string suggestModelName()
        {
            Autodesk.Revit.DB.Document doc = cbSelectedModel.SelectedItem as Autodesk.Revit.DB.Document;

            if (doc != null)
            {
                string filename = Path.GetFileNameWithoutExtension(doc.PathName);
                if (doc.IsWorkshared && (! doc.IsDetached))
                {
                    try
                    {
                       
                            var mp = doc.GetWorksharingCentralModelPath();
                            string centralPath = Autodesk.Revit.DB.ModelPathUtils.ConvertModelPathToUserVisiblePath(mp);
                            if ((mp.ServerPath == false) && System.IO.Path.IsPathRooted(centralPath))
                            {
                                string folder = Path.GetDirectoryName(centralPath);
                                string baseName = Path.GetFileNameWithoutExtension(centralPath);
                                filename = Path.Combine(folder, "Snapshots", baseName + "_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".sdb");
                            }
                            if (centralPath.ToUpper().StartsWith("BIM360:") || centralPath.ToUpper().StartsWith("AUTODESK DOC"))
                        {
                            string baseName = Path.GetFileNameWithoutExtension(filename);
                            filename = baseName + "_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".sdb";

                            filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), baseName + ".sdb");
                        }

                        if (filename.ToUpper().EndsWith(".SDB") == false) filename += ".sdb";
                       
                    }
                    catch { }
                }
                else
                {
                    if (String.IsNullOrEmpty(doc.PathName)) return String.Empty;

                    try
                    {
                        filename = Path.Combine(Path.GetDirectoryName(doc.PathName), filename + "_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".sdb");
                    }
                    catch (Exception ex)
                    {
                        doc.Application.WriteJournalComment("Note: struggling to get suggested filename: " + ex.GetType().Name + ": " + ex.Message, false);
                    }
                }
                


                

                return filename;
            }

            return string.Empty;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Utilities.Utility.IsValidPath(tbFilename.Text) == false)
            {
                MessageBox.Show("Please enter a valid path for the file.");
                return;
            }

            if (cbSelectedModel.SelectedItem == null)
            {
                MessageBox.Show("Please select a valid model to snapshot.");
                return;
            }

            SelectedDocument = cbSelectedModel.SelectedItem as Autodesk.Revit.DB.Document;
            Filename = tbFilename.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                tbFilename.Text = saveFileDialog1.FileName;
            }
        }
    }
}
