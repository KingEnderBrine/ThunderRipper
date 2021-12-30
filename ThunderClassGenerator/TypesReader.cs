using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderClassGenerator.Extensions;

namespace ThunderClassGenerator
{
    public class TypesReader
    {
        //Pattern for generic type definition, capturing type name without generic part
        //and first level generics to get parameters count
        //e.g. Test<T1, T2<T4>, T3<T5, T6>> would result in 3 first level generics
        //https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions#balancing-group-definitions
        private static Regex GenericPattern { get; } = new("^(?'name'[^<>]+?)<(?'firstLevel'([^<>]+(((?'Open'<)[^<>]+)+((?'Close-Open'>))+)*(?(Open)(?!))),?)+>$", RegexOptions.Compiled);
        private static string[] ExistingTypesMapping { get; } = new[]
        {
            "bool",
            "char",
            "byte",
            "uint8",
            "sbyte",
            "sint8",
            "short",
            "sint16",
            "ushort",
            "uint16",
            "unsigned short",
            "int",
            "sint32",
            "type*",
            "uint",
            "uint32",
            "unsigned int",
            "float",
            "single",
            "long",
            "long long",
            "sint64",
            "ulong",
            "uint64",
            "unsigned long long",
            "filesize",
            "double",
            "string",
            "pair",
            "map",
            "fixed_bitset",
            "staticvector",
            "vector",
            "set",
            "typelessdata",
            //Never used and are not propper c# types
            "void",
            "type*",

            "array",
        };

        public IEnumerable<SimpleTypeDef> ReadTypes(IEnumerable<UnityClass> classes, bool release = true)
        {
            var types = new Dictionary<(string, short), SimpleTypeDef>();
            var typePermutations = new Dictionary<(string, short), TypePermutations>();
            foreach (var @class in classes)
            {
                GatherPermutations(@class, release, typePermutations);
            }

            FixPermutations(typePermutations);

            foreach (var row in typePermutations)
            {
                var permutations = row.Value;
                var typeDef = types[row.Key] = new SimpleTypeDef
                {
                    Base = permutations.Class?.Base ?? string.Empty,
                    IsAbstract = permutations.Class?.IsAbstract ?? false,
                    IsStruct = permutations.Class == null,
                    Namespace = permutations.Class?.Namespace ?? string.Empty,
                    TypeID = permutations.Class?.TypeID ?? -1,
                    Version = row.Key.Item2,
                    Name = row.Key.Item1,
                };
                /*foreach (var fieldRow in CalculateGenericFields(permutations.Nodes))
                {
                    typeDef.GenericFields[fieldRow.Key] = fieldRow.Value;
                }
                typeDef.GenericCount = typeDef.GenericFields.Any() ? typeDef.GenericFields.Max(el => el.Value) : permutations.GenericCount;
                */
                foreach (var node in row.Value.Nodes)
                {
                    node.AssosiatedTypeDef = typeDef;
                }
            }

            foreach (var type in types.Values)
            {
                ProcessTypeFields(type, types, typePermutations);
            }

            /*foreach (var row in types)
            {
                var permutations = typePermutations[row.Key];
                var firstNode = permutations.Nodes.FirstOrDefault();
                if (firstNode == null)
                {
                    continue;
                }
                var type = row.Value;
                if (!string.IsNullOrWhiteSpace(type.Base))
                {
                    type.BaseType = types.FirstOrDefault(el => el.Key.Item1 == type.Base).Value;
                }
                foreach (var fieldNode in firstNode.SubNodes)
                {
                    var fieldDef = GetFieldDefForNode(fieldNode, types);

                    type.Fields.Add(fieldDef.Name, fieldDef);
                }
            }*/

            return types.Values;
        }

        private void ProcessTypeFields(SimpleTypeDef type, Dictionary<(string, short), SimpleTypeDef> types, Dictionary<(string, short), TypePermutations> typePermutations)
        {
            if (type.Done)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(type.Base))
            {
                type.BaseType = types.FirstOrDefault(el => el.Key.Item1 == type.Base).Value;
            }

            var permutations = typePermutations[(type.Name, type.Version)];
            var firstNode = permutations.Nodes.FirstOrDefault();
            if (firstNode == null)
            {
                type.Done = true;
                return;
            }

