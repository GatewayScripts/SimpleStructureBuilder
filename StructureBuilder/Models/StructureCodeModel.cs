using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureBuilder.Models
{
    public class StructureCodeModel
    {
        public string Code { get; set; }
        public string Meaning { get; set; }
        public string Scheme { get; set; }
        public string Display { get; set; }
        public StructureCodeModel()
        {
            
        }

        internal void UpdateDisplayMemberPath()
        {
            Display = $"{Meaning} ({Code})";
        }
    }
}
