using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metamorphosis.Objects
{
    public class ChangeSummary
    {
        #region Properties
        public String ModelName { get; set; }
        public String ModelPath { get; set; }
        public String PreviousFile { get; set; }
        public DateTime ComparisonDate { get; set; }

        public int NumberOfChanges { get; set; }

        public Dictionary<string, int> ModelSummary { get; set; } = new Dictionary<string, int>();

        public IList<string> LevelNames { get; set; } = new List<string>();
        public IList<Change> Changes { get; set; } = new List<Change>();
        #endregion
    }
}
