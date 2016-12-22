using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Metamorphosis.Objects;
using System.Data.SQLite;

namespace Metamorphosis
{
    public class ComparisonMaker
    {
        #region Declarations
        private Document _doc;
        private string _filename;
        private Dictionary<int, string> _parameterDict = new Dictionary<int, string>();
        private Dictionary<int, string> _valueDict = new Dictionary<int, string>();
        private Dictionary<string, int> _categoryCount = new Dictionary<string, int>();
        private HashSet<string> _requestedCategoryNames = new HashSet<string>();
        private IList<Level> _allLevels;




        // TODO: separate categories by dictionary of category, elementid, parameter
        private Dictionary<int, RevitElement> _idValues = new Dictionary<int, RevitElement>();
        private Dictionary<int, RevitElement> _currentElems = new Dictionary<int, RevitElement>();


        #endregion

        #region Accessors
        public Boolean AllCategories { get; set; } = true;
        public IList<Category> RequestedCategories { get; set; }
        #endregion

        #region Constructor
        public ComparisonMaker(Document doc, string previousFile)
        {
            _doc = doc;
            _filename = previousFile;
        }
        #endregion

        #region PublicMethods
        public IList<Change> Compare()
        {
            // we want to load up a previous model, 
            readPrevious();

            // read the existing data into memory
            readModel();

            // make our comparisons
            return compareData();


        }

        public void Serialize(string filename, IList<Change> changes)
        {
            System.IO.File.WriteAllText(filename, Serialize(changes));
        }

        public string Serialize(IList<Change> changes)
        {
            // build the change summary:

            ChangeSummary summary = new ChangeSummary();
            summary.ModelName = _doc.Title;
            summary.ModelPath = _doc.PathName;
            summary.NumberOfChanges = changes.Count;
            summary.PreviousFile = _filename;
            summary.ComparisonDate = DateTime.Now;
            summary.Changes = changes;
            summary.ModelSummary = _categoryCount;
            summary.LevelNames = _allLevels.OrderBy(a => a.Elevation).Select(a => a.Name).ToList();


            string result = Newtonsoft.Json.JsonConvert.SerializeObject(summary);
            //var serialize = new System.Web.Script.Serialization.JavaScriptSerializer();
            //string result = serialize.Serialize(summary);

            return result;

        }
        #endregion

        #region PrivateMethods
        private IList<Change> compareData()
        {
            List<Change> changes = new List<Change>();

            foreach (var currentPair in _currentElems)
            {
                var current = currentPair.Value;
                // find it from the previous
                if (_idValues.ContainsKey(currentPair.Key))
                {
                    // it exists, so let's compare
                    var previous = _idValues[currentPair.Key];

                    var change = compareElements(current, previous);
                    if (change != null) changes.Add(change);
                }
                else
                {
                    // it has been removed.
                    changes.Add(buildNew(current));
                }
            }

            // now look for deleted items
            foreach (var previousPair in _idValues)
            {
                if (_currentElems.ContainsKey(previousPair.Key) == false)
                {
                    if (!AllCategories && (_requestedCategoryNames.Contains(previousPair.Value.Category) == false)) continue; // do not include
                    changes.Add(buildDeleted(previousPair.Value));
                }
            }

            return changes;
        }

        private Change buildDeleted(RevitElement element)
        {
            Change c = new Change() { ElementId = element.ElementId, Category = element.Category, ChangeType = Change.ChangeTypeEnum.DeletedElement,
                Level = (element.Level != null) ? element.Level : "", IsType = element.IsType };
            c.BoundingBoxDescription = Utilities.RevitUtils.SerializeBoundingBox(element.BoundingBox);
            return c;
        }
        private Change compareElements(RevitElement current, RevitElement previous)
        {
            // compare the parameter values

            // at present, we can only compare string values
            Change c = compareParameters(current, previous);

            if (c != null) return c;

            c = compareGeometry(current, previous);

            return c;

        }

        private Change buildNew(RevitElement current)
        {
            Element e = _doc.GetElement(new ElementId(current.ElementId));

            Change c = new Change() { ElementId = current.ElementId, UniqueId = e.UniqueId, Category = current.Category, ChangeType = Change.ChangeTypeEnum.NewElement, Level = (current.Level != null) ? current.Level : "", IsType = current.IsType };
            c.BoundingBoxDescription = Utilities.RevitUtils.SerializeBoundingBox(current.BoundingBox);

            return c;
        }

