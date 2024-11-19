using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.IO;

namespace Metamorphosis.Utilities
{
    /// <summary>
    /// This class exists to help with schema updates to the SQLite database used for storing results.
    /// In some cases, it is necessary to upgrade the database structure in order to make things work.
    /// </summary>
    internal static class DataUtility
    {
        internal static Version CurrentVersion = new Version(1, 1);

        private static Assembly _CurrentAsm = System.Reflection.Assembly.GetExecutingAssembly();
        private static Dictionary<Version, string> _UpgradeScripts = new Dictionary<Version, string>() { { new Version(1, 0), "Metamorphosis.DBScript.UpgradeToV1.txt" },
            { new Version(1,1), "Metamorphosis.DBScript.UpgradeToV1.1.txt" } };
        
        internal static void UpgradeFrom(SQLiteConnection conn, Version v, Action<string> log)
        {
            var todo = _UpgradeScripts.Where(s => s.Key > v).OrderBy(s => s.Key).ToList();
            if (log != null) log("=> There are " + todo.Count + " upgrades to the database.");


            bool fixNameSpace = false;
            if (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name != "Metamorphosis")
            {
                fixNameSpace = true;
            }

            foreach( var script in todo )
            {
                string val = script.Value;
                if (fixNameSpace) val = val.Replace("Metamorphosis.", "MetamorphosisCore.");
                if (log != null) log("   => Version: " + script.Key + " Update: " + val);

                
                if (System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceInfo(val) == null)
                {
                    if (log != null) log("ISSUE: Unable to find desired script: " + val);
                    continue;
                }
                string[] statements = ReadSQLScript(val);
                if (statements == null)
                {
                    if (log != null) log("ISSUE: Unable to find desired script: " + val);
                    continue;
                }

                foreach( string sql in statements )
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }

                // then, after that, let's update the specific headers table explicitly.
                var updatecmd = conn.CreateCommand();
                updatecmd.CommandText = "UPDATE _objects_header SET Value = \"" + script.Key.Major + "." + script.Key.Minor + "\" WHERE Keyword = \"SchemaVersion\"";
                updatecmd.ExecuteNonQuery();

                if (log != null) log("=> Completed update: " + script.Key);
            }
        }

        internal static string[] ReadSQLScript(string name)
        {
            
        
            Stream s = _CurrentAsm.GetManifestResourceStream(name);

            if (s == null) return null;
            string sql = null;
            using (StreamReader sr = new StreamReader(s))
            {
                sql = sr.ReadToEnd();
            }


            return sql.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        }
    }
    
}
