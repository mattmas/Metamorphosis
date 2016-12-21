using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Metamorphosis.Objects
{
    public class Change
    {
        public enum ChangeTypeEnum { ParameterChange, Move, GeometryChange, NewElement, DeletedElement }
        #region Properties
        public int ElementId { get; set; }

        public string UniqueId { get; set; }

        public string Category { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ChangeTypeEnum ChangeType { get; set; }

        public String Level { get; set; } = String.Empty;
        public string BoundingBoxDescription { get; set; }

        public string ChangeDescription { get; set; } = String.Empty;

        public Boolean IsType { get; set; } = false;

        public string MoveDescription { get; set; }
        #endregion

        #region PublicMethods
        public override string ToString()
        {
            return Category + ": " + ChangeType;
        }
        #endregion
    }
}
