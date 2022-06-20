﻿using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ThunderClassGenerator.Extensions;
using ThunderClassGenerator.Generators;
using ThunderClassGenerator.Rewriters;
using ThunderClassGenerator.Utilities;
using ThunderRipperShared.Utilities;

namespace ThunderClassGenerator
{
    public static class ClassGenerator
    {
        public static async Task Main(string[] args)
        {
            //IEnumerable<SimpleTypeDef> previousReleaseTypes = null;
            //IEnumerable<SimpleTypeDef> previousEditorTypes = null;
            //foreach (var file in Directory.EnumerateFiles(@"D:\Projects\RoR2\TypeTreeDumps\InfoJson").OrderBy(f => new UnityVersion(Path.GetFileNameWithoutExtension(f))))
            //{
            //    var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
            //    var releaseTypes = new TypesReader().ReadTypes(info.Classes, true);

            //    info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
            //    var editorTypes = new TypesReader().ReadTypes(info.Classes, false);

            //    if (previousReleaseTypes != null)
            //    {
            //        CompareFieldsOrder(releaseTypes, previousReleaseTypes);
            //    }
            //    if (previousEditorTypes != null)
            //    {
            //        CompareFieldsOrder(editorTypes, previousEditorTypes);
            //    }

            //    previousReleaseTypes = releaseTypes;
            //    previousEditorTypes = editorTypes;
            //}

            //void CompareFieldsOrder(IEnumerable<SimpleTypeDef> current, IEnumerable<SimpleTypeDef> previous)
            //{
            //    foreach (var prevType in previous)
            //    {
            //        var currentType = current.FirstOrDefault(t => t.Name == prevType.Name && t.IsComponent == prevType.IsComponent);
            //        if (currentType == null)
            //        {
            //            continue;
            //        }

            //        //var maxIndex = -1;

            //        foreach (var field in prevType.Fields)
            //        {
            //            var currentField = currentType.Fields.FirstOrDefault(f => f.Name == field.Name);
            //            if (currentField != null && currentField.FixedLength != field.FixedLength)
            //            {
            //                throw new System.Exception();
            //            }
            //            //var index = currentType.Fields.FindIndex(f => f.Name == field.Name);
            //            //if (index < 0)
            //            //{
            //            //    continue;
            //            //}
            //            //if (maxIndex >= index)
            //            //{
            //            //    throw new System.Exception();
            //            //}

            //            //maxIndex = index;
            //        }
            //    }
            //}


            //foreach (var file in Directory.EnumerateFiles(@"D:\RoR2 Modding\Repos\TypeTreeDumps\InfoJson").OrderBy(f => new UnityVersion(Path.GetFileNameWithoutExtension(f))))
            //{
            //    var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
            //    var releaseTypes = new TypesReader().ReadTypes(info.Classes, true);
            //    
            //    info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
            //    var editorTypes = new TypesReader().ReadTypes(info.Classes, false);
            //}

            MSBuildLocator.RegisterDefaults();
            using (var workspace = MSBuildWorkspace.Create())
            {
                //var solution = await workspace.OpenSolutionAsync(Path.Combine(Strings.SolutionFolder, "ThunderRipper.sln"));
                var project = await workspace.OpenProjectAsync(Path.Combine(Strings.SolutionFolder, Strings.DefaultNamespace, "ThunderRipperWorker.csproj"));
                project = project.WithParseOptions(new CSharpParseOptions(GeneratorUtilities.LangVersion, preprocessorSymbols: new[] { "CG" }));
                var releasePath = new[] { Strings.DefaultNamespace, Strings.Release };

                foreach (var file in Directory.EnumerateFiles(@"D:\RoR2 Modding\Repos\TypeTreeDumps\InfoJson").OrderBy(f => Random.Shared.Next(500)))
                {
                    var constantsPath = Path.Combine(Strings.SolutionFolder, Strings.DefaultNamespace, "Constants.cs");
                    var constantsTree = CSharpSyntaxTree.ParseText(File.ReadAllText(constantsPath), new CSharpParseOptions(GeneratorUtilities.LangVersion, preprocessorSymbols: new[] { "CG" }), constantsPath);
                    var constantsRoot = constantsTree.GetRoot();
                    var supportedVersions = SupportedVersionsUtilities.GetSupportedVersionsFromRoot(constantsRoot);

                    var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
                    var types = new TypesReader().ReadTypes(info.Classes, true);
                    var newVersion = new UnityVersion(info.Version);

                    //var existingTypes = new HashSet<SimpleTypeDef>();
                    //foreach (var document in project.Documents.Where(d => d.Folders.StartsWith(releasePath)))
                    //{
                    //    var type = types.FirstOrDefault(t => document.Name.StartsWith(t.VersionnedName));
                    //    if (type != null)
                    //    {
                    //        existingTypes.Add(type);
                    //
                    //        var root = await document.GetSyntaxRootAsync();
                    //        root = new ClassIfDirectiveRewriter(newVersion, supportedVersions, true, false).Visit(root);
                    //        Formatter.FormatAsync(document.WithSyntaxRoot(root));
                    //    }
                    //}

                    //constantsRoot = new TypeMappingRewriter(types, newVersion, supportedVersions).Visit(constantsRoot);
                    constantsRoot = new SupportedVersionsRewriter(newVersion).Visit(constantsRoot);

                    constantsRoot = Formatter.Format(constantsRoot, workspace);

                    constantsTree = constantsTree.WithRootAndOptions(constantsRoot, constantsTree.Options);
                    WriteTree(constantsTree);
                }
            }

            //foreach (var file in Directory.EnumerateFiles(@"D:\RoR2 Modding\Repos\TypeTreeDumps\InfoJson").OrderBy(f => Random.Shared.Next(500)))
            //{
            //    var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
            //    var supportedVersionsPath = Path.Combine(Strings.SolutionFolder, Strings.DefaultNamespace, "Constants.cs");
            //    var supportedVersionsTree = CSharpSyntaxTree.ParseText(File.ReadAllText(supportedVersionsPath), new CSharpParseOptions(GeneratorUtilities.LangVersion), supportedVersionsPath);
            //    var supportedVersionsRoot = new SupportedVersionsRewriter(new UnityVersion(info.Version)).Visit(supportedVersionsTree.GetRoot());
            //    supportedVersionsTree = supportedVersionsTree.WithRootAndOptions(supportedVersionsRoot, supportedVersionsTree.Options);
            //    WriteTree(supportedVersionsTree);
            //}

            //foreach (var typeDef in types)
            //{
            //    var tree = GetOrCreateTree(typeDef, (typeDef) => MainClassGenerator.CreateTree(typeDef, true), "");
            //    var root = tree.GetRoot();
            //    root = new FieldsRewriter(typeDef, new UnityVersion(info.Version)).Visit(root);
            //    tree = tree.WithRootAndOptions(root, tree.Options);
            //    WriteTree(tree);
            //    
            //    var readerTree = ReaderClassGenerator.GetOrCreateTree(typeDef);
            //    var readerRoot = readerTree.GetRoot().NormalizeWhitespace();
            //    readerTree = readerTree.WithRootAndOptions(readerRoot, readerTree.Options);
            //    WriteTree(readerTree);
            //
            //    var exporterTree = ExporterClassGenerator.GetOrCreateTree(typeDef);
            //    var exporterRoot = exporterTree.GetRoot().NormalizeWhitespace();
            //    exporterTree = exporterTree.WithRootAndOptions(exporterRoot, exporterTree.Options);
            //    WriteTree(exporterTree);
            //}
        }

        private static SyntaxTree GetOrCreateTree(SimpleTypeDef typeDef, Func<SimpleTypeDef, SyntaxTree> createTreeFunc, string fileNamePostfix)
        {
            var filePath = Path.Combine(Strings.SolutionFolder, Path.Combine(GeneratorUtilities.GetNamespaceString(typeDef).Split('.')), typeDef.Name, $"{typeDef.VersionnedName}{fileNamePostfix}.cs");
            if (File.Exists(filePath))
            {
                return CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), new CSharpParseOptions(GeneratorUtilities.LangVersion), filePath);
            }
            return createTreeFunc(typeDef).WithFilePath(filePath);
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
