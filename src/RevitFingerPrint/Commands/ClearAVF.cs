using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace Metamorphosis.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ClearAVF : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
           try
            {
                if (commandData.Application.ActiveUIDocument != null)
                {
                    Utilities.AVFUtility.Clear(commandData.Application.ActiveUIDocument);
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex);
            }
            return Result.Failed;
        }
    }
}
