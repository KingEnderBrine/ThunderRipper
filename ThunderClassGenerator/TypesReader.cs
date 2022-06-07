using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderClassGenerator.Extensions;
using ThunderClassGenerator.Utilities;

namespace ThunderClassGenerator
{
    public class TypesReader
    {
        //Pattern for generic type definition, capturing type name without generic part
        //and first level generics to get parameters count
        //e.g. Test<T1, T2<T4>, T3<T5, T6>> would result in 3 first level generics
        //https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions#balancing-group-definitions
        private static Regex GenericPattern { get; } = new("^(?'name'[^<>]+?)<(?'firstLevel'([^<>]+(((?'Open'<)[^<>]+)+((?'Close-Open'>))+)*(?(Open)(?!))),?)+>$", RegexOptions.Compiled);
        private static Regex ArrayAccessPattern { get; } = new("^(?'name'.+?)\\[(?'index'\\d+?)\\]$", RegexOptions.Compiled);
        private static string[] ExistingTypes { get; } = new[]
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
            "array",

            //Never used and are not propper c# types
            "void",
            "type*",
        };

        public IEnumerable<SimpleTypeDef> ReadTypes(IEnumerable<UnityClass> classes, bool release = true)
        {
            FixClasses(classes, release);
            FixNodes(classes, release);
            AssignNodeParents(classes);

            var typePermutations = GatherPermutations(classes, release);
            FixPermutations(typePermutations);

            var types = CreateTypesFromPermutations(typePermutations);
            AssignBaseTypes(types);

            var inverseOrderTypes = GetTypesToProcess(types, classes, release);

            foreach (var type in inverseOrderTypes)
            {
                ProcessTypeFields(type, types, typePermutations);
            }

            return types.Values;
        }

        private static HashSet<SimpleTypeDef> GetTypesToProcess(Dictionary<(string, short, bool), SimpleTypeDef> types, IEnumerable<UnityClass> classes, bool release)
        {
            var inverseOrderTypes = new HashSet<SimpleTypeDef>();
            foreach (var @class in classes.Where(IsNotExistingType).SelectMany(c => Recursion.Downwards(c, (cc) => FindBase(classes, cc))))
            {
                var type = types.Values.FirstOrDefault(el => el.Name == @class.Name);
                if (inverseOrderTypes.Contains(type))
                {
                    continue;
                }

                var node = release ? @class.ReleaseRootNode : @class.EditorRootNode;
                if (node != null)
                {
                    foreach (var cNode in node.RecursionDepthFirst())
                    {
                        if (cNode.AssosiatedTypeDef is not PredefinedTypeDef)
                        {
                            inverseOrderTypes.Add(cNode.AssosiatedTypeDef);
                        }
                    }
                }
                inverseOrderTypes.Add(type);
            }

            return inverseOrderTypes;
        }

        private static void AssignBaseTypes(Dictionary<(string, short, bool), SimpleTypeDef> types)
        {
            foreach (var type in types.Values)
            {
                if (!string.IsNullOrWhiteSpace(type.Base))
                {
                    type.BaseType = types.FirstOrDefault(el => el.Key.Item1 == type.Base).Value;
                }
            }
        }

        private static Dictionary<(string, short, bool), SimpleTypeDef> CreateTypesFromPermutations(Dictionary<(string, short, bool), TypePermutations> typePermutations)
        {
            var types = new Dictionary<(string, short, bool), SimpleTypeDef>();
            
            foreach (var row in typePermutations)
            {
                var permutations = row.Value;
                var firstNode = permutations.Nodes.FirstOrDefault();
                var typeDef = types[row.Key] = new SimpleTypeDef
                {
                    Base = permutations.Class?.Base ?? string.Empty,
                    IsAbstract = permutations.Class?.IsAbstract ?? false,
                    Namespace = permutations.Class?.Namespace ?? string.Empty,
                    TypeID = permutations.Class?.TypeID ?? -1,
                    Version = row.Key.Item2,
                    Name = row.Key.Item1,
                    Done = permutations.Nodes.Count == 0,
                };
                if (firstNode != null)
                {
                    if (IsGenericTypeName(firstNode.TypeName, out _, out var count, out _))
                    {
                        typeDef.GenericCount = count;
                    }
                    typeDef.FlowMapping = (firstNode.MetaFlag & (int)MetaFlag.TransferUsingFlowMappingStyle) != 0;
                }
                foreach (var node in permutations.Nodes)
                {
                    node.AssosiatedTypeDef = typeDef;
                }
            }

            return types;
        }