            foreach (var node in permutations.Nodes)
            {
                //if (genericFields.Count == startFieldTypes.Count)
                //{
                //    break;
                //}
                foreach (var field in node.SubNodes)
                {
                    ProcessTypeFields(field.AssosiatedTypeDef, types, typePermutations);

                }
            }

            foreach (var fieldNode in firstNode.SubNodes)
            {
                var fieldDef = GetFieldDefForNode(fieldNode, types);

                type.Fields.Add(fieldDef.Name, fieldDef);
            }

            type.Done = true;
        }

        /// <summary>
        /// Special treatment for VR settings because Unity is stupid
        /// Some types have different set of fields even though they have the same name
        /// Split these to separate permutations
        /// </summary>
        /// <param name="typePermutations"></param>
        private void FixPermutations(Dictionary<(string, short), TypePermutations> typePermutations)
        {
            var comparer = new ListComparer<UnityNode> { ItemHashFunc = el => HashCode.Combine(el.Name) };
            var removePermutations = new List<(string, short)>();
            var addPermutations = new List<(string, short, TypePermutations)>();
            foreach (var row in typePermutations)
            {
                var permutation = row.Value;
                if (permutation.Class != null || permutation.Nodes.Count < 2)
                {
                    continue;
                }
                var uniqueNodes = new Dictionary<int, List<UnityNode>>();
                foreach (var node in permutation.Nodes)
                {
                    var nodes = uniqueNodes.GetOrAdd(comparer.GetHashCode(node.SubNodes));
                    nodes.Add(node);
                }
                if (uniqueNodes.Count == 1)
                {
                    continue;
                }
                removePermutations.Add(row.Key);
                foreach (var nodes in uniqueNodes.Values)
                {
                    var permutations = new TypePermutations();
                    permutations.Nodes.AddRange(nodes);
                    var firstNode = nodes[0];
                    var typeName = $"{GetParentName(firstNode)}_{firstNode.TypeName}_{firstNode.Name}";
                    foreach (var node in nodes)
                    {
                        node.TypeName = typeName;
                    }
                    addPermutations.Add((typeName, firstNode.Version, permutations));

                    static string GetParentName(UnityNode node) => ExistingTypesMapping.Contains(node.Parent.TypeName.ToLowerInvariant()) ? GetParentName(node.Parent) : node.Parent.TypeName;
                }
            }
            foreach (var row in removePermutations)
            {
                typePermutations.Remove(row);
            }
            foreach (var row in addPermutations)
            {
                typePermutations[(row.Item1, row.Item2)] = row.Item3;
            }
        }

        private static Dictionary<string, int> CalculateGenericFields(List<UnityNode> nodes)
        {
            if (nodes.Count == 0)
            {
                return new Dictionary<string, int>();
            }
            var genericFields = new HashSet<string>();

            var firstNode = nodes.First();
            var startFieldTypes = firstNode.SubNodes.ToDictionary(el => el.Name, el => el.TypeName);
            foreach (var node in nodes)
            {
                if (genericFields.Count == startFieldTypes.Count)
                {
                    break;
                }
                foreach (var field in node.SubNodes)
                {
                    if (startFieldTypes[field.Name] != field.TypeName)
                    {
                        genericFields.Add(field.Name);
                    }
                }
            }

            var groups = new HashSet<List<string>>(new ListComparer<string>());
            foreach (var node in nodes)
            {
                var tmpGroups = new Dictionary<string, List<string>>();
                foreach (var field in node.SubNodes)
                {
                    if (!genericFields.Contains(field.Name))
                    {
                        continue;
                    }
                    var group = tmpGroups.GetOrAdd(field.TypeName);
                    group.Add(field.Name);
                }
                foreach (var group in tmpGroups.Values)
                {
                    groups.Add(group);
                }
            }

            var finalGroups = new List<List<string>>();
            var removeFinalGroups = new List<List<string>>();
            var addFinalGroups = new List<List<string>>();
            foreach (var group in groups)
            {
                var reducedGroup = group;
                foreach (var finalGroup in finalGroups)
                {
                    var finalReducedGroup = finalGroup.Except(reducedGroup).ToList();
                    if (finalReducedGroup.Count != 0)
                    {
                        var finalOldGroup = finalGroup.Except(finalReducedGroup).ToList();
                        if (finalOldGroup.Count != 0)
                        {
                            addFinalGroups.Add(finalReducedGroup);
                            addFinalGroups.Add(finalOldGroup);
                            removeFinalGroups.Add(finalGroup);
                        }
                    }
                    reducedGroup = reducedGroup.Except(finalGroup).ToList();
                    if (reducedGroup.Count == 0)
                    {
                        break;
                    }
                }
                foreach (var finalGroup in removeFinalGroups)
                {
                    finalGroups.Remove(finalGroup);
                }
                finalGroups.AddRange(addFinalGroups);

                if (reducedGroup.Count != 0)
                {
                    finalGroups.Add(reducedGroup);
                }

                removeFinalGroups.Clear();
                addFinalGroups.Clear();
            }

            return finalGroups.OrderBy(el => el.Min(e => firstNode.SubNodes.FindIndex(ele => ele.Name == e))).SelectMany((el, index) => el.Select(field => (field, index))).ToDictionary(el => el.field, el => el.index);
        }

