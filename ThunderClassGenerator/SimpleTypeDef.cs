using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderClassGenerator.Utilities;

namespace ThunderClassGenerator
{
    [DebuggerDisplay("{VersionnedName} Component={IsComponent} Generic={IsGeneric}")]
    public class SimpleTypeDef : IChild<SimpleTypeDef>
    {
        public bool IsRelease { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Base { get; set; }
        public SimpleTypeDef BaseType { get; set; }
        public int GenericCount { get; set; }
        public bool IsAbstract { get; set; }
        public short Version { get; set; }
        public int TypeID { get; set; } = -1;
        public bool FlowMapping { get; set; }
        public List<FieldDef> Fields { get; } = new();
        public bool Done { get; set; }
        public virtual string VersionnedName => $"{Name}_V{Version}";
        public bool IsGeneric => GenericCount > 0;
        public bool IsComponent => TypeID != -1;
        public List<List<(string, List<byte>)>> GenericNodesPaths { get; } = new();

        SimpleTypeDef IChild<SimpleTypeDef>.Parent => BaseType;
    }
}