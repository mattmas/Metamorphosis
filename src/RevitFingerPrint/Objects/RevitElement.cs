using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metamorphosis.Objects
{
    public class RevitElement
    {
        public int ElementId { get; set; }

        public String UniqueId { get; set; }
        public string Category { get; set; }

        public Boolean IsType { get; set; }

        public string Level { get; set; }

        public string VersionGuid { get; set; }

        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public Dictionary<int, int> ParameterValueIds { get; set; } = new Dictionary<int, int>();

        public XYZ LocationPoint { get; set; }
        public XYZ LocationPoint2 { get; set; }

        public float Rotation { get; set; } = -1.0f;

        public BoundingBoxXYZ BoundingBox { get; set; }
    }
}
