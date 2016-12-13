using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metamorphosis.Utilities
{
    public static class Utility
    {
        public static IntPtr GetMainWindowHandle()
        {

            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
            return p.MainWindowHandle;
        }

        /// <summary>
        /// Validate that the given path is acceptable
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidPath(string path)
        {
            try
            {
                var info = new System.IO.FileInfo(path);

                if (info != null) return true;
            }
            catch (Exception)
            {

            }
            return false;
        }
    }
}
