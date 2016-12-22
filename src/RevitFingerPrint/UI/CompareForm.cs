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
    public partial class CompareForm : System.Windows.Forms.Form
    {
        #region Declarations/Properties
        public Boolean AllCategories { get; set; }
        public Document Document { get; set; }

        public Boolean DateStamp { get { return cbDateTime.Checked; } }

        public String SelectedFile { get; set; }

        public IList<Category> SelectedCategories { get; set; } = new List<Category>();

        public IList<Document> AllDocuments { get; set; }

        private IFilenameHint _hint;
        private Document _doc;
        #endregion

        public CompareForm(Document doc, IList<Document> allDocs, IFilenameHint hint)
        {
            InitializeComponent();

            _hint = hint;
            this.cbDocumentChoice.Items.AddRange(allDocs.ToArray());
            this.cbDocumentChoice.SelectedItem = doc;
           


           
            _doc = doc;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(tbPrevious.Text) == false)
            {
                MessageBox.Show("File does not exist???");
                return;
            }

            SelectedFile = tbPrevious.Text;

            AllCategories = true;
            collectCategories(treeView1.Nodes[0]);

            Document = cbDocumentChoice.SelectedItem as Document;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                tbPrevious.Text = openFileDialog1.FileName;
            }
        }

        private void onShown(object sender, EventArgs e)
        {
            renderCategories();
        }

        private void renderCategories()
        {
            treeView1.Nodes.Clear(); // reset.

            // build the list of categories...
            TreeNode root = new TreeNode("Categories");
            treeView1.Nodes.Add(root);
            Dictionary<CategoryType, TreeNode> types = new Dictionary<CategoryType, TreeNode>();

            Document doc = _doc;
            Document chosen = cbDocumentChoice.SelectedItem as Document;
            if (chosen != null) doc = chosen;

            foreach (Category c in doc.Settings.Categories)
            {
                if (c.CategoryType == CategoryType.Internal) continue;
                if (c.CategoryType == CategoryType.Invalid) continue;

                if (types.ContainsKey(c.CategoryType) == false)
                {
                    TreeNode catNode = new TreeNode(c.CategoryType.ToString());
                    types.Add(c.CategoryType, catNode);
                    root.Nodes.Add(catNode);
                }

                TreeNode node = new TreeNode(c.Name);
                node.Tag = c;
                types[c.CategoryType].Nodes.Add(node);
            }

            treeView1.Sort();
            root.Expand();
            root.Checked = true;
        }

        private void onAfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach( TreeNode child in e.Node.Nodes)
            {
                child.Checked = e.Node.Checked;
            }
        }

        private void collectCategories(TreeNode node)
        {
           
            foreach( TreeNode child in node.Nodes)
            {
                if (child.Tag is Category)
                {
                    if (child.Checked)
                    {
                        SelectedCategories.Add(child.Tag as Category);
                    }
                    else
                    {
                        AllCategories = false;
                    }
                }
                collectCategories(child);
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
                    renderCategories();
                }
            }
        }
    }
}
