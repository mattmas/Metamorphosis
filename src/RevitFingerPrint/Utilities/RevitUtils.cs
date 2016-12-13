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

        }
    }
}
