using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class SimpleTypeDef
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Base { get; set; }
        public SimpleTypeDef BaseType { get; set; }
        //public List<SimpleTypeDef> BaseGenericArgs { get; } = new();
        public int GenericCount { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsStruct { get; set; }
        public short Version { get; set; }
        public int TypeID { get; set; }
        public Dictionary<string, FieldDef> Fields { get; } = new();
        public Dictionary<string, int> GenericFields { get; } = new();

        public virtual string VersionnedName => $"{Name}_V{Version}";
    }
}