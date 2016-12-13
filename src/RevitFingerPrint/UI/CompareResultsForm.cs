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
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace Metamorphosis.UI
{
    public partial class CompareResultsForm : System.Windows.Forms.Form
    {
        private IList<Objects.Change> _changes;
        private UIDocument _uiDoc;
        private EventHandler<IdlingEventArgs> _idleEvent;
        private bool _isResetting = false;

        private enum ActionEnum { None,ShowElement, Shutdown, ColorElements, RemoveColor, ResetColors};
        private ActionEnum _action = ActionEnum.None;
        private ICollection<ElementId> _idsToShow;
        private Dictionary<int, bool> _viewsColored = new Dictionary<int, bool>();

        public CompareResultsForm(UIDocument uiDoc, IList<Objects.Change> changes)
        {
            _uiDoc = uiDoc;
            _changes = changes;
            InitializeComponent();

            _idleEvent = Application_Idling;
            _uiDoc.Application.Idling += _idleEvent;
        }

        private void onShown(object sender, EventArgs e)
        {
            // for now...
           

            cbRender.SelectedIndex = 0;
           
        }   

        private void Application_Idling(object sender, IdlingEventArgs e)
        {
            // see if there's something for us to do.

            try
            {
                // we want frequent callbacks
                e.SetRaiseWithoutDelay();
                ActionEnum tmpAction = _action;
                _action = ActionEnum.None;

                switch (tmpAction)
                {
                    case ActionEnum.ShowElement:
                        performShow();
                        break;

                    case ActionEnum.Shutdown:
                        performShutdown();
                        break;

                    case ActionEnum.ColorElements:
                        performColor();
                        break;

                    case ActionEnum.RemoveColor:
                        removeColor(_uiDoc.ActiveGraphicalView);
                        break;

                    case ActionEnum.ResetColors:
                        resetColors();
                        break;
                }
            }
            catch (Exception ex)
            {
                TaskDialog td = new TaskDialog("Error in Revit Fingerprint");
                td.MainContent = "An unexpected error occurred.";
                td.ExpandedContent = ex.GetType().Name + ": " + ex.Message + " " + ex.StackTrace;
            }
        }

        private void performShow()
        {
            _uiDoc.ShowElements(_idsToShow);
        }

        private void performShutdown()
        {
            _uiDoc.Application.Idling -= _idleEvent;
        }

        private void performColor()
        {
            // we need to collect all of the element Ids


            
            Transaction t = new Transaction(_uiDoc.Document, "Color Changed Elements");
            t.Start();

            if (!_isResetting) _viewsColored[_uiDoc.ActiveGraphicalView.Id.IntegerValue] = true;

            Autodesk.Revit.DB.Color overrideColor = new Autodesk.Revit.DB.Color(0, 0, 0);
            // group changes by type...
            foreach( var group in _changes.GroupBy( c => c.ChangeType).ToDictionary( c=> c.Key, c => c.ToList()))
            {
                if (group.Key == Objects.Change.ChangeTypeEnum.DeletedElement) continue; // can't

                
                var ogs = new Autodesk.Revit.DB.OverrideGraphicSettings();

                var patternCollector = new FilteredElementCollector(_uiDoc.Document);
                patternCollector.OfClass(typeof(Autodesk.Revit.DB.FillPatternElement));
                Autodesk.Revit.DB.FillPatternElement solidFill = patternCollector.ToElements().Cast<Autodesk.Revit.DB.FillPatternElement>().First(x => x.GetFillPattern().IsSolidFill);

                IList<ElementId> ids = collectIds(group.Value);
                
                switch( group.Key)
                {
                    case Objects.Change.ChangeTypeEnum.NewElement:
                        overrideColor = new Autodesk.Revit.DB.Color(0, 255, 0);
                        break;

                    case Objects.Change.ChangeTypeEnum.ParameterChange:
                        overrideColor = new Autodesk.Revit.DB.Color(255, 0, 0);
                        break;

                    case Objects.Change.ChangeTypeEnum.GeometryChange:
                        overrideColor = new Autodesk.Revit.DB.Color(0, 0, 255);
                        break;

                    default:
                        overrideColor = new Autodesk.Revit.DB.Color(255, 0, 0);
                        break;
                }
                ogs.SetProjectionFillColor(overrideColor);
                ogs.SetProjectionFillPatternId(solidFill.Id);
                ogs.SetProjectionLineColor(overrideColor);
                ogs.SetCutFillColor(overrideColor);
                ogs.SetCutFillPatternId(solidFill.Id);
                ogs.SetCutLineColor(overrideColor);
                
                foreach (ElementId id in ids)
                {
                    _uiDoc.ActiveGraphicalView.SetElementOverrides(id, ogs);
                }

            }
            
            t.Commit();

        }

        private void resetColors()
        {

            _isResetting = true;
            foreach( var pair in _viewsColored)
            {
                if (pair.Value == false) continue;

                ElementId id = new ElementId(pair.Key);

                Autodesk.Revit.DB.View v = _uiDoc.Document.GetElement(id) as Autodesk.Revit.DB.View;
                removeColor(v);
            }
        }

        private void removeColor(Autodesk.Revit.DB.View view)
        {
            // we need to collect all of the element Ids



            Transaction t = new Transaction(_uiDoc.Document, "Remove Color of Changed Elements");
            t.Start();

            if (!_isResetting) _viewsColored[_uiDoc.ActiveGraphicalView.Id.IntegerValue] = false;

            var changes = _changes.Where(c => c.ChangeType != Objects.Change.ChangeTypeEnum.DeletedElement).ToList();

            IList<ElementId> ids = collectIds(changes);

            var ogs = new Autodesk.Revit.DB.OverrideGraphicSettings();
           
                foreach (ElementId id in ids)
                {
                    view.SetElementOverrides(id, ogs);
                }

            

            t.Commit();

        }

        private IList<ElementId> collectIds(IList<Objects.Change> changes)
        {
            List<ElementId> ids = new List<ElementId>();
            foreach( var c in changes.Where( n => n.ChangeType != Objects.Change.ChangeTypeEnum.DeletedElement) )
            {
               
                ids.Add(new ElementId(c.ElementId));
            }
            return ids;
        }

        private void renderByCategory()
        {
            treeView1.Nodes.Clear();
            treeView1.ShowNodeToolTips = true;

            TreeNode root = new TreeNode("Changes: " + _changes.Count);
            treeView1.Nodes.Add(root);

            foreach( var group in _changes.GroupBy( c => c.Category).ToDictionary( c=>c.Key, c => c.ToList()))
            {
                TreeNode catNode = new TreeNode(group.Key);
                catNode.Text = catNode.Text + "(" + group.Value.Count + ")";
                root.Nodes.Add(catNode);

                foreach( var item in group.Value)
                {

                    TreeNode itemNode = buildItemNode(item);
                    catNode.Nodes.Add(itemNode);
                }
            }

            root.Expand();
        }

        private void renderByLevel()
        {
            treeView1.Nodes.Clear();
            treeView1.ShowNodeToolTips = true;

            TreeNode root = new TreeNode("Changes: " + _changes.Count);
            treeView1.Nodes.Add(root);

            foreach (var group in _changes.GroupBy(c => c.Level).ToDictionary(c => c.Key, c => c.ToList()))
            {
                string name = group.Key;
                if (String.IsNullOrEmpty(group.Key)) name = "(none)";
                TreeNode catNode = new TreeNode(name);
                
                catNode.Text = catNode.Text + "(" + group.Value.Count + ")";
                root.Nodes.Add(catNode);

                // second level, by category.
                foreach (var group2 in group.Value.GroupBy(c => c.Category).ToDictionary(c => c.Key, c => c.ToList()))
                {
                    TreeNode nextNode = new TreeNode(group2.Key);
                    nextNode.Text = nextNode.Text + "(" + group2.Value.Count + ")";
                    catNode.Nodes.Add(nextNode);
                    foreach (var item in group2.Value)
                    {

                        TreeNode itemNode = buildItemNode(item);
                        nextNode.Nodes.Add(itemNode);
                    }
                }
            }

            root.Expand();
        }

        private void renderByChangeType()
        {
            treeView1.Nodes.Clear();
            treeView1.ShowNodeToolTips = true;

            TreeNode root = new TreeNode("Changes: " + _changes.Count);
            treeView1.Nodes.Add(root);

            foreach (var group in _changes.GroupBy(c => c.ChangeType).ToDictionary(c => c.Key, c => c.ToList()))
            {
               
                TreeNode catNode = new TreeNode(group.Key.ToString());
                catNode.Text = catNode.Text + "(" + group.Value.Count + ")";
                root.Nodes.Add(catNode);

                // second level by category.
                foreach (var group2 in group.Value.GroupBy(c => c.Category).ToDictionary(c => c.Key, c => c.ToList()))
                {
                    TreeNode nextNode = new TreeNode(group2.Key);
                    nextNode.Text = nextNode.Text + "(" + group2.Value.Count + ")";
                    catNode.Nodes.Add(nextNode);
                    foreach (var item in group2.Value)
                    {

                        TreeNode itemNode = buildItemNode(item);
                        nextNode.Nodes.Add(itemNode);
                    }
                }
            }

            root.Expand();
        }

        private TreeNode buildItemNode(Objects.Change item)
        {
            TreeNode itemNode = new TreeNode(item.Category + (item.IsType ? "Type":"") + ": " + item.ElementId + ": " + item.ChangeType);
            itemNode.ToolTipText = item.ChangeDescription;
            if (item.ChangeType != Objects.Change.ChangeTypeEnum.DeletedElement) itemNode.Tag = item.ElementId;
            if (item.IsType == false) itemNode.ForeColor = System.Drawing.Color.Blue;

            return itemNode;

        }

        private void onAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                // we have an id?
                int id = (int)e.Node.Tag;

                _idsToShow = new ElementId[] { new ElementId(id) };
                _action = ActionEnum.ShowElement;
            }
        }

        private void onFormClosing(object sender, FormClosingEventArgs e)
        {
            // see if anyone has left a view colored...
            if (_viewsColored.Any( v => v.Value == true))
            {
                if (MessageBox.Show("There were view(s) that were colored via element overrides. Do you want to keep these colors?",
                    "View(s) still colored?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) != DialogResult.Yes)
                {

                    _action = ActionEnum.ResetColors;
                    return;
                }
                    
            }
            _action = ActionEnum.Shutdown;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _action = ActionEnum.Shutdown;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _action = ActionEnum.ColorElements;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _action = ActionEnum.RemoveColor;
        }

        private void onRenderChange(object sender, EventArgs e)
        {
            switch( cbRender.SelectedItem.ToString().ToUpper())
            {
                case "BY CATEGORY":
                    renderByCategory();
                    break;

                case "BY LEVEL":
                    renderByLevel();
                    break;

                case "BY CHANGE TYPE":
                    renderByChangeType();
                    break;
            }
        }
    }
}