        private static UnityClass FindBase(IEnumerable<UnityClass> classes, UnityClass @class)
        {
            if (string.IsNullOrWhiteSpace(@class.Base))
            {
                return null;
            }

            return classes.FirstOrDefault(el => el.Name == @class.Base);
        }

        private static bool IsNotExistingType(UnityClass @class) => !ExistingTypes.Contains(@class.Name, StringComparer.InvariantCultureIgnoreCase);

        private static void AssignNodeParents(IEnumerable<UnityClass> classes)
        {
            foreach (var @class in classes)
            {
                if (@class.ReleaseRootNode != null)
                {
                    SetChildrenParent(@class.ReleaseRootNode);
                }
                if (@class.EditorRootNode != null)
                {
                    SetChildrenParent(@class.EditorRootNode);
                }
            }

            static void SetChildrenParent(UnityNode node)
            {
                foreach (var cNode in node.SubNodes)
                {
                    cNode.Parent = node;
                    SetChildrenParent(cNode);
                }
            }
        }

        private void FixNodes(IEnumerable<UnityClass> classes, bool release)
        {
            foreach (var @class in classes)
            {
                var node = release ? @class.ReleaseRootNode : @class.EditorRootNode;
                if (node == null)
                {
                    continue;
                }
                FixNode(node);
            }
        }

        private void FixNode(UnityNode node)
        {
            if (node.TypeName == "string")
            {
                //Up to Unity 2018.4.24f1 in release PlayableDirector.ExposedReferenceTable.m_References has special string that is actually int
                if (node.SubNodes.FirstOrDefault()?.TypeName == "int")
                {
                    node.TypeName = "int";
                    node.MetaFlag = node.SubNodes.FirstOrDefault().MetaFlag;
                    node.SubNodes.Clear();
                    return;
                }
                //Starting from Unity 2018.4.25f1 in release PlayableDirector.ExposedReferenceTable.m_References has special string that is a string^2
                if (node.SubNodes.FirstOrDefault()?.TypeName == "string")
                {
                    node.MetaFlag = node.SubNodes.FirstOrDefault().MetaFlag;
                    node.SubNodes = node.SubNodes.FirstOrDefault().SubNodes;
                    return;
                }
            }

            foreach (var cNode in node.SubNodes)
            {
                FixNode(cNode);
            }
        }

