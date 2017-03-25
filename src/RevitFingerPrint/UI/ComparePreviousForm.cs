using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace Metamorphosis.UI
{
    public partial class ComparePreviousForm : System.Windows.Forms.Form
    {
        #region Declarations
        public String SelectedFile { get; set; }
        public Document Document { get; set; }

        private IFilenameHint _hint;
        private Document _doc;
        #endregion

        #region Constructor
        public ComparePreviousForm(Document doc, IList<Document> allDocs, IFilenameHint hint)
        {
            InitializeComponent();

            _hint = hint;
            this.cbDocumentChoice.Items.AddRange(allDocs.ToArray());
            this.cbDocumentChoice.SelectedItem = doc;


            this.Text += " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            _doc = doc;
        }
        #endregion

        #region PublicMethods

        #endregion

        #region PrivateMethods
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                Uri url = new Uri(tbPrevious.Text);
                if (url.IsFile)
                {
                    if (System.IO.File.Exists(tbPrevious.Text) == false)
                    {
                        MessageBox.Show("File does not exist???");
                        return;
                    }
                }
                else
                {
                    if (url.Scheme.ToUpper().StartsWith("HTTP") == false)
                    {
                        throw new ApplicationException("URL schemes only support HTTP/S at present?");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            

            SelectedFile = tbPrevious.Text;

            Document = cbDocumentChoice.SelectedItem as Document;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "JSON Results Files(*.json)|*.json|All Files (*.*)|*.*";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                
                tbPrevious.Text = openFileDialog1.FileName;
            }
        }

        private void onSelectedModelChange(object sender, EventArgs e)
        {
            if (_hint != null)
            {
                // get the selected document.
                Document d = cbDocumentChoice.SelectedItem as Document;
                if (d != null)
                {
                    tbPrevious.Text = _hint.GetFilenameHint(d);
                    
                }
            }
        }
        #endregion


    }
}
