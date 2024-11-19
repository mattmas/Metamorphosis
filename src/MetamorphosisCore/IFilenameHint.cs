using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Metamorphosis
{
    public interface IFilenameHint
    {
        string GetFilenameHint(Document doc);
    }
}