        private Change compareParameters(RevitElement current, RevitElement previous)
        {
            StringBuilder description = null;
            int numParmsChanged = 0;
            foreach (var pair in current.Parameters)
            {
                if (previous.Parameters.ContainsKey(pair.Key))
                {
                    // test if they match
                    if (current.Parameters[pair.Key] != previous.Parameters[pair.Key])
                    {
                        if (description == null) description = new StringBuilder();
                        numParmsChanged++;
                        if (numParmsChanged > 1) description.Append(", ");
                        description.Append(pair.Key + " From: " + previous.Parameters[pair.Key] + " to " + current.Parameters[pair.Key]);

                    }

                }
            }
            if (numParmsChanged > 0)
            {
                Element e = _doc.GetElement(new ElementId(current.ElementId));
                Change c = new Change()
                {
                    ElementId = current.ElementId,
                    UniqueId = e.UniqueId,
                    Category = (e.Category != null) ? e.Category.Name : "(none)",
                    ChangeType = Change.ChangeTypeEnum.ParameterChange,
                    ChangeDescription = description.ToString(),
                    Level = (current.Level != null) ? current.Level : "",
                    IsType = current.IsType
                };
                c.BoundingBoxDescription = Utilities.RevitUtils.SerializeBoundingBox(current.BoundingBox);

                return c;
            }

            return null;
        }

        private Change compareGeometry(RevitElement current, RevitElement previous)
        {
            // try to do a comparison based on bounding boxes and locations...
            // this is CERTAINLY imperfect

            double tolerance = 0.0006;  // decimal feet - 1/128"?

            double dist = -1;

            if (didMove(current.LocationPoint, previous.LocationPoint, tolerance, out dist))
            {

                Change c = new Change()
                {
                    ChangeType = Change.ChangeTypeEnum.Move,
                    Category = current.Category,
                    ElementId = current.ElementId,
                    UniqueId = current.UniqueId,
                    ChangeDescription = "Location Offset " + dist + " ft."
                };
                if (current.BoundingBox != null) c.BoundingBoxDescription = Utilities.RevitUtils.SerializeBoundingBox(current.BoundingBox);

                // we want to check if the LocationPoint2 also moved?
                if ((current.LocationPoint2 != null) && (previous.LocationPoint2 != null) &&
                    (didMove(current.LocationPoint2, previous.LocationPoint2, tolerance, out dist)))
                {

                    // both moved. record both.
                    c.MoveDescription = Utilities.RevitUtils.SerializeDoubleMove(previous.LocationPoint, current.LocationPoint,
                                                                                  previous.LocationPoint2, current.LocationPoint2);

                }
                else
                {
                    // single move.
                    c.MoveDescription = Utilities.RevitUtils.SerializeMove(previous.LocationPoint, current.LocationPoint);
                }

                return c;

            }

            // only a move of the second one...
            if ((current.LocationPoint2 != null) && (previous.LocationPoint2 != null) && 
                    didMove(previous.LocationPoint2, current.LocationPoint2, tolerance, out dist))
            {
                
                    Change c = new Change()
                    {
                        ChangeType = Change.ChangeTypeEnum.Move,
                        Category = current.Category,
                        ElementId = current.ElementId,
                        UniqueId = current.UniqueId,
                        ChangeDescription = "Location Offset " + dist + " ft.",
                        Level = current.Level
                    };
                    if (current.BoundingBox != null) c.BoundingBoxDescription = Utilities.RevitUtils.SerializeBoundingBox(current.BoundingBox);

                // only one side moved though...
                c.MoveDescription = Utilities.RevitUtils.SerializeMove(previous.LocationPoint2, current.LocationPoint2);
                    return c;
                
            }

            // check rotation
            float rotationTolerance = 0.0349f; // two degrees?
            float rotationDiff = current.Rotation - previous.Rotation;
            
            if (Math.Abs(rotationDiff) > rotationTolerance)
            {
                Change c = new Change()
                {
                    ChangeType = Change.ChangeTypeEnum.Rotate,
                    Category = current.Category,
                    ElementId = current.ElementId,
                    UniqueId = current.UniqueId,
                    ChangeDescription = "Rotation: " + ((rotationDiff) * 180.0 / Math.PI).ToString("F2") + " degrees",
                    Level = current.Level
                };
                if (current.BoundingBox != null) c.BoundingBoxDescription = Utilities.RevitUtils.SerializeBoundingBox(current.BoundingBox);
                c.RotationDescription = Utilities.RevitUtils.SerializeRotation(current.LocationPoint, XYZ.BasisZ, rotationDiff);

                return c;
            }

            // now the bounding box...
            if ((current.BoundingBox != null) && (previous.BoundingBox != null))
            {
                double maxDist = Math.Max(current.BoundingBox.Min.DistanceTo(previous.BoundingBox.Min),
                                           current.BoundingBox.Max.DistanceTo(previous.BoundingBox.Max));

                if (maxDist > tolerance)
                {
                    Change c = new Change()
                    {
                        ChangeType = Change.ChangeTypeEnum.GeometryChange,
                        Category = current.Category,
                        ElementId = current.ElementId,
                        UniqueId = current.UniqueId,
                        ChangeDescription = "BoundingBox Offset " + maxDist + " ft.",
                    };
                    c.BoundingBoxDescription = Utilities.RevitUtils.SerializeBoundingBox(current.BoundingBox);
                    return c;
                }
            }

            return null;

        }

