using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    [DebuggerDisplay("{VersionnedName} Generic={IsGeneric}")]
    public class SimpleTypeDef
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Base { get; set; }
        public SimpleTypeDef BaseType { get; set; }
        public int GenericCount { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsStruct { get; set; }
        public short Version { get; set; }
        public int TypeID { get; set; }
        public bool FlowMapping { get; set; }
        public Dictionary<string, FieldDef> Fields { get; } = new();
        public bool Done { get; set; }
        public virtual string VersionnedName => $"{Name}_V{Version}";
        public bool IsGeneric => GenericCount > 0;
        public List<List<(string, List<byte>)>> GenericNodesPaths { get; } = new();
    }
}