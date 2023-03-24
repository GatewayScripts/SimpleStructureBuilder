using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureBuilder.Models
{
    public class AsymmetricMarginModel
    {
        public double Ant { get; set; }
        public double Post { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Sup { get; set; }
        public double Inf { get; set; }
        public string MarginDirection { get; set; }
        public AsymmetricMarginModel(string marginDirection, double ant, double post, double left, double right, double sup, double inf)
        {
            MarginDirection = marginDirection;
            Ant = ant;
            Post = post;
            Left = left;
            Right = right;
            Sup = sup;
            Inf = inf;
        }
    }
}
