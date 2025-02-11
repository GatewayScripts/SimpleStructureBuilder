using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace StructureBuilder.Models
{
    public class StructureModel
    {
        public string StructureId { get; set; }
        public bool bHiRes { get; set; }
        /// <summary>
        /// Constructor getting properties from eSAPI structure
        /// </summary>
        /// <param name="structure">ESAPI structure.</param>
        public StructureModel(Structure structure)
        {
            StructureId = structure.Id;
            bHiRes = structure.IsHighResolution;
        }
        /// <summary>
        /// parameterless constructor to add properties
        /// </summary>
        public StructureModel()
        {
            
        }
    }
}
