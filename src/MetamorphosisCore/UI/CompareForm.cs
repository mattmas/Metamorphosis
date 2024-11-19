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

        public Boolean UseDocumentGUID { get { return cbUseDocumentGuid.Checked; } }

        private IFilenameHint _hint;
        private Document _doc;
        private bool _suspendTreeUpdates = false;
        private static bool _UseDocumentGUID = false;
        #endregion

        public CompareForm(Document doc, IList<Document> allDocs, IFilenameHint hint)
        {
            InitializeComponent();

            _hint = hint;
            this.cbDocumentChoice.Items.AddRange(allDocs.ToArray());
            this.cbDocumentChoice.SelectedItem = doc;


            this.Text += " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            cbUseDocumentGuid.Checked = _UseDocumentGUID;
            cbUseDocumentGuid.Enabled = false;
            if (Double.TryParse(doc.Application.VersionNumber, out double revitVer))
            {
                cbUseDocumentGuid.Enabled = (revitVer >= 2023.0);
            }

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

            readCategorySettings();
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
                    catNode.Tag = c.CategoryType;
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

        private void readCategorySettings()
        {
            try
            {
                cbSelectionSets.Items.Clear();
                IList<Utilities.CategorySettingsFile> files = Utilities.CategorySettingsFile.GetFiles();
                cbSelectionSets.Items.Add("Select");

                foreach (var item in files) cbSelectionSets.Items.Add(item);                
                
                //set the default value
                string defaultSet = Utilities.Settings.GetDefaultCategories();
                if (defaultSet != null)
                {
                    var item = files.FirstOrDefault(f => f.Name.ToUpper() == defaultSet.ToUpper());
                    if (item != null) cbSelectionSets.SelectedItem = item;
                }
                if (cbSelectionSets.SelectedItem == null) cbSelectionSets.SelectedIndex = 0;

                //disable if not present.
                if (cbSelectionSets.Items.Count <= 1) cbSelectionSets.Enabled = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error reading stored category settings: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void onAfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_suspendTreeUpdates) return;

            foreach( TreeNode child in e.Node.Nodes)
            {
                child.Checked = e.Node.Checked;
            }

            // if it is an un-check, we also want to uncheck the parent, but not trigger the same update backwards.
            if (e.Node.Checked == false)
            {
                if ((e.Node.Parent != null) && (e.Node.Parent.Checked))
                {
                    _suspendTreeUpdates = true;
                    e.Node.Parent.Checked = false;
                }
            }

            _suspendTreeUpdates = false;

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

        private void btnSaveCategories_Click(object sender, EventArgs e)
        {
            try
            {
                string central, user;
                Utilities.CategorySettingsFile.GetFolders(out central, out user);

                string target = central;
                if (Utilities.Utility.CanWriteToFolder(central) == false) target = user;

                saveFileDialog1.Filter = "CategorySettings Files(*.categories)|*.categories|All Files(*.*)|*.*";
                saveFileDialog1.InitialDirectory = target;
                if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    // we need to capture the current settings
                    Utilities.CategorySettingsFile cf = new Utilities.CategorySettingsFile(saveFileDialog1.FileName);
                    populateCategoriesToSave(cf, treeView1.Nodes[0]);

                    cf.Save(saveFileDialog1.FileName);

                    MessageBox.Show("Current Settings saved as " + cf.Name + Environment.NewLine + "File: " + cf.Filename);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error while attempting to save category list: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void populateCategoriesToSave(Utilities.CategorySettingsFile cf, TreeNode node)
        {
            if (node.Tag is Category)
            {
                Category c = node.Tag as Category;
                if (node.Checked) cf.Settings.Add(new Utilities.CategorySetting() { Name = c.Name, CategoryId = c.Id.AsInt32(), Enabled = true });
            }

            // let's handle CategoryType level selections differently. Store just the categorytype, and none of the children.
            if (node.Tag is CategoryType)
            {
                CategoryType cType = (CategoryType)node.Tag;
                if (node.Checked && areAllChildrenChecked(node))
                {
                    // store as negative, to use as an indicator.
                    cf.Settings.Add(new Utilities.CategorySetting() { Name = "Type:" + cType, CategoryId = (int)cType, Enabled = true });

                    // in this case, there's no need to store individuals, and the individuals might change over time.
                    return;
                }
            }

            if (node.Nodes.Count>0)
            {
                foreach( TreeNode child in node.Nodes)
                {
                    populateCategoriesToSave(cf, child);
                }
            }
        }

        private bool areAllChildrenChecked(TreeNode parent)
        {
            foreach( TreeNode tn in parent.Nodes )
            {
                if (tn.Checked == false) return false;
                if (tn.Nodes.Count>0)
                {
                    if (!areAllChildrenChecked(tn)) return false;
                }
            }

            // if we got this far, they're all ok.
            return true;
        }

        private void onCategorySettingChanged(object sender, EventArgs e)
        {
            if (cbSelectionSets.SelectedIndex == 0) return; // the select entry.

            // first, set all nodes to blank.
            updateChecks(treeView1.Nodes[0], false);

            // now let's retrieve all of the nodes which are set.
            HashSet<long> idsToEnable = new HashSet<long>();
            HashSet<long> typesToEnable = new HashSet<long>();
            Utilities.CategorySettingsFile cf = cbSelectionSets.SelectedItem as Utilities.CategorySettingsFile;
            if (cf != null)
            {
                foreach( Utilities.CategorySetting cs in cf.Settings )
                {
                    // look specifically for types first.
                    if (cs.Name.ToUpper().StartsWith("TYPE:"))
                    {
                        if (cs.Enabled) typesToEnable.Add(cs.CategoryId);
                    }
                    else
                    {
                        // regular categories.
                        if (cs.Enabled) idsToEnable.Add(cs.CategoryId);
                    }
                }
            }
            
            updateChecksByInfo(treeView1.Nodes[0], idsToEnable, typesToEnable);
        }

        private void updateChecks(TreeNode node, bool isChecked)
        {
            node.Checked = isChecked;

            foreach( TreeNode child in node.Nodes)
            {
                updateChecks(child, isChecked);
            }
        }

        private void updateChecksByInfo(TreeNode node, HashSet<long> idsToEnable, HashSet<long> typesToEnable)
        {
            Category c = node.Tag as Category;
            if (c != null)
            {
                if (idsToEnable.Contains(c.Id.AsLong())) node.Checked = true;

            }
            if (node.Tag is CategoryType)
            {
                CategoryType cType = (CategoryType)node.Tag;
                if (typesToEnable.Contains((int)cType))
                {
                    node.Checked = true;
                }
            }

            foreach( TreeNode child in node.Nodes)
            {
                updateChecksByInfo(child, idsToEnable, typesToEnable);
            }
        }

        private void onEpisodeGUIDChecked(object sender, EventArgs e)
        {
            if (cbUseDocumentGuid.Checked)
            {
                Document = cbDocumentChoice.SelectedItem as Document;
                if (Document != null)
                {
                    if (Document.IsWorkshared == false)
                    {
                        MessageBox.Show("Warning: This model is not workshared, and as such if you use this method, we will not be able to identify DELETED elements compared to the previous model.");
                    }
                }
            }
        }
    }
}
