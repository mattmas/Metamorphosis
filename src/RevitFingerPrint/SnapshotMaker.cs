using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace Metamorphosis
{
    internal class SnapshotMaker
    {
        private Document _doc;
        private Dictionary<int, Parameter> _paramDict = new Dictionary<int, Parameter>();
        private Dictionary<string, int> _valueDict = new Dictionary<string, int>();
        private Dictionary<string, string> _headerDict = new Dictionary<string, string>();
        private string _filename;
        private string _dbFilename;
        private int _valueId = 0;
        private List<Level> _allLevels;
        private Utilities.Settings.LogLevel _logLevel = Utilities.Settings.LogLevel.Basic;


        #region Constructor
        internal SnapshotMaker(Document doc, string filename)
        {
            _doc = doc;
            _filename = filename;

            _dbFilename = _filename;
            // see: http://system.data.sqlite.org/index.html/info/bbdda6eae2
            if (_filename.StartsWith(@"\\")) _dbFilename = @"\\" + _dbFilename;

            _logLevel = Utilities.Settings.GetLogLevel();
        }
        #endregion

        #region Accessor
        internal TimeSpan Duration { get; private set; }
        #endregion

        #region PublicMethods
        internal void Export()
        {
            // make the sqlite database
            createDatabase();
            // populate
            exportParameterData();
        }
        #endregion

        #region PrivateMethods
        private void createDatabase()
        {
            if (File.Exists(_filename)) File.Delete(_filename); // we have to replace the contents anyway

            //create the SQLite database file.
            SQLiteConnection.CreateFile(_filename);

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbFilename + ";Version=3;"))
            {
                conn.Open();

                // create the table structure from the sql instructions.
                string[] lines = Utilities.DataUtility.ReadSQLScript("Metamorphosis.databaseFormat.txt");

                foreach (string sql in lines)
                {

                    SQLiteCommand command = new SQLiteCommand(sql, conn);
                    command.ExecuteNonQuery();
                }
            }
        
        }

        
        private void exportParameterData()
        {
            DateTime start = DateTime.Now;
            
            // retrieve all of the instance elements, and process them.
            FilteredElementCollector coll = new FilteredElementCollector(_doc);
            coll.WhereElementIsNotElementType();

            Dictionary<ElementId, Element> typeElementsUsed = new Dictionary<ElementId, Element>();
            IList<Element> instances = coll.ToElements().Where(e => e.Category != null).ToList();
            foreach ( var elem in instances)
            {
                if (elem.Category == null) continue; // don't do it!

                // see if the current element has a type element, and make sure we're getting that.
                ElementId typeId = elem.GetTypeId();
                if (typeId != ElementId.InvalidElementId)
                {
                    if (typeElementsUsed.ContainsKey(typeId) == false)
                    {
                        Element typeElem = _doc.GetElement(typeId);
                        if ((typeElem.Category != null))
                        {
                            typeElementsUsed.Add(typeId, typeElem); // only add if it's a typeElement with Category
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": " + instances.Count + " instances and " + typeElementsUsed.Count + " types.");

            // go through all of the type elements and instances and capture the parameter ids
            _headerDict["SchemaVersion"] = "1.0";
            _headerDict["Model"] = Utilities.RevitUtils.GetModelPath(_doc);
            _headerDict["ExportVersion"] = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _headerDict["ExportDate"] = DateTime.Now.ToString();
            _headerDict["ExportDateTicks"] = DateTime.Now.Ticks.ToString();
            _headerDict["ExportingUser"] = Environment.UserDomainName + "\\" + Environment.UserName;
            _headerDict["MachineName"] = Environment.MachineName;

            updateHeaderTable();

            updateParameterDictionary(typeElementsUsed.Values.ToList());
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Parameter Dictionary Updated for Types");
            updateParameterDictionary(instances);
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Parameter Dictionary Updated for Instances");

            updateIdTable(typeElementsUsed.Values.ToList(), true);
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Id Table Updated for Types");
            updateIdTable(instances, false);
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Id Table Updated for Instances");
            updateAttributeTable();
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Attribute Table Updated for All");


            updateEntityAttributeValues(typeElementsUsed.Values.ToList());
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Att/Values Table Updated for Types");

            updateEntityAttributeValues(instances);
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Att/Values Table Updated for Instances");
            updateValueTable();
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Value Table Updated for All");

            updateGeometryTable(instances);
            System.Diagnostics.Debug.WriteLine((DateTime.Now - start) + ": Geometry Table Updated for Types");
            Duration = DateTime.Now - start;
            System.Diagnostics.Debug.WriteLine("Total Time: " + Duration.TotalMinutes + " minutes");
        }

        private void updateIdTable(IList<Element> elements, bool isTypes)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbFilename + ";Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (Element e in elements)
                    {
                        Category c = e.Category;
                        if (c == null)
                        {
                            FamilySymbol fs = e as FamilySymbol;
                            if (fs != null) c = fs.Family.FamilyCategory;
                        }
                        string catName = (c != null) ? c.Name : "(none)";
                        if (catName.Contains("'")) catName = catName.Replace("'", "''");
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = String.Format("INSERT INTO _objects_id (id,external_id,category,isType) VALUES({0},'{1}','{2}',{3})", e.Id.IntegerValue, e.UniqueId,catName, (isTypes) ? 1:0);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        private void updateHeaderTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbFilename + ";Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var pair in _headerDict)
                    {
                       
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = String.Format("INSERT INTO _objects_header (keyword,value) VALUES('{0}','{1}')", pair.Key,pair.Value);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }
        private void updateAttributeTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbFilename + ";Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var pair in _paramDict)
                    {
                        string name = pair.Value.Definition.Name;
                        if (name.Contains("'")) name = name.Replace("'", "''");
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = String.Format("INSERT INTO _objects_attr (id,name,category,data_type) VALUES({0},'{1}','{2}',{3})", pair.Value.Id.IntegerValue, name, LabelUtils.GetLabelFor(pair.Value.Definition.ParameterGroup), (int)pair.Value.Definition.ParameterGroup);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        private void updateValueTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbFilename + ";Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (var pair in _valueDict)
                    {
                        string val = pair.Key;
                        if (val.Contains("'")) val = val.Replace("'", "''"); // need to escape single quotes.
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = String.Format("INSERT INTO _objects_val (id,value) VALUES({0},'{1}')", pair.Value, val);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        private void updateEntityAttributeValues(IList<Element> elems)
        {

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbFilename + ";Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {

                    foreach (Element e in elems)
                    {
                        IList<Parameter> parms = Utilities.RevitUtils.GetParameters(e);

                        foreach (var p in parms)
                        {
                            if (p.Definition == null) continue; // don't want that!

                            //Quick and Dirty - will need to call different stuff for each thing
                            string val = null;

                            switch (p.StorageType)
                            {
                                case StorageType.String:
                                    val = p.AsString();
                                    break;
                                default:
                                    val = p.AsValueString();
                                    break;
                            }
                            

                            if (val == null) val = "(n/a)";

                            if (_valueDict.ContainsKey(val) == false)
                            {
                                _valueId++;
                                _valueDict.Add(val, _valueId);
                            }


                            var cmd = conn.CreateCommand();
                            cmd.CommandText = String.Format("INSERT INTO _objects_eav (entity_id,attribute_id,value_id) VALUES({0},{1},{2})", e.Id.IntegerValue, p.Id.IntegerValue, _valueDict[val]);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }

            }
        }
        private void updateParameterDictionary(IList<Element> elems)
        {

            foreach( Element e in elems)
            {
                IList<Parameter> parms = Utilities.RevitUtils.GetParameters(e);

                                             
                foreach( Parameter p in parms )
                {
                    if (p.Definition == null) continue; // ignore!
                    if (_paramDict.ContainsKey(p.Id.IntegerValue) == false) _paramDict.Add(p.Id.IntegerValue, p);
                }                                
            }
        }


     

        private void updateGeometryTable(IList<Element> elements)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbFilename + ";Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (Element e in elements)
                    {
                        BoundingBoxXYZ box = e.get_BoundingBox(null);
                        Location loc = e.Location;

                        if ((loc == null) && (box == null)) continue; // nothing to see here.

                        String bbMin = String.Empty;
                        String bbMax = String.Empty;
                        string lp = String.Empty;
                        string lp2 = String.Empty;
                        float rotation = -1.0f;

                        if (box != null)
                        {
                            bbMin = Utilities.RevitUtils.SerializePoint(box.Min);
                            bbMax = Utilities.RevitUtils.SerializePoint(box.Max);
                        }

                        XYZ p1 = null;
                        if (loc != null)
                        {
                            LocationPoint pt = loc as LocationPoint;
                            if (pt != null)
                            {
                                XYZ pt1 = pt.Point;
                                // special cases.
                                if ((e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Columns)||
                                    (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns))
                                {
                                    // in this case, get the Z value from the 
                                    var offset = e.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);

                                    if ((e.LevelId != ElementId.InvalidElementId) && (offset != null))
                                    {
                                        Level levPt1 = lookupLevel(e, pt1);
                                        double newZ = levPt1.Elevation + offset.AsDouble();
                                        pt1 = new XYZ(pt1.X, pt1.Y, newZ);
                                    }
                                }

                                lp = Utilities.RevitUtils.SerializePoint(pt1);
                                try
                                {
                                    if (e is FamilyInstance)
                                    {
                                        if ((e as FamilyInstance).CanRotate)
                                        {
                                            rotation = (float)pt.Rotation;
                                        }
                                    }
                                }
                                catch {  // swallow. Some just don't like it...
                                }
                            }
                            else
                            {
                                LocationCurve crv = loc as LocationCurve;
                                if (crv != null)
                                {
                                    if (crv.Curve.IsBound)
                                    {
                                        p1 = crv.Curve.GetEndPoint(0);
                                        XYZ p2 = crv.Curve.GetEndPoint(1);
                                        lp = Utilities.RevitUtils.SerializePoint(p1);
                                        lp2 = Utilities.RevitUtils.SerializePoint(p2);
                                    }
                                }
                                else
                                {
                                    if (box == null)
                                    {
                                        // ok, special case one: Grid
                                        if (e is Grid)
                                        {
                                            Grid g = e as Grid;
                                            p1 = g.Curve.GetEndPoint(0);
                                            XYZ p2 = g.Curve.GetEndPoint(1);
                                            lp = Utilities.RevitUtils.SerializePoint(p1);
                                            lp2 = Utilities.RevitUtils.SerializePoint(p2);
                                        }
                                        else
                                        {
                                            continue; // not sure what this is???
                                        }
                                    }
                                }
                            }
                        }

                        // retrieve the level
                        Level lev = lookupLevel(e, p1);
                        string levName = String.Empty;
                        if (lev != null) levName = lev.Name;

                            var cmd = conn.CreateCommand();
                        cmd.CommandText = String.Format("INSERT INTO _objects_geom (id,BoundingBoxMin,BoundingBoxMax,Location,Location2,Level,Rotation) VALUES({0},'{1}','{2}','{3}','{4}','{5}',{6})", e.Id.IntegerValue, bbMin, bbMax, lp, lp2, escapeQuote(levName), rotation.ToString(CultureInfo.InvariantCulture));

                        if (_logLevel == Utilities.Settings.LogLevel.Verbose) _doc.Application.WriteJournalComment(cmd.CommandText,false);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        private Level lookupLevel(Element e, XYZ pt)
        {
            // given the Element, figure out the level if possible.
            if (e.LevelId != ElementId.InvalidElementId) return e.Document.GetElement(e.LevelId) as Level;
            // otherwise, let's see if we can get it from the location point.

            if (pt == null) return null; // we don't know.

            if (_allLevels == null)
            {
                FilteredElementCollector coll = new FilteredElementCollector(_doc);
                coll.OfClass(typeof(Level));

                _allLevels = coll.Cast<Level>().ToList();
            }

            // we want the next level down from the z value...
            Level lev = Utilities.RevitUtils.GetNextLevelDown(pt, _allLevels);

            return lev;
        }
        
        private string escapeQuote(string input)
        {
            return input.Replace("'", "''");
        }

        #endregion
    }
}