        private void FixClasses(IEnumerable<UnityClass> classes, bool release)
        {
            var comparer = new ListComparer<UnityNode> { ItemHashFunc = el => HashCode.Combine(el.Name) };
            foreach (var @class in classes)
            {
                var node = release ? @class.ReleaseRootNode : @class.EditorRootNode;
                if (node == null || @class.Name == node.TypeName)
                {
                    continue;
                }

                //Unity 3.5.0f5 - class "EditorUserSettings" nodes have misspelled type name "EditorUSerSettings"
                if (node.TypeName.ToLowerInvariant() == @class.Name.ToLowerInvariant())
                {
                    node.TypeName = @class.Name;
                    continue;
                }

                var correctClass = classes.FirstOrDefault(el => el.Name == node.TypeName);
                if (release && correctClass.ReleaseRootNode == null)
                {
                    correctClass.ReleaseRootNode = node;
                    @class.ReleaseRootNode = null;
                }
                else if (!release && correctClass.EditorRootNode == null)
                {
                    correctClass.EditorRootNode = node;
                    @class.EditorRootNode = null;
                }
                //Unity 2020.2.0a10 - "ScriptableCamera" root node has type "Camera" even though it has extra fields compared to base type "Camera"
                else if (node.TypeName == @class.Base)
                {
                    if (release && @class.ReleaseRootNode != null && !comparer.Equals(@class.ReleaseRootNode.SubNodes, correctClass.ReleaseRootNode.SubNodes))
                    {
                        node.TypeName = @class.Name;
                    }
                    else if (!release && @class.EditorRootNode != null && !comparer.Equals(@class.EditorRootNode.SubNodes, correctClass.EditorRootNode.SubNodes))
                    {
                        node.TypeName = @class.Name;
                    }
                }
            }

            // Some inherited classes have the same fields, but base type node is empty
            // Populate base type node with common fields
            foreach (var @class in classes.Where(el => release ? el.ReleaseRootNode == null : el.EditorRootNode == null))
            {
                var derivedClasses = classes.Where(el => @class.Derived.Contains(el.Name) && (release ? el.ReleaseRootNode != null : el.EditorRootNode != null)).Select(el => release ? el.ReleaseRootNode : el.EditorRootNode).ToList();
                if (!derivedClasses.Any())
                {
                    continue;
                }
                var first = derivedClasses.FirstOrDefault();
                var sameFieldsCount = first.SubNodes.Count;
                foreach (var derivedClass in derivedClasses)
                {
                    sameFieldsCount = Math.Min(sameFieldsCount, derivedClass.SubNodes.Count);
                    for (var i = 0; i < sameFieldsCount; i++)
                    {
                        var derivedFieldNode = derivedClass.SubNodes[i];
                        var firstFieldNode = first.SubNodes[i];
                        if (derivedFieldNode.Name != firstFieldNode.Name || derivedFieldNode.TypeName != firstFieldNode.TypeName)
                        {
                            sameFieldsCount = i;
                        }
                    }
                }
                var subNodes = new List<UnityNode>();
                for (var i = 0; i < sameFieldsCount; i++)
                {
                    subNodes.Add(first.SubNodes[i].DeepClone());
                }
                var rootNode = new UnityNode
                {
                    TypeName = @class.Name,
                    Name = "Base",
                    Level = 0,
                    ByteSize = subNodes.Any(el => el.ByteSize == -1) ? -1 : subNodes.Sum(el => el.ByteSize),
                    Index = 0,
                    Version = 1,
                    TypeFlags = first.TypeFlags,
                    MetaFlag = first.MetaFlag,
                    SubNodes = subNodes,
                };
                if (release)
                {
                    @class.ReleaseRootNode = rootNode;
                }
                else
                {
                    @class.EditorRootNode = rootNode;
                }
            }
        }