        private void GatherPermutations(UnityClass @class, bool release, Dictionary<(string, short), TypePermutations> typePermutations)
        {
            var node = release ? @class.ReleaseRootNode : @class.EditorRootNode;
            var permutations = typePermutations.GetOrAdd((@class.Name, node?.Version ?? 1));
            permutations.Class = @class;
            if (node == null)
            {
                return;
            }
            permutations.Nodes.Add(node);
            foreach (var cNode in node.SubNodes)
            {
                GatherPermutations(cNode, typePermutations);
            }
        }

        private void GatherPermutations(UnityNode node, Dictionary<(string, short), TypePermutations> typePermutations)
        {
            var typeLower = node.TypeName.ToLowerInvariant();
            if (ExistingTypesMapping.Contains(typeLower))
            {
                switch (typeLower)
                {
                    case "pair":
                        GatherPermutations(node.SubNodes[0], typePermutations);
                        GatherPermutations(node.SubNodes[1], typePermutations);
                        break;
                    case "map":
                        var pairNode = node.SubNodes[0].SubNodes[1];
                        GatherPermutations(pairNode.SubNodes[0], typePermutations);
                        GatherPermutations(pairNode.SubNodes[1], typePermutations);
                        break;
                    case "fixed_bitset":
                    case "staticvector":
                    case "vector":
                    case "set":
                        GatherPermutations(node.SubNodes[0].SubNodes[1], typePermutations);
                        break;
                    case "typelessdata":
                        GatherPermutations(node.SubNodes[1], typePermutations);
                        break;
                }
                return;
            }
            _ = IsGenericType(node.TypeName, out var name, out var genericCount, out _);

            var permutations = typePermutations.GetOrAdd((name, node.Version));
            permutations.Nodes.Add(node);
            permutations.GenericCount = genericCount;
            foreach (var cNode in node.SubNodes)
            {
                GatherPermutations(cNode, typePermutations);
            }
        }

        private static bool IsGenericType(string typeName, out string nameWithoutGeneric, out int genericCount, out string[] genericArgs)
        {
            var match = GenericPattern.Match(typeName);
            if (!match.Success)
            {
                nameWithoutGeneric = typeName;
                genericCount = 0;
                genericArgs = Array.Empty<string>();
                return false;
            }

            nameWithoutGeneric = match.Groups["name"].Value;
            genericCount = match.Groups["firstLevel"].Captures.Count;
            genericArgs = match.Groups["firstLevel"].Captures.Select(el => el.Value).ToArray();
            return true;
        }

        public static FieldDef GetFieldDefForNode(UnityNode node, Dictionary<(string, short), SimpleTypeDef> otherTypeDefs)
        {
            var fieldDef = new FieldDef
            {
                Name = node.Name,
                MetaFlags = node.MetaFlag,
                Type = GetTypeUsageDefForNode(node, otherTypeDefs),
            };

            return fieldDef;
        }

