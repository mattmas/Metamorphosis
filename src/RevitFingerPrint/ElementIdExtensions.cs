using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metamorphosis
{
    /// <summary>
    /// a class to add static methods to deal with long vs. integer underlying values.
    /// </summary>
    public static class ElementIdExtentions
    {
        public static bool IsCategory(this ElementId elementId, BuiltInCategory cat)
        {
#if LONGELEMENTIDS
            return (elementId.Value == (long)cat);
#else
            return (elementId.IntegerValue == (int)cat);
#endif
        }

        public static bool IsNotCategory(this ElementId elementId, BuiltInCategory cat)
        {
#if LONGELEMENTIDS
            return (elementId.Value != (long)cat);
#else
            return (elementId.IntegerValue != (int)cat);
#endif
        }

        public static long AsLong(this ElementId elementId)
        {
#if LONGELEMENTIDS
            return elementId.Value;
#else
            return (long)elementId.IntegerValue;
#endif
        }

        public static Int32 AsInt32(this ElementId elementId)
        {
#if LONGELEMENTIDS
            // hopefully only in cases where a long is unlikely, like BuiltInCategories?
            return (int)elementId.Value;
#else
            return elementId.IntegerValue;
#endif
        }

        public static BuiltInCategory AsBuiltInCategory(this ElementId elementId)
        {
#if LONGELEMENTIDS
            return (BuiltInCategory)elementId.Value;
#else
            return (BuiltInCategory)elementId.IntegerValue;
#endif
        }

       
    }
}