        private void ProcessTypeFields(SimpleTypeDef type, Dictionary<(string, short, bool), SimpleTypeDef> types, Dictionary<(string, short, bool), TypePermutations> typePermutations)
        {
            if (type.Done)
            {
                return;
            }

            var permutations = typePermutations[(type.Name, type.Version, type.IsComponent)];
            var firstNode = permutations.Nodes.FirstOrDefault();
            if (firstNode == null)
            {
                type.Done = true;
                return;
            }

            var groupItemComparer = new GroupItemComparer();
            var genericFieldPaths = CalculateGenericFieldPaths(permutations, types);
            var genericNodePaths = CalculateGenericNodePaths(permutations);

            foreach (var fieldNode in firstNode.SubNodes)
            {
                var fieldDef = GetFieldDefForNode(fieldNode, types);
                type.Fields.Add(fieldDef);
            }

            if (genericFieldPaths.Count > 0)
            {
                var groups = new HashSet<List<(string, List<byte>)>>(new ListComparer<(string, List<byte>)>() { ItemComparer = groupItemComparer });
                foreach (var node in permutations.Nodes)
                {
                    var tmpGroups = new Dictionary<SimpleTypeDef, List<(string, List<byte>)>>();
                    foreach (var pathRow in genericFieldPaths)
                    {
                        var typeUsage = GetFieldDefForNode(node.SubNodes.FirstOrDefault(el => el.Name == pathRow.Item1), types).Type;
                        foreach (var i in pathRow.Item2)
                        {
                            typeUsage = typeUsage.GenericArgs[i];
                        }

                        var group = tmpGroups.GetOrAdd(typeUsage.Type);
                        group.Add(pathRow);
                    }
                    foreach (var group in tmpGroups.Values)
                    {
                        groups.Add(group);
                    }
                }

                var nodeGroups = new HashSet<List<(string, List<byte>)>>(new ListComparer<(string, List<byte>)>() { ItemComparer = groupItemComparer });
                foreach (var node in permutations.Nodes)
                {
                    var tmpGroups = new Dictionary<SimpleTypeDef, List<(string, List<byte>)>>();
                    foreach (var pathRow in genericNodePaths)
                    {
                        var cNode = node.SubNodes.FirstOrDefault(el => el.Name == pathRow.Item1);
                        foreach (var i in pathRow.Item2)
                        {
                            cNode = cNode.SubNodes[i];
                        }

                        var group = tmpGroups.GetOrAdd(cNode.AssosiatedTypeDef);
                        group.Add(pathRow);
                    }
                    foreach (var group in tmpGroups.Values)
                    {
                        nodeGroups.Add(group);
                    }
                }

                var finalGroups = SplitGroups(groups, groupItemComparer).OrderBy(el => el.Min(e => genericFieldPaths.IndexOf(e))).ToList();

                var typeToIndex = new Dictionary<SimpleTypeDef, int>();
                for (var i = 0; i < finalGroups.Count; i++)
                {
                    var group = finalGroups[i];
                    foreach (var pathRow in group)
                    {
                        var usage = TraverseFieldDef(type.Fields.First(f => f.Name == pathRow.Item1), pathRow.Item2);
                        usage.Type = null;
                        usage.GenericArgs.Clear();
                        usage.GenericIndex = i;
                    }
                }

                type.GenericNodesPaths.AddRange(SplitGroups(nodeGroups, groupItemComparer).OrderBy(el => el.Min(e => genericNodePaths.IndexOf(e))).ToList());
                type.GenericCount = finalGroups.Count;
            }

            var currentIndex = -1;
            string currentName = null;
            for (var i = 0; i < type.Fields.Count; i++)
            {
                var match = ArrayAccessPattern.Match(type.Fields[i].Name);
                if (!match.Success)
                {
                    ResetCurrent(i);
                    continue;
                }
                if (currentName != null && currentName != match.Groups["name"].Value)
                {
                    ResetCurrent(i);
                }
                if (int.Parse(match.Groups["index"].Value) != currentIndex + 1)
                {
                    ResetCurrent(i);
                    continue;
                }

                currentName = match.Groups["name"].Value;
                currentIndex++;
            }
            ResetCurrent(type.Fields.Count - 1);

            void ResetCurrent(int endIndex)
            {
                if (currentIndex != -1)
                {
                    var removeIndex = endIndex - currentIndex + 1;
                    var field = type.Fields[removeIndex - 1];
                    field.Name = currentName;
                    field.Type = new TypeUsageDef
                    {
                        GenericArgs = { new TypeUsageDef { Type = field.Type.Type } },
                        Type = PredefinedTypeDef.List,
                    };
                    field.FixedLength = currentIndex + 1;

                    for (; currentIndex > 0; currentIndex--)
                    {
                        type.Fields.RemoveAt(removeIndex);
                    }

                    currentName = null;
                    currentIndex = -1;
                }
            }

            foreach (var field in type.Fields)
            {
                if (FieldExistsInParent(field.Name, type))
                {
                    field.ExistsInBase = true;
                }
            }

            type.Done = true;
        }

        private static bool FieldExistsInParent(string fieldName, SimpleTypeDef type)
        {
            foreach (var baseType in type.RecursionUpwards())
            {
                if (baseType.Fields.Any(f => f.Name == fieldName))
                {
                    return true;
                }
            }
            return false;
        }

