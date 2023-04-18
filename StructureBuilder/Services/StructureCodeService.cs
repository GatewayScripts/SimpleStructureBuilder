using Newtonsoft.Json;
using StructureBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StructureBuilder.Services
{
    public static class StructureCodeService
    {
        public static List<StructureCodeModel> StructureCodes = new List<StructureCodeModel>();
        public static void GetStructureCodes()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "structure_dict.json");
            List<StructureCodeModel> localStructureCodes = JsonConvert.DeserializeObject<List<StructureCodeModel>>(File.ReadAllText(filePath));
            foreach(var code in localStructureCodes.OrderBy(sc=>sc.Meaning))
            {
                StructureCodes.Add(code);
            }
        }
        
    }
}
