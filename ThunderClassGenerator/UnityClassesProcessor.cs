using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class UnityClassesProcessor
    {
        public static IEnumerable<SimpleTypeDef> GenerateTypeDefs(IEnumerable<UnityClass> unityClasses)
        {
            return new InternalProcessor(unityClasses).Run();
        }

        private class InternalProcessor
        {
            //Pattern for generic type definition, capturing type name without generic part
            //and first level generics to get parameters count
            //e.g. Test<T1, T2<T4>, T3<T5, T6>> would result in 3 first level generics
            //https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions#balancing-group-definitions
            private static Regex GenericPattern { get; } = new("^(?'name'[^<>]+?)<(?'firstLevel'([^<>]+(((?'Open'<)[^<>]+)+((?'Close-Open'>))+)*(?(Open)(?!))),?)+>$", RegexOptions.Compiled);
            private static string[] ExistingTypes { get; } = new[]
            {
                "string",
                "int",
                "byte",
                "long",
                "char",
                "float",
                "bool",
                "double",
                "sbyte",
                "short",
                "ushort",
                "uint",
                "ulong",
                "sint8",
                "uint8",
                "sint16",
                "uint16",
                "unsigned short",
                "sint32",
                "uint32",
                "unsigned int",
                "sint64",
                "uint64",
                "unsigned long",
                "filesize",
                "single",
                "pair",
                "map",
                "vector",
                "set",
                "typelessdata",
                "fixed_bitset",
                //Never used and are not a propper c# type
                "void",
                "type*",
            };

            private readonly Dictionary<string, SimpleTypeDef> typeDefs = new();
            private readonly Dictionary<string, UnityNode> uniqueTypeNodes = new();
            private readonly IEnumerable<UnityClass> classes;

            public InternalProcessor(IEnumerable<UnityClass> classes)
            {
                this.classes = classes;
            }

            public IEnumerable<SimpleTypeDef> Run()
            {
                foreach (var @class in classes)
                {
                    GetOrCreateClass(@class);
                }
                while (uniqueTypeNodes.Count > 0)
                {
                    GetOrCreateType(uniqueTypeNodes.First().Value);
                }

                return typeDefs.Values.ToList();
            }

            private SimpleTypeDef GetOrCreateType(UnityNode typeNode)
            {
                _ = IsGenericType(typeNode.TypeName, out var name, out var genericCount);
                if (typeDefs.TryGetValue(typeNode.TypeName, out var typeDef))
                {
                    return typeDef;
                }
                uniqueTypeNodes.Remove(name);

                typeDef = typeDefs[name] = new SimpleTypeDef
                {
                    Name = name,
                    //Currently only relevant to "PPtr" struct
                    GenericCount = genericCount,
                    Namespace = null,
                    TypeID = -1,
                    IsAbstract = false,
                    Version = typeNode.Version,
                    Base = null,
                    IsStruct = true,
                    BaseType = null,
                };

                UpdateFields(typeDef, typeNode, true, true);

                return typeDef;
            }

            private SimpleTypeDef GetOrCreateClass(UnityClass @class)
            {
                if (ExistingTypes.Contains(@class.Name))
                {
                    return null;
                }
                if (typeDefs.TryGetValue(@class.Name, out var typeDef))
                {
                    return typeDef;
                }
                uniqueTypeNodes.Remove(@class.Name);

                typeDef = typeDefs[@class.Name] = new SimpleTypeDef
                {
                    Name = @class.Name,
                    //Unity classes don't support generics
                    GenericCount = 0,
                    Namespace = @class.Namespace,
                    TypeID = @class.TypeID,
                    IsAbstract = @class.IsAbstract,
                    Version = @class.ReleaseRootNode?.Version ?? 1,
                    Base = @class.Base,
                    IsStruct = false,
                };

                var parentClass = string.IsNullOrWhiteSpace(@class.Base) ? null : classes.FirstOrDefault(el => el.Name == @class.Base);
                typeDef.BaseType = parentClass == null ? null : GetOrCreateClass(parentClass);

                UpdateFields(typeDef, @class.EditorRootNode, true, false);
                UpdateFields(typeDef, @class.ReleaseRootNode, false, true);

                return typeDef;
            }

            private void UpdateFields(SimpleTypeDef typeDef, UnityNode baseNode, bool isEditor, bool isRelease)
            {
                if (baseNode == null || (baseNode.SubNodes?.Count ?? 0) == 0)
                {
                    return;
                }
                foreach (var fieldNode in baseNode.SubNodes)
                {
                    if (!typeDef.Fields.TryGetValue(fieldNode.Name, out var fieldDef))
                    {
                        if (ParentHasFieldReqursive(fieldNode, typeDef.BaseType))
                        {
                            return;
                        }

                        var collectionItemTypeNodes = new Dictionary<string, UnityNode>();
                        fieldDef = typeDef.Fields[fieldNode.Name] = new FieldDef
                        {
                            Name = fieldNode.Name,
                            //Type = NormalizeNodeTypeName(fieldNode, collectionItemTypeNodes, out var isExistingType),
                        };
                        //UpdateUniqueTypeNodes(fieldNode, collectionItemTypeNodes, isExistingType);
                    }
#warning TODO: check if a field can have different types in editor vs release
                    //fieldDef.InEditor |= isEditor;
                    //fieldDef.InRelease |= isRelease;
                }
            }

#warning TODO: There are types like PlayerSettings.VRSettings.Google. Need to do something about that
            private void UpdateUniqueTypeNodes(UnityNode fieldNode, Dictionary<string, UnityNode> collectionItemTypeNodes, bool existingType)
            {
                foreach (var row in collectionItemTypeNodes)
                {
                    AddNode(row.Value);
                }

                if (!existingType)
                {
                    AddNode(fieldNode);
                }

                void AddNode(UnityNode node)
                {
                    _ = IsGenericType(node.TypeName, out var nameWithoutGeneric, out _);

                    if (!typeDefs.ContainsKey(nameWithoutGeneric) && !uniqueTypeNodes.ContainsKey(nameWithoutGeneric))
                    {
                        uniqueTypeNodes.Add(nameWithoutGeneric, node);
                    }
                }
            }

            private bool ParentHasFieldReqursive(UnityNode fieldNode, SimpleTypeDef parentType)
            {
                if (parentType == null)
                {
                    return false;
                }

                if (parentType.Fields.TryGetValue(fieldNode.Name, out _))
                {
                    return true;
                }

                return ParentHasFieldReqursive(fieldNode, parentType.BaseType);
            }

            private bool IsGenericType(string typeName, out string nameWithoutGeneric, out int genericCount)
            {
                var match = GenericPattern.Match(typeName);
                if (!match.Success)
                {
                    nameWithoutGeneric = typeName;
                    genericCount = 0;
                    return false;
                }

                nameWithoutGeneric = match.Groups["name"].Value;
                genericCount = match.Groups["firstLevel"].Captures.Count;
                return true;
            }

            private string NormalizeNodeTypeName(UnityNode node, Dictionary<string, UnityNode> collectionItemTypeNodes, out bool isExistingType)
            {
                isExistingType = true;
                var typeLower = node.TypeName.ToLower();
                switch (typeLower)
                {
                    case "string":
                    case "int":
                    case "byte":
                    case "long":
                    case "char":
                    case "float":
                    case "bool":
                    case "double":
                    case "sbyte":
                    case "short":
                    case "ushort":
                    case "uint":
                    case "ulong":
                        return typeLower;
                    case "sint8":
                        return "sbyte";
                    case "uint8":
                        return "byte";
                    case "sint16":
                        return "short";
                    case "uint16":
                    case "unsigned short":
                        return "ushort";
                    case "sint32":
                        return "int";
                    case "type*":
                        return "int";
                    case "uint32":
                    case "unsigned int":
                        return "uint";
                    case "sint64":
                        return "long";
                    case "uint64":
                    case "unsigned long":
                    case "filesize":
                        return "ulong";
                    case "single":
                        return "float";
                    //Complex types
                    case "pair":
                        return $"KeyValuePair<{GetCollectionItemType(node.SubNodes[0])}, {GetCollectionItemType(node.SubNodes[1])}>";
                    case "map":
                        var pairNode = node.SubNodes[0].SubNodes[1];
                        return $"Dictionary<{GetCollectionItemType(pairNode.SubNodes[0])}, {GetCollectionItemType(pairNode.SubNodes[1])}>";
                    case "fixed_bitset":
                    case "vector":
                        //More suitable type is array but lists are easier for reqursive name generation
                        //with 2-dimensional array you would get something like int[]int[] which is not valid
                        return $"List<{GetCollectionItemType(node.SubNodes[0].SubNodes[1])}>";
                    case "set":
                        return $"HashSet<{GetCollectionItemType(node.SubNodes[0].SubNodes[1])}>";
                    //TODO: maybe something more special???
                    case "typelessdata":
                        return $"List<{GetCollectionItemType(node.SubNodes[1])}>";
                }

                isExistingType = false;
                return node.TypeName;

                string GetCollectionItemType(UnityNode node)
                {
                    var resultType = NormalizeNodeTypeName(node, collectionItemTypeNodes, out var tmpIsExistingType);
                    if (!tmpIsExistingType)
                    {
                        collectionItemTypeNodes[resultType] = node;
                    }
                    return resultType;
                }
            }
        }
    }
}
