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
        private Document _chosenDoc;
        private EventHandler<IdlingEventArgs> _idleEvent;
        private bool _isResetting = false;
        private bool _isLinkDoc = false;

        private enum ActionEnum { None,ShowElement, Shutdown, ColorElements, RemoveColor, ResetColors, ShowSolid, ShowMove, ShowRotation, Select};
        private ActionEnum _action = ActionEnum.None;
        private ICollection<ElementId> _idsToShow;
        private Dictionary<int, bool> _viewsColored = new Dictionary<int, bool>();
        private IList<Objects.Change> _selectedItems;
        private TreeNode _rightClickedNode;

        public CompareResultsForm(UIDocument uiDoc, Document chosenDoc, IList<Objects.Change> changes)
        {
            _uiDoc = uiDoc;
            _changes = changes;
            _chosenDoc = chosenDoc;
            InitializeComponent();

            _idleEvent = Application_Idling;
            _uiDoc.Application.Idling += _idleEvent;

            _isLinkDoc = (uiDoc.Document.Title != chosenDoc.Title);
        }

        private void onShown(object sender, EventArgs e)
        {
            // for now...
           

            cbRender.SelectedIndex = 0;

            if (_isLinkDoc)
            {
                MessageBox.Show(this, "The results are not for the currently active model. Limited highlighting will be available", "Limitations for Linked Model");
                button3.Enabled = false;
                button2.Enabled = false;

                this.Text = "Metamorphosis: Compare Results for: " + _chosenDoc.Title;
                this.BackColor = System.Drawing.Color.LightBlue;
            }
           
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

                    case ActionEnum.ShowSolid:
                        performShowSolid();
                        break;

                    case ActionEnum.ShowMove:
                        performShowMove();
                        break;

                    case ActionEnum.ShowRotation:
                        performShowRotate();
                        break;

                    case ActionEnum.Select:
                        performSelect();
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
            if (_isLinkDoc)
            {
                performShowSolid();
                return;
            }
            List<ElementId> ids = new List<ElementId>();
            foreach (var change in _selectedItems) ids.Add(new ElementId(change.ElementId));
            
            _uiDoc.ShowElements(ids);
            _uiDoc.Selection.SetElementIds(ids);
        }

        private void performSelect()
        {
            if (_isLinkDoc) return; // can't

            List<ElementId> ids = new List<ElementId>();
            foreach (var item in _selectedItems) ids.Add(new ElementId(item.ElementId));
            _uiDoc.Selection.SetElementIds(ids);
        }

        private void performShutdown()
        {
            _uiDoc.Application.Idling -= _idleEvent;
        }

        private void performShowMove()
        {

            try
            {
                // try to show the move stuff.

                // show the vectors, and zoom.
                List<ElementId> ids = new List<ElementId>();
                foreach (var change in _selectedItems) ids.Add(new ElementId(change.ElementId));

                if (!_isLinkDoc) _uiDoc.ShowElements(ids);

                // retrieve the points:
                IList<Objects.VectorObject> vectors = lookupPoints(_selectedItems);
                Utilities.AVFUtility.ShowVectors(_uiDoc.Document, vectors, true); 

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while trying to show move: " + ex);
            }

        }

        private void performShowRotate()
        {

            try
            {
                // try to show the rotate stuff.

                // show the vectors, and zoom.
                List<ElementId> ids = new List<ElementId>();
                foreach (var change in _selectedItems) ids.Add(new ElementId(change.ElementId));

                if (!_isLinkDoc) _uiDoc.ShowElements(ids);

                // retrieve the rotation vectors:
                IList<Objects.VectorObject> vectors = lookupRotations(_selectedItems);
                Utilities.AVFUtility.ShowVectors(_uiDoc.Document, vectors, true);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while trying to show rotation: " + ex);
            }

        }
        private void performShowSolid()
        {
            // maybe we don't need a transaction right here?

            if (_selectedItems == null) return;
            try
            {
                double xMin = 9999999; double xMax = -999999999; double yMin = 99999999; double yMax = -999999999; double zMin = 99999999; double zMax = -99999999;
                List<Solid> solids = new List<Solid>();
                List<double> values = new List<double>();
                foreach (var item in _selectedItems)
                {
                    
                        BoundingBoxXYZ box = Utilities.RevitUtils.DeserializeBoundingBox(item.BoundingBoxDescription);
                        if (box != null)
                        {
                            Solid s = Utilities.RevitUtils.CreateSolidFromBox(_uiDoc.Application.Application, box);
                        if (s != null)
                        {
                            solids.Add(s);
                            values.Add(1.0);
                        }
                        // see if the box is bigger. If so, expand it.
                        if (box.Min.X < xMin) xMin = box.Min.X;
                        if (box.Min.Y < yMin) yMin = box.Min.Y;
                        if (box.Min.Z < zMin) zMin = box.Min.Z;
                        if (box.Max.X > xMax) xMax = box.Max.X;
                        if (box.Max.Y > yMax) yMax = box.Max.Y;
                        if (box.Max.Z > zMax) zMax = box.Max.Z;
                            
                        }
                   
                }
                Utilities.AVFUtility.ShowSolids(_uiDoc.Document, solids, values);

                UIView v = _uiDoc.GetOpenUIViews().FirstOrDefault();
                if (v != null)
                {
                    BoundingBoxXYZ bigBox = new BoundingBoxXYZ() { Min = new XYZ(xMin, yMin, zMin), Max = new XYZ(xMax, yMax, zMax) };
                    v.ZoomAndCenterRectangle(bigBox.Min, bigBox.Max);
                }

            }
            catch (Exception ex)
            {
                // swallow.
                MessageBox.Show("Error: " + ex);
            }
        }
        private void performColor()
        {
            // we need to collect all of the element Ids


            
            Transaction t = new Transaction(_uiDoc.Document, "Color Changed Elements");
            t.Start();

            if (!_isResetting) _viewsColored[_uiDoc.ActiveGraphicalView.Id.IntegerValue] = true;

            Autodesk.Revit.DB.Color overrideColor = new Autodesk.Revit.DB.Color(0, 0, 0);

            // group changes by type...
            var grouped =  _changes.GroupBy(c => c.ChangeType).ToDictionary(c => c.Key, c => c.ToList());

            var list = grouped.Keys.ToList();
            if (list.Contains( Objects.Change.ChangeTypeEnum.DeletedElement)) list.Remove(Objects.Change.ChangeTypeEnum.DeletedElement);
            if (list.Count==0)
            {
                MessageBox.Show("No color-able change types!");
                t.RollBack();
                return;
            }
            UI.ColorChoiceForm choice = new UI.ColorChoiceForm(list);
            if (choice.ShowDialog(this) != DialogResult.OK)
            {
                t.RollBack();
                return;
            }

            foreach ( var group in grouped)
            {
                if (group.Key == Objects.Change.ChangeTypeEnum.DeletedElement) continue; // can't

                if (choice.ChangeTypes.Contains(group.Key) == false) continue; // not selected.
                
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
            treeView1.Sort();
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

            treeView1.Sort();
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
            itemNode.Tag = item;
            
            if (item.IsType == false) itemNode.ForeColor = System.Drawing.Color.Blue;

            return itemNode;

        }

        private void onAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                Objects.Change item = e.Node.Tag as Objects.Change;
                if (item != null)
                {
                    _selectedItems = new Objects.Change[] { item };

                    switch (item.ChangeType)
                    {
                        case Objects.Change.ChangeTypeEnum.Move:
                            _action = ActionEnum.ShowMove;
                            break;

                        case Objects.Change.ChangeTypeEnum.Rotate:
                            _action = ActionEnum.ShowRotation;
                            break;

                        case Objects.Change.ChangeTypeEnum.DeletedElement:
                            _action = ActionEnum.ShowSolid;
                            break;

                        default:
                            _action = (_isLinkDoc ? ActionEnum.ShowSolid : ActionEnum.ShowElement);
                            break;
                    }
                }               
                
                else
                {
                    // we have an id?
                    int id = (int)e.Node.Tag;

                    _idsToShow = new ElementId[] { new ElementId(id) };
                    if (!_isLinkDoc) _action = ActionEnum.ShowElement;
                }
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

        private IList<Objects.VectorObject> lookupPoints(IList<Objects.Change> items)
        {
            List<Objects.VectorObject> vectors = new List<Objects.VectorObject>();
            foreach (var item in items)
            {
                if (item.ChangeType != Objects.Change.ChangeTypeEnum.Move) continue;

                if (String.IsNullOrEmpty(item.MoveDescription) == false)
                {
                    XYZ from1 = null; XYZ from2 = null; XYZ to1 = null; XYZ to2 = null;

                    if (Utilities.RevitUtils.DeSerializeMove(item.MoveDescription, out from1, out to1, out from2, out to2))
                    {
                        vectors.Add(new Objects.VectorObject(from1, to1.Subtract(from1)));

                        if (from2 != null)
                        {
                            vectors.Add(new Objects.VectorObject(from2, to2.Subtract(from2)));
                        }
                    }


                }
            }

            return vectors;

        }

        private IList<Objects.VectorObject> lookupRotations(IList<Objects.Change> items)
        {
            List<Objects.VectorObject> vectors = new List<Objects.VectorObject>();
            foreach (var item in items)
            {
                if (item.ChangeType != Objects.Change.ChangeTypeEnum.Rotate) continue;

                if (String.IsNullOrEmpty(item.RotationDescription) == false)
                {
                    XYZ point = null; XYZ vector = null; float rotation;

                    if (Utilities.RevitUtils.DeSerializeRotation(item.RotationDescription, out point, out vector, out rotation ))
                    {
                        double defaultTall = 10;
                        double defaultWide = 5;
                        var box = Utilities.RevitUtils.DeserializeBoundingBox(item.BoundingBoxDescription);
                        if (box != null)
                        {
                            defaultTall = (box.Max.Z - box.Min.Z) * 1.2;
                            defaultWide = (box.Max.X - box.Min.X) * 1.5;
                        }
                        // one vector going up.
                        vectors.Add(new Objects.VectorObject(point, vector.Normalize().Multiply(defaultTall)));

                        // then one for each rotation indicator
                        vectors.Add(new Objects.VectorObject(point, new XYZ(1.0, 0.0, 0).Multiply(defaultWide)));
                        vectors.Add(new Objects.VectorObject(point, new XYZ(Math.Cos((double)rotation), Math.Sin((double)rotation), 0).Multiply(defaultWide)));


                        
                    }


                }
            }

            return vectors;
        }



        private void onOpening(object sender, CancelEventArgs e)
        {
            // when the thing opens...

        }

        private void onNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _rightClickedNode = e.Node;
            }
            else
            {
                _rightClickedNode = null;
            }
        }

        private void tsmiShow_Click(object sender, EventArgs e)
        {
            
            // context menu...
            List<Objects.Change> changes = new List<Objects.Change>();
            if (_rightClickedNode == null)
            {
                MessageBox.Show("Please right click on a node in the tree?");
                return;
            }

            collectTreeChanges(_rightClickedNode, changes);

            // see if we've got all nodes that are shown the same way...
            if (changes.Count==0)
            {
                MessageBox.Show("There are no show-able changes?");
                return;
            }

            IList<Objects.Change.ChangeTypeEnum> types = changes.Select(g => g.ChangeType).Distinct().ToList();

            if (types.Count>1)
            {
                MessageBox.Show("Sorry! You cannot show different kinds of changes at the same time via this method. Select a smaller set.");
                return;
            }

            _selectedItems = changes;

            if (types[0] == Objects.Change.ChangeTypeEnum.DeletedElement) _action = ActionEnum.ShowSolid;
            if (types[0] == Objects.Change.ChangeTypeEnum.GeometryChange) _action = ActionEnum.ShowElement;
            if (types[0] == Objects.Change.ChangeTypeEnum.Move) _action = ActionEnum.ShowMove;
            if (types[0] == Objects.Change.ChangeTypeEnum.Rotate) _action = ActionEnum.ShowRotation;
            if (types[0] == Objects.Change.ChangeTypeEnum.NewElement) _action = ActionEnum.ShowElement;
            if (types[0] == Objects.Change.ChangeTypeEnum.ParameterChange) _action = ActionEnum.ShowElement;

            
        }

        private void collectTreeChanges( TreeNode node, IList<Objects.Change> changes)
        {
            if (node.Tag is Objects.Change)
            {
                Objects.Change c = node.Tag as Objects.Change;
                if (c != null) changes.Add(c);
            }

            if (node.Nodes.Count>0)
            {
                foreach( TreeNode child in node.Nodes )
                {
                    collectTreeChanges(child, changes);
                }
            }
        }

        private void tsmiSelect_Click(object sender, EventArgs e)
        {
            if (_isLinkDoc)
            {
                MessageBox.Show("Unable to select in Linked-Model...");
                return;
            }
            // context menu...
            List<Objects.Change> changes = new List<Objects.Change>();
            if (_rightClickedNode == null)
            {
                MessageBox.Show("Please right click on a node in the tree?");
                return;
            }

            collectTreeChanges(_rightClickedNode, changes);

            // see if we've got all nodes that are shown the same way...
            if (changes.Count == 0)
            {
                MessageBox.Show("There are no select-able changes?");
                return;
            }

            _selectedItems = 
            changes.Where(c => c.ChangeType != Objects.Change.ChangeTypeEnum.DeletedElement).ToList();

            if (_selectedItems.Count==0)
            {
                MessageBox.Show("Cannot select these changes?");
                return;
            }

            _action = ActionEnum.Select;
        }
    }
}
