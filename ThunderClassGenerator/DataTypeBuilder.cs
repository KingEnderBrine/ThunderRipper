using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class DataTypeBuilder
    {
        public static string BaseNamespace = "ThunderRipper.Unity";

        private Dictionary<string, FieldInfo> Fields { get; } = new();
        public string Namespace { get; private set; }
        public string Class { get; private set; }
        public string Base { get; private set; }
        public bool IsStruct { get; private set; }

        public DataTypeBuilder(string @namespace, string @class, string @base, bool isStruct = false)
        {
            Namespace = @namespace;
            Class = @class;
            Base = @base;
            IsStruct = isStruct;
        }

        public FieldInfo AddOrGetProperty(string name, out bool isNew)
        {
            if (Fields.TryGetValue(name, out var fieldInfo))
            {
                isNew = false;
                return fieldInfo;
            }

            isNew = true;
            return Fields[name] = new FieldInfo(name);
        }

        public override string ToString()
        {
            return 
$@"using System.Collections.Generic;
using ThunderRipper.Attributes;

namespace {BaseNamespace}{(string.IsNullOrWhiteSpace(Namespace) ? "" : $".{Namespace}")}
{{
    public partial {(IsStruct ? "struct" : "class")} {Class}{(string.IsNullOrWhiteSpace(Base) ? "" : $" : {Base}")}
    {{
{string.Join(Environment.NewLine, Fields.Values.Select(el => el.ToString()))}
    }}
}}";
        }

        public class FieldInfo
        {
            private List<AttributeInfo> Attributes { get; } = new();
            public string Name { get; private set; }
            public string Type { get; set; }

            public FieldInfo(string name)
            {
                Name = name;
            }

            public void AddAttribute(string attribute, params string[] values)
            {
                Attributes.Add(new AttributeInfo(attribute, values));
            }

            public override string ToString()
            {
                return $"{(Attributes.Count == 0 ? "" : $"        {string.Join($"{Environment.NewLine}        ", Attributes.Select(el => el.ToString()))}{Environment.NewLine}")}        public {Type} {Name};";
            }
        }

        public class AttributeInfo
        {
            public string Name { get; private set; }
            private List<string> Values { get; } = new();

            public AttributeInfo(string name, IEnumerable<string> values)
            {
                Name = name;
                Values.AddRange(values);
            }

            public override string ToString()
            {
                return $"[{Name}{(Values.Count == 0 ? "" : $"({string.Join(", ", Values)})")}]";
            }
        }
    }
}
