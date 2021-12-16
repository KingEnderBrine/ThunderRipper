using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    public class TypesReader
    {
        private readonly Dictionary<string, Permutation> typePermutations = new Dictionary<string, Permutation>();
        private readonly Dictionary<string, SimpleTypeDef> typeDefs = new Dictionary<string, SimpleTypeDef>();

        public IEnumerable<SimpleTypeDef> ReadTypes(IEnumerable<UnityClass> classes, bool release = true)
        {
            var classTypes = new List<SimpleTypeDef>();
            var structTypes = new List<SimpleTypeDef>();

            foreach (var @class in classes)
            {
                var rootNode = release ? @class.ReleaseRootNode : @class.EditorRootNode;
                GatherPermutations(rootNode);
            }

            while (typePermutations.Count > 0)
            {
                var row = typePermutations.First();
                var permutations = row.Value;
                var typeDef = typeDefs[row.Key] = new SimpleTypeDef();
                typeDef.Base = permutations.Class?.Base;
                typeDef.IsAbstract = permutations.Class?.IsAbstract ?? false;
                typeDef.IsStruct = permutations.Class == null;
                typeDef.Namespace = permutations.Class?.Namespace;
                typeDef.TypeID = permutations.Class?.TypeID ?? -1;
                typeDef.Version = permutations.Nodes.First().Version;
                typeDef.Name = row.Key;
                foreach (var fieldRow in CalculateGenericFields(permutations.Nodes))
                {
                    typeDef.GenericFields[fieldRow.Key] = fieldRow.Value;
                    typeDef.GenericCount = Math.Max(typeDef.GenericCount, fieldRow.Value + 1);
                }
            }

            return classTypes.Union(structTypes);
        }

        private static Dictionary<string, int> CalculateGenericFields(List<UnityNode> nodes)
        {
            var fields = new Dictionary<string, int>();
            var groups = new List<List<string>>();
            var tmpGroups = new Dictionary<string, List<string>>();

            foreach (var node in nodes)
            {
                tmpGroups.Clear();
                foreach (var field in node.SubNodes)
                {
                    if (!tmpGroups.TryGetValue(field.TypeName, out var group))
                    {
                        tmpGroups[field.TypeName] = group = new List<string>();
                    }
                    group.Add(field.Name);
                }
                groups.AddRange(tmpGroups.Values);
            }


            return fields;
        }

        private void GatherPermutations(UnityNode node)
        {
            if (!typePermutations.TryGetValue(node.TypeName, out var permutations))
            {
                typePermutations[node.TypeName] = permutations = new Permutation();
            }
            permutations.Nodes.Add(node);
            foreach (var cNode in node.SubNodes)
            {
                GatherPermutations(cNode);
            }
        }

        private class Permutation
        {
            public UnityClass Class { get; set; }
            public List<UnityNode> Nodes { get; } = new List<UnityNode>();
        }

        public static void Test()
        {
            var types = new[] { "string", "int", "byte", "short", "long" };
            var permutations = GetPermutationsWithRept(types, types.Length).ToList();
            var nodes = new List<UnityNode>();
            for (var i = 0; i < permutations.Count; i++)
            {
                var p = permutations[i];
                var node = new UnityNode();
                node.SubNodes = new List<UnityNode>();
                node.SubNodes.Add(new UnityNode { TypeName = "string", Name = $"f0" });
                for (var j = 0; j < types.Length; j++)
                {
                    var cNode = new UnityNode();
                    cNode.TypeName = p.ElementAt(j);
                    cNode.Name = $"f{j + 1}";
                    node.SubNodes.Add(cNode);
                }
                nodes.Add(node);
            }

            var fields = CalculateGenericFields(nodes);

            static IEnumerable<IEnumerable<T>> GetPermutationsWithRept<T>(IEnumerable<T> list, int length)
            {
                if (length == 1) return list.Select(t => new T[] { t });
                return GetPermutationsWithRept(list, length - 1)
                    .SelectMany(t => list,
                        (t1, t2) => t1.Concat(new T[] { t2 }));
            }
        }

        private class GenericTest<T1, T2, T3, T4, T5>
        {
            public string f0;
            public T1 f1;
            public T2 f2;
            public T3 f3;
            public T4 f4;
            public T5 f5;
        }

    }
}
