using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Linq;
using System.Text.Json;
using ThunderClassGenerator.Generators;

namespace ThunderClassGenerator
{
    public static class ClassGenerator
    {
        public static void Main(string[] args)
        {
            var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(Path.Combine(@"D:\", "info.json")));
            var types = new TypesReader().ReadTypes(info.Classes);

            foreach (var typeDef in types)
            {
                var tree = MainClassGenerator.GetOrCreateTree(typeDef);
                var root = tree.GetRoot().NormalizeWhitespace();
                tree = tree.WithRootAndOptions(root, tree.Options);
                WriteTree(tree);

                var readerTree = ReaderClassGenerator.GetOrCreateTree(typeDef);
                var readerRoot = readerTree.GetRoot().NormalizeWhitespace();
                readerTree = readerTree.WithRootAndOptions(readerRoot, readerTree.Options);
                WriteTree(readerTree);
            }

            File.WriteAllText(@"D:\classes.cs", string.Join(",\n", types.Where(el => el.TypeID != -1).Select(el => $"[{el.TypeID}] = {el.VersionnedName}")));
        }

        private static void WriteTree(SyntaxTree tree)
        {
            var directory = Path.GetDirectoryName(tree.FilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(tree.FilePath, tree.ToString());
        }
    }
}
