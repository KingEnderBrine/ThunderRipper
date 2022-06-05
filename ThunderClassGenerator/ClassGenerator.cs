using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Linq;
using System.Text.Json;
using ThunderClassGenerator.Generators;
using ThunderRipperShared.Utilities;

namespace ThunderClassGenerator
{
    public static class ClassGenerator
    {
        public static void Main(string[] args)
        {
            /*
            foreach (var file in Directory.EnumerateFiles(@"D:\RoR2 Modding\Repos\TypeTreeDumps\InfoJson").OrderBy(f => new UnityVersion(Path.GetFileNameWithoutExtension(f))))
            {
                var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
                var releaseTypes = new TypesReader().ReadTypes(info.Classes, true);

                info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
                var editorTypes = new TypesReader().ReadTypes(info.Classes, false);
            }
            */

            var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(@"D:\info.json"));
            var types = new TypesReader().ReadTypes(info.Classes, true);

            foreach (var typeDef in types)
            {
                var tree = MainClassGenerator.GetOrCreateTree(typeDef);
                var root = tree.GetRoot().NormalizeWhitespace();
                tree = tree.WithRootAndOptions(root, tree.Options);
                WriteTree(tree);
                /*
                var readerTree = ReaderClassGenerator.GetOrCreateTree(typeDef);
                var readerRoot = readerTree.GetRoot().NormalizeWhitespace();
                readerTree = readerTree.WithRootAndOptions(readerRoot, readerTree.Options);
                WriteTree(readerTree);

                var exporterTree = ExporterClassGenerator.GetOrCreateTree(typeDef);
                var exporterRoot = exporterTree.GetRoot().NormalizeWhitespace();
                exporterTree = exporterTree.WithRootAndOptions(exporterRoot, exporterTree.Options);
                WriteTree(exporterTree);*/
            }
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