        private bool didMove(XYZ oldPoint, XYZ newPoint, double tolerance, out double distance)
        {
            distance = -1;
            if (newPoint == null) return false;
            if (oldPoint == null) return false;

            distance = newPoint.DistanceTo(oldPoint);
            if (distance > tolerance)
            {
                return true;
            }

            return false;
        }

        private void readModel()
        {

            if (!AllCategories)
            {
                foreach (var c in RequestedCategories) _requestedCategoryNames.Add(c.Name);
                
            }
            
            FilteredElementCollector coll = new FilteredElementCollector(_doc);
            coll.WhereElementIsNotElementType();

            var elems = coll.ToElements().Where(e => e.Category != null).ToList();
            Dictionary<ElementId, Element> typesToCheck = new Dictionary<ElementId, Element>();
            foreach( var elem in elems )
            {
                if (elem.Category == null) continue; // we don't want this.

                ElementId typeId = elem.GetTypeId();
                if (typeId != ElementId.InvalidElementId)
                {
                    if (typesToCheck.ContainsKey(typeId) == false) typesToCheck[typeId] = _doc.GetElement(typeId);
                }
            }

            //////////////////
            populateExisting(typesToCheck.Values.ToList(), true);
            populateExisting(elems, false);
            
        }

        private void populateExisting(IList<Element> elems, bool isTypes)
        {
            // get the levels:
            if (_allLevels == null)
            {
                FilteredElementCollector coll = new FilteredElementCollector(_doc);
                coll.OfClass(typeof(Level));

                _allLevels = coll.Cast<Level>().ToList();
            }

            foreach ( var e in elems)
            {
                Category c = e.Category;
                if (e is FamilySymbol)
                {
                    FamilySymbol fs = e as FamilySymbol;
                    c = fs.Family.FamilyCategory;
                }

                if (c == null) continue; // we don't want these things?

                if (!AllCategories && (_requestedCategoryNames.Contains(c.Name) == false)) continue; // not appropriate

                // count the category usage, for reference.
                if (_categoryCount.ContainsKey(c.Name) == false) _categoryCount[c.Name] = 0;
                _categoryCount[c.Name]++;

                var revitElem = new RevitElement() { ElementId = e.Id.IntegerValue, Category = (c != null) ? c.Name : "(none)" };
                _currentElems.Add(e.Id.IntegerValue, revitElem);

                foreach( var p in e.GetOrderedParameters())
                {
                    //Quick and Dirty - will need to call different stuff for each thing
                    try
                    {
                        if (p.Definition == null) continue; // we don't want this!
                        string definition = p.Definition.Name;
                        string val = null;
                        switch ( p.StorageType)
                        {
                            case StorageType.String:
                                val = p.AsString();
                                break;

                            default:
                                val = p.AsValueString();
                                break;
                        }

                        if (val == null) val = "(n/a)";

                        revitElem.Parameters[p.Definition.Name] = val;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Weird database: " + ex);
                    }
                }
                revitElem.IsType = isTypes;

                if (!isTypes)
                {
                    var box = e.get_BoundingBox(null);
                    if (box != null) revitElem.BoundingBox = box;

                    LocationPoint lp = e.Location as LocationPoint;
                    if (lp != null)
                    {
                        revitElem.LocationPoint = lp.Point;
                        if (e is FamilyInstance)
                        {
                            revitElem.Rotation = (float)lp.Rotation;
                        }
                    }
                    else
                    {
                        LocationCurve lc = e.Location as LocationCurve;
                        if (lc != null)
                        {
                            if (lc.Curve.IsBound)
                            {
                                revitElem.LocationPoint = lc.Curve.GetEndPoint(0);
                                revitElem.LocationPoint2 = lc.Curve.GetEndPoint(1);
                            }
                        }
                    }


                    if (e.LevelId != ElementId.InvalidElementId)
                    {
                        revitElem.Level = _doc.GetElement(e.LevelId).Name;
                    }
                    else
                    {
                        if (revitElem.LocationPoint != null)
                        {


                            // we want the next level down from the z value...
                            Level lev = Utilities.RevitUtils.GetNextLevelDown(revitElem.LocationPoint, _allLevels);
                            if (lev != null) revitElem.Level = lev.Name;
                        }
                    }
                }
                
            }
        }
        private void readPrevious()
        {
            readParameters();
            readValues();
            readElements();
            readGeometry();
        }

