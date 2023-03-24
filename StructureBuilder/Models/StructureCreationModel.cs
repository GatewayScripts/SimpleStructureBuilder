using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureBuilder.Models
{
    public class StructureCreationModel:BindableBase
    {

        public string ResultStructure { get; set; }
        public string StructureOperation { get; set; }
        public string BaseStructure { get; set; }
        public string TargetStructure { get; set; }
        public bool bTemp { get; set; }
        public double Margin { get; set; }
        public AsymmetricMarginModel AsymmetricMargin { get; set; }
        //List<string> AdditionalStructures { get; set; }
    }
}
