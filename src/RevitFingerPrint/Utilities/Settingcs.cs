using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Metamorphosis.Utilities
{
    internal static class Settings
    {
        internal enum LogLevel { Basic, Verbose};
        private static XmlDocument _doc;


        internal static Autodesk.Revit.DB.Color GetColor( object typeName)
        {
            readData();

            string name = typeName.ToString();

            if (_doc == null)
            {
                System.Diagnostics.Debug.WriteLine("Did not find settings document!");
                return new Autodesk.Revit.DB.Color(255, 0, 0); // default
            }

            XmlNode node = _doc.SelectSingleNode("/Settings/ColorChoices/ChangeType[@name='" + name + "']");

            if (node == null)
            {
                System.Diagnostics.Debug.WriteLine("Did not find setting: " + name);
                return new Autodesk.Revit.DB.Color(255, 0, 0);
            }

            string val = node.Attributes["color"].Value;


            System.Drawing.Color c = System.Drawing.Color.FromName(val);
            if (c != null)
            {
                return new Autodesk.Revit.DB.Color(c.R, c.G, c.B);
            }
            else
            {
                // see if it's RGB
                string[] vals = val.Split(',');
                if (vals.Length == 3)
                {
                    int r, g, b;
                    if ( (Int32.TryParse(vals[0], out r)) && (Int32.TryParse(vals[1], out g)) && (Int32.TryParse(vals[2], out b)))
                    { 
                        return new Autodesk.Revit.DB.Color((byte)r, (byte)g, (byte)b);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("Unable to understand color: " + val);

            return new Autodesk.Revit.DB.Color(255, 0, 0);
        }

        public static string GetDefaultCategories()
        {
            readData();

          
            if (_doc == null)
            {
                System.Diagnostics.Debug.WriteLine("Did not find settings document!");
                return null;
            }

            XmlElement elem = _doc.SelectSingleNode("/Settings/DefaultSelection") as XmlElement;

            if (elem == null) return null;

            if (String.IsNullOrEmpty(elem.InnerText)) return null;

            return elem.InnerText;

        }

        public static LogLevel GetLogLevel()
        {
            readData();


            if (_doc == null)
            {
                System.Diagnostics.Debug.WriteLine("Did not find settings document!");
                return LogLevel.Basic;
            }

            XmlElement elem = _doc.SelectSingleNode("/Settings/LogLevel") as XmlElement;

            if (elem == null) return LogLevel.Basic;

            if (String.IsNullOrEmpty(elem.InnerText)) return LogLevel.Basic;

            LogLevel level = LogLevel.Basic;
            if (Enum.TryParse<LogLevel>( elem.InnerText, out level))
            {
                return level;
            }

            return LogLevel.Basic;
        }

        private static void readData()
        {
            if (_doc != null) return;

            _doc = new XmlDocument();

            
                string filename = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings.xml");

                _doc.Load(filename);
            
            
        }
        
    }
}
