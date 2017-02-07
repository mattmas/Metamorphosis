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

        public ColorChoiceForm( IList<Objects.Change.ChangeTypeEnum> types)
        {
            InitializeComponent();

            foreach( var typ in types )
            {
                checkedListBox1.Items.Add(typ, true);
                
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count==0)
            {
                MessageBox.Show("You must select at least one change type!");
                return; 
            }
            ChangeTypes = new List<Objects.Change.ChangeTypeEnum>();

            foreach( var item in checkedListBox1.CheckedItems )
            {
                ChangeTypes.Add((Objects.Change.ChangeTypeEnum)item);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
