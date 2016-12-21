using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

namespace Metamorphosis.Utilities
{
    internal static class AVFUtility
    {
        internal enum StyleEnum { Faces, Vectors};

        private static int _SchemaId = -1;
        private const int MAX_POINTS_PER_PRIMITIVE = 975;


        #region PublicMethods
        internal static void ShowSolids(Document doc, IEnumerable<Solid> solids, IEnumerable<double> values)
        {
            SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
            if (sfm == null) sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView, 1);


            if (_SchemaId != -1)
            {
                IList<int> results = sfm.GetRegisteredResults();

                if (!results.Contains(_SchemaId))
                {
                    _SchemaId = -1;
                }
            }

            if (_SchemaId == -1)
            {
                _SchemaId = registerResults(sfm, "ShowChanges", "Description");
            }

            List<double> valueList = values.ToList();
            int i = 0;
            foreach (Solid s in solids)
            {
                double value = valueList[i];
                i++;
                FaceArray faces = s.Faces;
                Transform trf = Transform.Identity;

                foreach (Face face in faces)
                {
                    int idx = sfm.AddSpatialFieldPrimitive(face, trf);

                    IList<UV> uvPts = new List<UV>();
                    List<double> doubleList = new List<double>();
                    IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                    BoundingBoxUV bb = face.GetBoundingBox();
                    uvPts.Add(bb.Min);
                    doubleList.Add(value);
                    valList.Add(new ValueAtPoint(doubleList));
                    FieldDomainPointsByUV pnts = new FieldDomainPointsByUV(uvPts);
                    FieldValues vals = new FieldValues(valList);

                    sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, _SchemaId);
                }
            }