        private List<List<(string, List<byte>)>> SplitGroups(HashSet<List<(string, List<byte>)>> groups, IEqualityComparer<(string, List<byte>)> groupItemComparer)
        {
            var finalGroups = new List<List<(string, List<byte>)>>();
            var removeFinalGroups = new List<List<(string, List<byte>)>>();
            var addFinalGroups = new List<List<(string, List<byte>)>>();
            foreach (var group in groups)
            {
                var reducedGroup = group;
                foreach (var finalGroup in finalGroups)
                {
                    var finalReducedGroup = finalGroup.Except(reducedGroup, groupItemComparer).ToList();
                    if (finalReducedGroup.Count != 0)
                    {
                        var finalOldGroup = finalGroup.Except(finalReducedGroup, groupItemComparer).ToList();
                        if (finalOldGroup.Count != 0)
                        {
                            addFinalGroups.Add(finalReducedGroup);
                            addFinalGroups.Add(finalOldGroup);
                            removeFinalGroups.Add(finalGroup);
                        }
                    }
                    reducedGroup = reducedGroup.Except(finalGroup, groupItemComparer).ToList();
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

            return finalGroups;
        }

        private List<(string, List<byte>)> CalculateGenericNodePaths(TypePermutations permutations)
        {
            var firstNode = permutations.Nodes.FirstOrDefault();
            if (firstNode == null)
            {
                return new List<(string, List<byte>)>();
            }

            var groupItemComparer = new GroupItemComparer();
            var genericFieldPaths = new HashSet<(string, List<byte>)>(groupItemComparer);

            foreach (var node in permutations.Nodes)
            {
                for (var i = 0; i < node.SubNodes.Count; i++)
                {
                    var cNode = node.SubNodes[i];
                    RecursiveGatherPaths(cNode, firstNode.SubNodes[i], new List<byte>());

                    void RecursiveGatherPaths(UnityNode first, UnityNode second, IEnumerable<byte> path)
                    {
                        if (first.TypeName != second.TypeName)
                        {
                            genericFieldPaths.Add((cNode.Name, path.ToList()));
                            return;
                        }
                        for (byte i = 0; i < first.SubNodes.Count; i++)
                        {
                            RecursiveGatherPaths(first.SubNodes[i], second.SubNodes[i], path.Append(i));
                        }
                    }
                }
            }

            return ReorderAndRemoveIncludedPaths(genericFieldPaths, firstNode, groupItemComparer);
        }

        private List<(string, List<byte>)> CalculateGenericFieldPaths(TypePermutations permutations, Dictionary<(string, short, bool), SimpleTypeDef> types)
        {
            var firstNode = permutations.Nodes.FirstOrDefault();
            if (firstNode == null)
            {
                return new List<(string, List<byte>)>();
            }

            var groupItemComparer = new GroupItemComparer();
            var genericFieldPaths = new HashSet<(string, List<byte>)>(groupItemComparer);
            var firstNodeFieldDefs = firstNode.SubNodes.Select(el => GetFieldDefForNode(el, types)).ToList();

            foreach (var node in permutations.Nodes)
            {
                for (var i = 0; i < node.SubNodes.Count; i++)
                {
                    var fieldDef = GetFieldDefForNode(node.SubNodes[i], types);
                    RecursiveGatherPaths(fieldDef.Type, firstNodeFieldDefs[i].Type, new List<byte>());

                    void RecursiveGatherPaths(TypeUsageDef first, TypeUsageDef second, IEnumerable<byte> path)
                    {
                        if (first.Type != second.Type)
                        {
                            genericFieldPaths.Add((fieldDef.Name, path.ToList()));
                            return;
                        }

                        for (byte i = 0; i < first.GenericArgs.Count; i++)
                        {
                            RecursiveGatherPaths(first.GenericArgs[i], second.GenericArgs[i], path.Append(i));
                        }
                    }
                }
            }

            return ReorderAndRemoveIncludedPaths(genericFieldPaths, firstNode, groupItemComparer);
        }

        private List<(string, List<byte>)> ReorderAndRemoveIncludedPaths(HashSet<(string, List<byte>)> genericFieldPaths, UnityNode firstNode, IComparer<(int, string, List<byte>)> groupItemComparer)
        {
            var orderedPaths = genericFieldPaths.OrderBy(el => (firstNode.SubNodes.FindIndex(e => e.Name == el.Item1), el.Item1, el.Item2), groupItemComparer).ToList();
            var i = 1;
            while (i < orderedPaths.Count)
            {
                var previous = orderedPaths[i - 1];
                var current = orderedPaths[i];

                if (previous.Item2.Count < current.Item2.Count)
                {
                    var containsPrevious = true;
                    for (var j = 0; j < previous.Item2.Count; j++)
                    {
                        if (previous.Item2[j] != current.Item2[j])
                        {
                            containsPrevious = false;
                            break;
                        }
                    }
                    if (containsPrevious)
                    {
                        orderedPaths.RemoveAt(i);
                        continue;
                    }
                }
                i++;
            }
            return orderedPaths;
        }

        private static TypeUsageDef TraverseFieldDef(FieldDef fieldDef, IEnumerable<byte> path)
        {
            var typeUsage = fieldDef.Type;
            foreach (var i in path)
            {
                typeUsage = typeUsage.GenericArgs[i];
            }
            return typeUsage;
        }

        private static UnityNode TraverseFieldNode(UnityNode node, IEnumerable<byte> path)
        {
            var cNode = node;
            foreach (var i in path)
            {
                cNode = cNode.SubNodes[i];
            }
            return cNode;
        }

        /// <summary>
        /// Special treatment for VR settings because Unity is stupid
        /// Some types have different set of fields even though they have the same name
        /// Split these to separate permutations
        /// </summary>
        /// <param name="typePermutations"></param>
        private void FixPermutations(Dictionary<(string, short, bool), TypePermutations> typePermutations)
        {
            var comparer = new ListComparer<UnityNode> { ItemHashFunc = el => HashCode.Combine(el.Name) };
            var removePermutations = new List<(string, short, bool)>();
            var addPermutations = new List<(string, short, bool, TypePermutations)>();
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
                    //Assuming such types can't be generic, use hash of field types and names for unique names
                    var typeName = $"{firstNode.TypeName}_{(uint)firstNode.SubNodes.Aggregate(0, (total, el) => total * -1521134295 + el.TypeName.GetDeterministicHashCode() + el.Name.GetDeterministicHashCode())}";
                    foreach (var node in nodes)
                    {
                        node.TypeName = typeName;
                    }
                    addPermutations.Add((typeName, firstNode.Version, false, permutations));
                }
            }
            foreach (var row in removePermutations)
            {
                typePermutations.Remove(row);
            }
            foreach (var row in addPermutations)
            {
                typePermutations[(row.Item1, row.Item2, row.Item3)] = row.Item4;
            }
        }

