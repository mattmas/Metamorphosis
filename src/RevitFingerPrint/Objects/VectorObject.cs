using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Metamorphosis.Objects
{
    internal class VectorObject
    {

        public XYZ Origin { get; set; }
        public XYZ Vector { get; set; }

        internal VectorObject(XYZ pt, XYZ direction)
        {
            Origin = pt;
            Vector = direction;
        }

        public override string ToString()
        {
            return "O: " + Origin + " V:" + Vector;
        }
    }
}
