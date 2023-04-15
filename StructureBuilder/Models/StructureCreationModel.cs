
namespace StructureBuilder.Models
{
    public class StructureCreationModel
    {
        public int StructureStepId { get; set; }
        public string ResultStructure { get; set; }
        public string StructureOperation { get; set; }
        public string BaseStructure { get; set; }
        public string TargetStructure { get; set; }
        public bool bTemp { get; set; }
        public double Margin { get; set; }
        public AsymmetricMarginModel AsymmetricMargin { get; set; }
        public string ResultStructureColor { get; set; }
        public StructureCodeModel ResultStructureCode { get; set; }
        //List<string> AdditionalStructures { get; set; }
    }
}