        private static Dictionary<(string, short, bool), TypePermutations> GatherPermutations(IEnumerable<UnityClass> classes, bool release)
        {
            var typePermutations = new Dictionary<(string, short, bool), TypePermutations>();
            foreach (var @class in classes.Where(IsNotExistingType))
            {
                GatherPermutations(@class, typePermutations, release);
            }

            return typePermutations;
        }

        private static void GatherPermutations(UnityClass @class, Dictionary<(string, short, bool), TypePermutations> typePermutations, bool release)
        {
            var node = release ? @class.ReleaseRootNode : @class.EditorRootNode;
            typePermutations.GetOrAdd((@class.Name, node?.Version ?? 1, true)).Class = @class;
            if (node == null)
            {
                return;
            }

            foreach (var cNode in node.RecursionSimple())
            {
                if (ExistingTypes.Contains(cNode.TypeName, StringComparer.InvariantCultureIgnoreCase))
                {
                    cNode.AssosiatedTypeDef = GetPredefinedTypeDef(cNode);
                    continue;
                }

                _ = IsGenericTypeName(cNode.TypeName, out var name, out _, out _);

                typePermutations.GetOrAdd((name, cNode.Version, cNode == node)).Nodes.Add(cNode);
            }
        }

        private static bool IsGenericTypeName(string typeName, out string nameWithoutGeneric, out int genericCount, out string[] genericArgs)
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

        public static FieldDef GetFieldDefForNode(UnityNode node, Dictionary<(string, short, bool), SimpleTypeDef> types)
        {
            var fieldDef = new FieldDef
            {
                Name = node.Name,
                Type = GetTypeUsageDefForNode(node, types),
            };

            return fieldDef;
        }