        private static TypeUsageDef GetTypeUsageDefForNode(UnityNode node, Dictionary<(string, short), SimpleTypeDef> otherTypeDefs)
        {
            var typeLower = node.TypeName.ToLower();
            var typeDef = typeLower switch
            {
                "bool" => PredefinedTypeDef.Bool,
                "char" => PredefinedTypeDef.Char,
                "byte" => PredefinedTypeDef.Byte,
                "uint8" => PredefinedTypeDef.Byte,
                "sbyte" => PredefinedTypeDef.SByte,
                "sint8" => PredefinedTypeDef.SByte,
                "short" => PredefinedTypeDef.Short,
                "sint16" => PredefinedTypeDef.Short,
                "ushort" => PredefinedTypeDef.UShort,
                "uint16" => PredefinedTypeDef.UShort,
                "unsigned short" => PredefinedTypeDef.UShort,
                "int" => PredefinedTypeDef.Int,
                "sint32" => PredefinedTypeDef.Int,
                "type*" => PredefinedTypeDef.Int,
                "uint" => PredefinedTypeDef.UInt,
                "uint32" => PredefinedTypeDef.UInt,
                "unsigned int" => PredefinedTypeDef.UInt,
                "float" => PredefinedTypeDef.Float,
                "single" => PredefinedTypeDef.Float,
                "long" => PredefinedTypeDef.Long,
                "long long" => PredefinedTypeDef.Long,
                "sint64" => PredefinedTypeDef.Long,
                "ulong" => PredefinedTypeDef.ULong,
                "uint64" => PredefinedTypeDef.ULong,
                "unsigned long long" => PredefinedTypeDef.ULong,
                "filesize" => PredefinedTypeDef.ULong,
                "double" => PredefinedTypeDef.Double,
                "string" => PredefinedTypeDef.String,
                "pair" => PredefinedTypeDef.KeyValuePair,
                "map" => PredefinedTypeDef.Dictionary,
                "fixed_bitset" => PredefinedTypeDef.List,
                "staticvector" => PredefinedTypeDef.List,
                "vector" => PredefinedTypeDef.List,
                "set" => PredefinedTypeDef.HashSet,
                "typelessdata" => PredefinedTypeDef.List,
                //"array" => PredefinedTypeDef.List,
                _ => otherTypeDefs[(node.AssosiatedTypeDef.Name, node.Version)]
            };

            var genericDef = new TypeUsageDef
            {
                Type = typeDef,
            };

            switch (typeLower)
            {
                case "pair":
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(node.SubNodes[0], otherTypeDefs));
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(node.SubNodes[1], otherTypeDefs));
                    break;
                case "map":
                    var pairNode = node.SubNodes[0].SubNodes[1];
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(pairNode.SubNodes[0], otherTypeDefs));
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(pairNode.SubNodes[1], otherTypeDefs));
                    break;
                case "fixed_bitset":
                case "staticvector":
                case "vector":
                case "set":
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(node.SubNodes[0].SubNodes[1], otherTypeDefs));
                    break;
                //case "array":
                case "typelessdata":
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(node.SubNodes[1], otherTypeDefs));
                    break;
                default:
                    if (IsGenericType(node.TypeName, out _, out _, out var args))
                    {
                        genericDef.GenericArgs.AddRange(args.Select(el => new TypeUsageDef { Type = otherTypeDefs.FirstOrDefault(e => e.Key.Item1 == el).Value }));
                    }
                    break;
            }

            return genericDef;
        }


        private class TypePermutations
        {
            public UnityClass Class { get; set; }
            public int GenericCount { get; set; }
            public List<UnityNode> Nodes { get; } = new List<UnityNode>();
        }

        private class ListComparer<T> : IEqualityComparer<List<T>>
        {
            public IEqualityComparer<T> ItemComparer { get; set; }
            public Func<T, int> ItemHashFunc { get; set; }

            public bool Equals(List<T> x, List<T> y)
            {
                return x.Count == y.Count && !x.Where(el => !y.Contains(el, ItemComparer)).Any();
            }

            public int GetHashCode([DisallowNull] List<T> obj)
            {
                return obj.Select(el => ItemHashFunc == null ? el.GetHashCode() : ItemHashFunc(el)).Aggregate((total, el) => total ^ el);
            }
        }

        private class UnityNodeNameComparer : IEqualityComparer<UnityNode>
        {
            public bool Equals(UnityNode x, UnityNode y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode([DisallowNull] UnityNode obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
