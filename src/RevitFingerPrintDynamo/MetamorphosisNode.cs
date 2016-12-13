using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.DesignScript.Runtime;

namespace MetamorphosisDynamo
{
    public static class MetaMorphosisDynamo
    {
        /// <summary>
        /// Creates a snapshot file of the current model
        /// </summary>
        /// <param name="filename">OPTIONAL: Specify the snapshot filename</param>
        /// <returns>The snapshot filename</returns>
        public static string Snapshot(string filename = null)
        {
            var doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;

            
            
            string file = filename;
            if (file == null)
            {
                if (String.IsNullOrEmpty(doc.PathName)) throw new ApplicationException("Unable to determine the filename from the Revit file- which has no path!");

                string folder = Path.GetDirectoryName(doc.PathName);
                string name = Path.GetFileNameWithoutExtension(doc.Title) + "_" + DateTime.Now.ToString("yyyyMMdd_hhmm") + ".sdb";
                file = Path.Combine(folder, name);

                if (File.Exists(file))
                {
                    // go for something a little more unique.
                    name = Path.GetFileNameWithoutExtension(doc.Title) + "_" + DateTime.Now.Ticks + ".sdb";
                    file = Path.Combine(folder, file);
                }
            }
            else
            {
              

                // did somebody pass in an unescaped string?
                if (file.Any(c => char.IsControl(c))) throw new ApplicationException("The specified input file needs to have 'escaped' backslashes?");
                if (File.Exists(file)) File.Delete(file);
            }

            Metamorphosis.SnapshotMaker maker = new Metamorphosis.SnapshotMaker(doc, file);
            maker.Export();

            return file;
           
        }

        
        [Autodesk.DesignScript.Runtime.MultiReturn(new string[] { "NumChanges", "JSONData" })]
        public static Dictionary<string,object> Compare(string filename, Boolean AllCategories = true, [Autodesk.DesignScript.Runtime.DefaultArgument("MetamorphosisDynamo.GetNull()")] Revit.Elements.Category[] specifiedCategories = null)
        {

            if (File.Exists(filename) == false) throw new ApplicationException("The specified file is not found: " + filename);
            var doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;

            Metamorphosis.ComparisonMaker compare = new Metamorphosis.ComparisonMaker(doc, filename);

            // determine whether all categories should be compared, or only selected categories
            compare.AllCategories = AllCategories;
            if (compare.AllCategories == false)
            {
                if ((specifiedCategories == null)||(specifiedCategories.Length==0)) throw new ApplicationException("You must specify categories");

                List<Autodesk.Revit.DB.Category> cats = new List<Autodesk.Revit.DB.Category>();
                foreach( var cat in specifiedCategories)
                {
                    var category = doc.Settings.Categories.get_Item(cat.Name);
                    if (category != null) cats.Add(category);
                }
                compare.RequestedCategories = cats;
            }

            // execute.
            var changes = compare.Compare();

            return new Dictionary<string,object>() { { "NumChanges", changes.Count}, { "JSONData", compare.Serialize(changes)} };

        }

        [IsVisibleInDynamoLibrary(false)]
        public static object GetNull()
        {
            return null;
        }

    }
}