            updateView(doc.ActiveView, StyleEnum.Faces);

        }

        internal static void ShowVectors(Document doc, IList<Objects.VectorObject> points, bool scaleVectors)
        {
            double viewScale = 12.0 / Convert.ToDouble(doc.ActiveView.Scale);

           
            SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
            if (sfm == null) sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView, 1);

            if (sfm == null) throw new System.ApplicationException("SFM still null!");
            sfm.Clear();

            // schema
            if (_SchemaId != -1)
            {
                IList<int> results = sfm.GetRegisteredResults();

                if (!results.Contains(_SchemaId))
                {
                    _SchemaId = -1;
                }
            }

            if (_SchemaId == -1)
            {
                _SchemaId = registerResults(sfm, "ShowChanges", "Description");
            }


           

            IList<VectorAtPoint> valList = new List<VectorAtPoint>();
            List<XYZ> dummyList = new List<XYZ>();
            dummyList.Add(XYZ.BasisZ);

            FieldDomainPointsByXYZ pnts = null;

            FieldValues vals = null;

            List<XYZ> tmpXYZ = new List<XYZ>();
            List<string> tmpNames = new List<String>();

            int idx = sfm.AddSpatialFieldPrimitive();
            int localPointCount = 0;
            int max = points.Count;

            for (int i = 0; i < max; i++)
            {

                tmpXYZ.Add(points[i].Origin);


                if (scaleVectors)
                {
                    dummyList[0] = points[i].Vector.Multiply(viewScale); 
                }
                else
                {
                    dummyList[0] = points[i].Vector;
                }
                valList.Add(new VectorAtPoint(dummyList));

                if (localPointCount > MAX_POINTS_PER_PRIMITIVE)
                {

                    pnts = new FieldDomainPointsByXYZ(tmpXYZ);
                    vals = new FieldValues(valList);
                    sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, _SchemaId);

                    // create a new primitive
                    idx = sfm.AddSpatialFieldPrimitive();

                    // reset
                    localPointCount = 0;
                    tmpXYZ = new List<XYZ>();
                    valList = new List<VectorAtPoint>();
                }


                localPointCount++;

            }

            // do it one more time if there are leftovers
            if (tmpXYZ.Count > 0)
            {

                pnts = new FieldDomainPointsByXYZ(tmpXYZ);
                vals = new FieldValues(valList);
                sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, _SchemaId);
            }



          

            updateView(doc.ActiveView, StyleEnum.Vectors);

        }
        #endregion

        #region PrivateMethods
        private static int registerResults(SpatialFieldManager sfm, string name, string description)
        {
            IList<int> results = sfm.GetRegisteredResults();
            if ((results != null) && (results.Count > 0))
            {
                for (int i = 0; i < results.Count; i++)
                {
                    try
                    {
                        if (sfm.GetResultSchema(i).Name.ToUpper() == name.ToUpper()) return i;
                    }
                    catch { } // ran into cases in 2015 where this produced "Non-existent schema"
                }
            }

            AnalysisResultSchema resultSchema1 = new AnalysisResultSchema(name, description);
            int result = sfm.RegisterResult(resultSchema1);
            return result;


        }

        private static void updateView(View v, StyleEnum style)
        {

            if (testDocNeedsInitializing(v.Document)) createAllStyles(v.Document);

            
            //does the current view have an analysis style?
            Parameter avf = v.get_Parameter(BuiltInParameter.VIEW_ANALYSIS_DISPLAY_STYLE);
            Document doc = v.Document;

            if (avf != null)
            {
                ElementId eid = avf.AsElementId();

                string name = "";
                switch (style)
                {
                    case StyleEnum.Faces:
                        name = "SolidView";
                        break;

                    case StyleEnum.Vectors:
                        name = "VectorView";
                        break;

                    default:
                        throw new ApplicationException("Unexpected Display Style: " + style);
                }

                ElementId pc = AnalysisDisplayStyle.FindByName(doc, name);
                
                

                if (pc.IntegerValue > 0)
                {
                    if (avf.AsElementId() != pc)
                    {
                        Transaction t = null;
                        if (v.Document.IsModifiable == false)
                        {
                            t = new Transaction(v.Document, "Set AVF view style");
                            t.Start();
                        }
                        bool success = avf.Set(pc);
                        if (t != null) t.Commit();
                    }
                    
                }
            }
        }

        private static bool testDocNeedsInitializing(Document doc)
        {
            // does it already exist?  look for the latest kind...
            return (AnalysisDisplayStyle.FindByName(doc, "SolidView").IntegerValue < 0);
        }

        private static void createAllStyles(Document doc)
        {
            // see if we need a transaction
            Transaction t = null;
            if (doc.IsModifiable == false)
            {
                t = new Transaction(doc, "Create Display Styles");
                t.Start();
            }

            // create the styles...
            if (AnalysisDisplayStyle.FindByName(doc, "SolidView").IntegerValue < 0) createSolidDisplayStyle(doc, "SolidView");
            if (AnalysisDisplayStyle.FindByName(doc, "VectorView").IntegerValue < 0) createVectorDisplayStyle(doc, "VectorView");

            if (t != null)
            {
                t.Commit();
            }
        }

        private static Element createSolidDisplayStyle(Document doc, string name)
        {
            AnalysisDisplayColoredSurfaceSettings style = new AnalysisDisplayColoredSurfaceSettings();

            style.ShowGridLines = true;

            AnalysisDisplayColorSettings colors = new AnalysisDisplayColorSettings();
            colors.ColorSettingsType = AnalysisDisplayStyleColorSettingsType.GradientColor;
            colors.MinColor = new Color((byte)255, (byte)165, (byte)0);
            colors.MaxColor = new Color((byte)0, (byte)0, (byte)255);   // orange to blue

            // we need to create it
            Element pc =
                AnalysisDisplayStyle.CreateAnalysisDisplayStyle(doc, name,
                                                                style,
                                                                colors,
                                                                new AnalysisDisplayLegendSettings() { ShowLegend = false });

            return pc;
        }

        private static Element createVectorDisplayStyle(Document doc, string name)
        {
            AnalysisDisplayVectorSettings style = new AnalysisDisplayVectorSettings();

            style.VectorOrientation = AnalysisDisplayStyleVectorOrientation.Linear;
            style.VectorPosition = AnalysisDisplayStyleVectorPosition.FromDataPoint;
            style.VectorTextType = AnalysisDisplayStyleVectorTextType.ShowNone;
            

            AnalysisDisplayColorSettings colors = new AnalysisDisplayColorSettings();
            colors.ColorSettingsType = AnalysisDisplayStyleColorSettingsType.GradientColor;
            colors.MinColor = new Color((byte)255, (byte)165, (byte)0);
            colors.MaxColor = new Color((byte)0, (byte)0, (byte)255);   // orange to blue

            // we need to create it
            Element pc =
                AnalysisDisplayStyle.CreateAnalysisDisplayStyle(doc, name,
                                                                style,
                                                                colors,
                                                                new AnalysisDisplayLegendSettings() { ShowLegend = false });

            return pc;
        }

        #endregion
    }
}
