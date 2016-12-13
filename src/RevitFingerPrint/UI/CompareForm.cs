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
        public Boolean AllCategories { get; set; }
        public Document Document { get; set; }

        public Boolean DateStamp { get { return cbDateTime.Checked; } }

        public String SelectedFile { get; set; }

        public IList<Category> SelectedCategories { get; set; } = new List<Category>();

        private Document _doc;

        public CompareForm(Document doc, string suggestedFile)
        {
            InitializeComponent();

            tbPrevious.Text = suggestedFile;
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
            // build the list of categories...
            TreeNode root = new TreeNode("Categories");
            treeView1.Nodes.Add(root);
            Dictionary<CategoryType, TreeNode> types = new Dictionary<CategoryType, TreeNode>();

            foreach( Category c in _doc.Settings.Categories)
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
      
    }
}
