using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Metamorphosis.UI
{
    public partial class ColorChoiceForm : Form
    {
        public IList<Objects.Change.ChangeTypeEnum> ChangeTypes { get; set; }
        private TreeNode _root;

        public ColorChoiceForm( IList<Objects.Change.ChangeTypeEnum> types)
        {
            InitializeComponent();

            treeView1.CheckBoxes = true;
            _root = treeView1.Nodes.Add("Change Type(s)");
            foreach( var typ in types )
            {
                TreeNode tn = _root.Nodes.Add( typ.ToString(), typ.ToString());
                tn.Tag = typ;

                Autodesk.Revit.DB.Color c = Utilities.Settingcs.GetColor(typ);

                tn.BackColor = Color.FromArgb(c.Red, c.Green, c.Blue);
                tn.Checked = true;
            }
            _root.ExpandAll();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (TreeNode node in _root.Nodes) if (node.Checked) count++;

            if (count==0)
            {
                MessageBox.Show("You must select at least one change type!");
                return; 
            }
            ChangeTypes = new List<Objects.Change.ChangeTypeEnum>();

            foreach( TreeNode item in _root.Nodes )
            {
                if (item.Checked) ChangeTypes.Add((Objects.Change.ChangeTypeEnum)item.Tag);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    

        
    }
}