        private void readParameters()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _filename + ";Version=3;"))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id,name FROM _objects_attr";

                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    _parameterDict[id] = name;
                }

            }
        }

        private void readValues()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _filename + ";Version=3;"))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id,value FROM _objects_val";

                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string value = reader.GetString(1);
                    _valueDict[id] = value;
                }

            }
        }

        private void readElements()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _filename + ";Version=3;"))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id,entity_id,attribute_id,value_id FROM _objects_eav";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int entity_id = reader.GetInt32(1);
                        int attribute_id = reader.GetInt32(2);
                        int value_id = reader.GetInt32(3);

                        if (_idValues.ContainsKey(entity_id) == false) _idValues[entity_id] = new RevitElement() { ElementId = entity_id };

                        _idValues[entity_id].ParameterValueIds[attribute_id] = value_id;
                        _idValues[entity_id].Parameters[_parameterDict[attribute_id]] = _valueDict[value_id];

                    }
                }


                /// read the ID information for each element.
                cmd = conn.CreateCommand();
                cmd.CommandText = "select id,external_id,category,isType FROM _objects_id";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        if (_idValues.ContainsKey(id) == false) continue; // not sure how.
                        string guid = reader.GetString(1);
                        string cat = reader.GetString(2);
                        int isType = reader.GetInt32(3);

                        var elem = _idValues[id];
                        elem.Category = cat;
                        elem.UniqueId = guid;
                        elem.IsType = (isType == 1);
                    }
                }
            }
        }

        private void readGeometry()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _filename + ";Version=3;"))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "select id,BoundingBoxMin,BoundingBoxMax,Location,Location2,Level,Rotation FROM _objects_geom";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int entity_id = reader.GetInt32(0);

                        if (_idValues.ContainsKey(entity_id) == false) continue; // not sure how, but let's protect just in case.
                        string bbMin = reader.GetString(1);
                        string bbMax = reader.GetString(2);
                        string lp = reader.GetString(3);
                        string lp2 = reader.GetString(4);
                        string levName = reader.GetString(5);
                        float rot = reader.GetFloat(6);

                        RevitElement elem = _idValues[entity_id];

                        if (!String.IsNullOrEmpty(bbMin)) elem.BoundingBox = new BoundingBoxXYZ()
                        {
                            Min = parsePoint(bbMin),
                            Max = parsePoint(bbMax)
                        };
                        if (!String.IsNullOrEmpty(lp)) elem.LocationPoint = parsePoint(lp);
                        if (!String.IsNullOrEmpty(lp2)) elem.LocationPoint2 = parsePoint(lp2);

                        elem.Level = levName;
                        elem.Rotation = rot;
                    }
                }
            }
        }

        /// <summary>
        /// From a string point, return an XYZ 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private XYZ parsePoint(string input)
        {
            if (String.IsNullOrEmpty(input)) return null;

            string[] pieces = input.Split(',');
            if (pieces.Length != 3) return null;

            return new XYZ(Double.Parse(pieces[0]), Double.Parse(pieces[1]), Double.Parse(pieces[2]));
        }
        #endregion
    }
}
