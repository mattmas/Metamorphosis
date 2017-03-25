using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Metamorphosis.Utilities
{
    public class CategorySettingsFile
    {
        #region Properties
        public String Name { get; set; }
        public String Filename { get; set; }
        public IList<CategorySetting> Settings { get; set; } = new List<CategorySetting>();
        #endregion

        #region Constructors
        public CategorySettingsFile(string filename)
        {
            Name = Path.GetFileNameWithoutExtension(filename);
            Filename = filename;
        }
        #endregion


        #region Factory Methods
        public static IList<CategorySettingsFile> GetFiles()
        {
            // we are always going to get the files from the same folders:
            // ProgramData and UserRoaming

            string folder1, folder2;
            GetFolders(out folder1, out folder2);

            List<CategorySettingsFile> files = new List<CategorySettingsFile>();

            
                List<string> allFiles = new List<string>();
                if (Directory.Exists(folder1))
                {
                    allFiles.AddRange(Directory.GetFiles(folder1, "*.categories"));
                }
                if (Directory.Exists(folder2))
                {
                    allFiles.AddRange(Directory.GetFiles(folder2, "*.categories"));
                }

                if (allFiles.Count == 0) return files;  // nothing there.
            
                foreach( string file in allFiles )
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);

                    CategorySettingsFile f = new CategorySettingsFile(file);


                    foreach (XmlElement node in doc.SelectNodes("//Category"))
                    {
                        CategorySetting s = new CategorySetting() { Name = node.Attributes["name"].Value, CategoryId = Int32.Parse(node.Attributes["id"].Value), Enabled = (node.Attributes["enabled"].Value == "1") };
                        f.Settings.Add(s);
                    }
                    files.Add(f);
                }
                catch (Exception ex)
                {
                    throw new SystemException("Error loading category settings file: " + file + " Error: " + ex.GetType().Name + ": " + ex.Message, ex);
                }
            }

            return files;
            

        }

        public static CategorySettingsFile GetFileByName(string name)
        {
            var list = GetFiles();

            foreach( var item in list )
            {
                if (item.Name.ToUpper() == name.ToUpper()) return item;
            }

            throw new FileNotFoundException("Unable to find Category Settings File with name:'" + name + "' out of " + list.Count + " considered.");
        }

        public static void GetFolders(out string central, out string user)
        {
            central = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Metamorphosis");
            user = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Metamorphosis");

            try
            {
                if (Directory.Exists(user) == false) Directory.CreateDirectory(user);
                if (Directory.Exists(central) == false) Directory.CreateDirectory(central);
            }
            catch { }  // not sure why we can't, but don't sweat it for now.
        }

        #endregion

        #region PublicMethods
        public override string ToString()
        {
            return Name; 
        }

        public void Save(string filename)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Categories");
            doc.AppendChild(root);

            foreach( var setting in Settings )
            {
                XmlElement entry = doc.CreateElement("Category");
                entry.Attributes.Append(doc.CreateAttribute("name"));
                entry.Attributes.Append(doc.CreateAttribute("id"));
                entry.Attributes.Append(doc.CreateAttribute("enabled"));

                entry.Attributes["name"].Value = setting.Name;
                entry.Attributes["id"].Value = setting.CategoryId.ToString();
                entry.Attributes["enabled"].Value = (setting.Enabled ? "1" : "0");
                root.AppendChild(entry);
            }

            doc.Save(filename);
        }
        #endregion

    
    }

    public class CategorySetting
    {
        public String Name { get; set; }
        public Int32 CategoryId { get; set; }
        public bool Enabled { get; set; }
    }
}
