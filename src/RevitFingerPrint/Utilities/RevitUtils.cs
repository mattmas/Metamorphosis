using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Globalization;

namespace Metamorphosis.Utilities
{
    internal static class RevitUtils
    {
        internal static String SerializePoint(XYZ pt)
        {
            if (pt == null) return String.Empty;
            return pt.X.ToString(CultureInfo.InvariantCulture) + "," + pt.Y.ToString(CultureInfo.InvariantCulture) + "," + pt.Z.ToString(CultureInfo.InvariantCulture);
        }

        internal static String SerializeBoundingBox(BoundingBoxXYZ box)
        {
            if (box == null) return String.Empty;

            return box.Min.X.ToString(CultureInfo.InvariantCulture) + "," + box.Min.Y.ToString(CultureInfo.InvariantCulture) + "," + box.Min.Z.ToString(CultureInfo.InvariantCulture) + "," +
                box.Max.X.ToString(CultureInfo.InvariantCulture) + "," + box.Max.Y.ToString(CultureInfo.InvariantCulture) + "," + box.Max.Z.ToString(CultureInfo.InvariantCulture);
        }

        internal static BoundingBoxXYZ DeserializeBoundingBox(string input)
        {
            if (String.IsNullOrEmpty(input)) return null;

            string[] values = input.Split(',');

            if (values.Length == 6)
            {
                BoundingBoxXYZ box = new BoundingBoxXYZ()
                {
                    Min = new XYZ(Double.Parse(values[0]), Double.Parse(values[1]), Double.Parse(values[2])),
                    Max = new XYZ(Double.Parse(values[3]), Double.Parse(values[4]), Double.Parse(values[5]))
                };

                return box;
            }

            return null;
        }

        internal static string SerializeMove( XYZ from, XYZ to )
        {
            return from.X.ToString(CultureInfo.InvariantCulture) + "," + from.Y.ToString(CultureInfo.InvariantCulture) + "," + from.Z.ToString(CultureInfo.InvariantCulture) + "," +
                to.X.ToString(CultureInfo.InvariantCulture) + "," + to.Y.ToString(CultureInfo.InvariantCulture) + "," + to.Z.ToString(CultureInfo.InvariantCulture);

        }

        internal static string SerializeDoubleMove( XYZ from1, XYZ to1, XYZ from2, XYZ to2 )
        {
            // in the case where two locationpoints are moving, capture both, in an array of 12.
            return from1.X.ToString(CultureInfo.InvariantCulture) + "," + from1.Y.ToString(CultureInfo.InvariantCulture) + "," + from1.Z.ToString(CultureInfo.InvariantCulture) + "," +
                to1.X.ToString(CultureInfo.InvariantCulture) + "," + to1.Y.ToString(CultureInfo.InvariantCulture) + "," + to1.Z.ToString(CultureInfo.InvariantCulture) + "," +
                from2.X.ToString(CultureInfo.InvariantCulture) + "," + from2.Y.ToString(CultureInfo.InvariantCulture) + "," + from2.Z.ToString(CultureInfo.InvariantCulture) + "," +
                to2.X.ToString(CultureInfo.InvariantCulture) + "," + to2.Y.ToString(CultureInfo.InvariantCulture) + "," + to2.Z.ToString(CultureInfo.InvariantCulture);

        }

        internal static bool DeSerializeMove( string input, out XYZ from1, out XYZ to1, out XYZ from2, out XYZ to2 )
        {
            from1 = null; to1 = null; from2 = null; to2 = null;

            if (String.IsNullOrEmpty(input)) return false;

            string[] values = input.Split(',');
            if ((values.Length != 6) && (values.Length != 12)) return false;

            from1 = new XYZ(Double.Parse(values[0]), Double.Parse(values[1]), Double.Parse(values[2]));
            to1 = new XYZ(Double.Parse(values[3]), Double.Parse(values[4]), Double.Parse(values[5]));

            if (values.Length==12)
            {
                from2 = new XYZ(Double.Parse(values[6]), Double.Parse(values[7]), Double.Parse(values[8]));
                to2 = new XYZ(Double.Parse(values[9]), Double.Parse(values[10]), Double.Parse(values[11]));

            }

            return true;
        }

        internal static Level GetNextLevelDown( XYZ pt, IList<Level> levels)
        {
            if (pt == null) return null;
            if (levels == null) return null;

            // we want the next level down from the z value...
            IList<Level> levelsBelow = levels.Where(v => v.Elevation <= pt.Z).ToList();
            Level lev = null;
            if (levelsBelow.Count == 0) return null;
            if (levelsBelow.Count == 1) return levelsBelow[0];

            // otherwise look for which one is closest.
            lev = levelsBelow.Aggregate((x, y) => Math.Abs(x.Elevation - pt.Z) < Math.Abs(y.Elevation - pt.Z) ? x : y);

            return lev;
        }

        internal static IList<Document> GetProjectsInMemory(Autodesk.Revit.ApplicationServices.Application app)
        {
            List<Document> docs = new List<Document>();
            foreach( Document doc in app.Documents )
            {
                if (doc.IsFamilyDocument == false) docs.Add(doc);
            }

            return docs;
        }

        internal static void GetExtents(Autodesk.Revit.UI.UIApplication uiApp, out int x, out int y)
        {
#if REVIT2017
            try
            {
                x = uiApp.DrawingAreaExtents.Left;
                y = uiApp.DrawingAreaExtents.Top;
            }
            catch
            {
                x = 10;
                y = 10;
            } 
#endif
#if REVIT2016
            try
            {
                x = uiApp.DrawingAreaExtents.Left;
                y = uiApp.DrawingAreaExtents.Top;
            }
            catch
            {
                x = 10;
                y = 10;
            }
#endif
        }

        internal static Solid CreateSolidFromBox( Autodesk.Revit.ApplicationServices.Application app, BoundingBoxXYZ box)
        {
            // create a set of curves from the base of the box.
            
            // presumes an untransformed box.
            XYZ A1 = box.Min;
            XYZ A2 = new XYZ(box.Max.X, box.Min.Y, box.Min.Z);
            XYZ A3 = new XYZ(box.Max.X, box.Max.Y, box.Min.Z);
            XYZ A4 = new XYZ(box.Min.X, box.Max.Y, box.Min.Z);

            List<Curve> crvs = new List<Curve>();
            
            crvs.Add(Line.CreateBound(A1, A2));
            crvs.Add(Line.CreateBound(A2, A3));
            crvs.Add(Line.CreateBound(A3, A4));
            crvs.Add(Line.CreateBound(A4, A1));

            CurveLoop loop = CurveLoop.Create(crvs);
            List<CurveLoop> loops = new List<CurveLoop>() { loop };

            Solid s = GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, (box.Max.Z - box.Min.Z));

            return s;
        }

        
    }
}