        private static TypeUsageDef GetTypeUsageDefForNode(UnityNode node, Dictionary<(string, short, bool), SimpleTypeDef> types)
        {
            var typeDef = GetPredefinedTypeDef(node) ?? types[(node.AssosiatedTypeDef.Name, node.Version, false)];

            var genericDef = new TypeUsageDef
            {
                Type = typeDef,
                MetaFlags = node.MetaFlag,
            };

            var typeLower = node.TypeName.ToLowerInvariant();
            switch (typeLower)
            {
                case "pair":
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(node.SubNodes[0], types));
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(node.SubNodes[1], types));
                    break;
                case "map":
                case "fixed_bitset":
                case "staticvector":
                case "vector":
                case "set":
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(node.SubNodes[0].SubNodes[1], types));
                    genericDef.MetaFlags |= node.SubNodes[0].MetaFlag;
                    break;
                case "typelessdata":
                    genericDef.GenericArgs.Add(GetTypeUsageDefForNode(node.SubNodes[1], types));
                    break;
                default:
                    if (IsGenericTypeName(node.TypeName, out var nameWithoutGeneric, out _, out var args))
                    {
                        var isComponent = nameWithoutGeneric == "PPtr";
                        genericDef.GenericArgs.AddRange(args.Select(el => new TypeUsageDef { Type = types.FirstOrDefault(e => e.Key.Item1 == el && e.Key.Item3 == isComponent).Value }));
                    }
                    else if (typeDef.IsGeneric)
                    {
                        foreach (var pathRows in typeDef.GenericNodesPaths)
                        {
                            var firstRow = pathRows[0];
                            var cNode = TraverseFieldNode(node.SubNodes.FirstOrDefault(el => el.Name == firstRow.Item1), firstRow.Item2);
                            var def = GetTypeUsageDefForNode(cNode, types);
                            genericDef.GenericArgs.Add(def);
                        }
                    }
                    break;
            }

            return genericDef;
        }

        private static SimpleTypeDef GetPredefinedTypeDef(UnityNode node)
        {
            var typeLower = node.TypeName.ToLowerInvariant();
            return typeLower switch
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
                "map" => PredefinedTypeDef.List,
                "fixed_bitset" => PredefinedTypeDef.List,
                "staticvector" => PredefinedTypeDef.List,
                "vector" => PredefinedTypeDef.List,
                "set" => PredefinedTypeDef.List,
                "typelessdata" => PredefinedTypeDef.List,
                "array" => PredefinedTypeDef.List,
                _ => null
            };
        }

        private class TypePermutations
        {
            public UnityClass Class { get; set; }
            public List<UnityNode> Nodes { get; } = new List<UnityNode>();
        }

        private class ListComparer<T> : IEqualityComparer<List<T>>
        {
            public IEqualityComparer<T> ItemComparer { get; set; }
            public Func<T, int> ItemHashFunc { get; set; }
            public bool StrictOrder { get; set; }

            public bool Equals(List<T> x, List<T> y)
            {
                return x.Count == y.Count && !x.Where((el, i) => StrictOrder ? !ItemComparer.Equals(el, y[i]) : !y.Contains(el, ItemComparer)).Any();
            }

            public int GetHashCode([DisallowNull] List<T> obj)
            {
                return obj.Any() ? obj.Select(el => ItemHashFunc == null ? el.GetHashCode() : ItemHashFunc(el)).Aggregate((total, el) => total ^ el) : 0;
            }
        }

        private class GroupItemComparer : IEqualityComparer<(string, List<byte>)>, IComparer<(int, string, List<byte>)>
        {
            public int Compare((int, string, List<byte>) x, (int, string, List<byte>) y)
            {
                if (x.Item1 < y.Item1)
                {
                    return -1;
                }
                if (x.Item1 > y.Item1)
                {
                    return 1;
                }

                var count = Math.Min(x.Item3.Count, y.Item3.Count);
                for (var i = 0; i < count; i++)
                {
                    if (x.Item3[i] < y.Item3[i])
                    {
                        return -1;
                    }
                    if (x.Item3[i] > y.Item3[i])
                    {
                        return 1;
                    }
                }

                if (x.Item3.Count < y.Item3.Count)
                {
                    return -1;
                }
                if (x.Item3.Count > y.Item3.Count)
                {
                    return 1;
                }

                return 0;
            }

            public bool Equals((string, List<byte>) x, (string, List<byte>) y)
            {
                return x.Item1 == y.Item1 && x.Item2.Count == y.Item2.Count && x.Item2.All((el, i) => el == y.Item2[i]);
            }

            public int GetHashCode([DisallowNull] (string, List<byte>) obj)
            {
                return HashCode.Combine(obj.Item1.GetHashCode(), obj.Item2.Any() ? obj.Item2.Select(el => el.GetHashCode()).Aggregate((total, el) => total ^ el) : 0);
            }
        }
    }
}
