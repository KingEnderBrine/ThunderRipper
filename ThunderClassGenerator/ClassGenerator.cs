using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
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
                var originalParseOptions = project.ParseOptions;
                project = project.WithParseOptions(new CSharpParseOptions(GeneratorUtilities.LangVersion, preprocessorSymbols: new[] { "CG" }));
                var componentsPath = new[] { Strings.Release, Strings.UnityComponents };
                var classesPath = new[] { Strings.Release, Strings.UnityClasses };

                var constantsId = project.Documents.FirstOrDefault(d => d.Folders.Count == 0 && d.Name == "Constants.cs").Id;

                foreach (var file in Directory.EnumerateFiles(@"D:\RoR2 Modding\Repos\TypeTreeDumps\InfoJson").OrderBy(f => Random.Shared.Next(500)))
                {
                    var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(file));
                    var newVersion = new UnityVersion(info.Version);

                    var constantsDocument = project.GetDocument(constantsId);
                    var constantsRoot = await constantsDocument.GetSyntaxRootAsync();
                    
                    var supportedVersions = SupportedVersionsUtilities.GetSupportedVersionsFromRoot(constantsRoot);
                    if (supportedVersions.Contains(newVersion))
                    {
                        continue;
                    }

                    var types = new TypesReader().ReadTypes(info.Classes, true);
                    
                    constantsRoot = new TypeMappingRewriter(types, newVersion, supportedVersions).Visit(constantsRoot);
                    constantsRoot = new SupportedVersionsRewriter(newVersion).Visit(constantsRoot);
                    constantsDocument = await Formatter.FormatAsync(constantsDocument.WithSyntaxRoot(constantsRoot));
                    project = constantsDocument.Project;

                    var existingTypes = new HashSet<SimpleTypeDef>();
                    foreach (var documentId in project.DocumentIds)
                    {
                        var document = project.GetDocument(documentId);
                        var isComponent = document.Folders.StartsWith(componentsPath);
                        var isClass = !isComponent && document.Folders.StartsWith(classesPath);

                        if (!isComponent && !isClass)
                        {
                            continue;
                        }

                        var type = types.FirstOrDefault(t => t.IsComponent == isComponent && (document.Name == t.VersionnedName + ".cs" || document.Name.StartsWith(t.VersionnedName + '_')));
                        existingTypes.Add(type);
              
                        var root = await document.GetSyntaxRootAsync();
                        root = new ClassIfDirectiveRewriter(newVersion, supportedVersions, type != null, false).Visit(root);
                        if (document.Name.EndsWith("_ReadBinary.cs"))
                        {
                            //TODO: update binary
                        }
                        else if (document.Name.EndsWith("_ExportYAML.cs"))
                        {
                            //TODO: update YAML
                        }
                        else
                        {
                            if (type != null)
                            {
                                root = new FieldsRewriter(type, newVersion, supportedVersions, false).Visit(root);
                            }
                        }
                        document = await Formatter.FormatAsync(document.WithSyntaxRoot(root));
                        project = document.Project;
                    }

                    var projectDir = Path.GetDirectoryName(project.FilePath);
                    foreach (var type in types.Except(existingTypes))
                    {
                        var folders = GeneratorUtilities.GetNamespaceString(type).Split('.').Skip(1).Append(type.Name).ToArray();
                        
                        var mainDocument = await CreateDocument(type, true, "", folders, new FieldsRewriter(type, newVersion, supportedVersions, true), new ClassIfDirectiveRewriter(newVersion, supportedVersions, true, true));
                        project = mainDocument.Project;

                        //var readBinaryDocument = await CreateDocument(type, false, "_ReadBinary", folders, new ClassIfDirectiveRewriter(newVersion, supportedVersions, true, true));
                        //project = readBinaryDocument.Project;
                        //
                        //var exportYAMLDocument = await CreateDocument(type, false, "_ExportYAML", folders, new ClassIfDirectiveRewriter(newVersion, supportedVersions, true, true));
                        //project = exportYAMLDocument.Project;

                        async Task<Document> CreateDocument(SimpleTypeDef type, bool mainDocument, string postfix, string[] folders, params CSharpSyntaxRewriter[] rewriters)
                        {
                            var filePath = Path.Combine(projectDir, Path.Combine(folders), $"{type.VersionnedName}{postfix}.cs");

                            var root = MainClassGenerator.CreateRoot(type, mainDocument);
                            foreach (var rewriter in rewriters)
                            {
                                root = rewriter.Visit(root);
                            }

                            var document = project.AddDocument(Path.GetFileName(filePath), root, folders, filePath);
                            return await Formatter.FormatAsync(document.WithSyntaxRoot(root));
                        }
                    }

                    //break;
                }
                workspace.TryApplyChanges(project.WithParseOptions(originalParseOptions).Solution);
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

            //var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(@"D:\info.json"));
            //var types = new TypesReader().ReadTypes(info.Classes, true);
            //foreach (var typeDef in types)
            //{
            //    var tree = GetOrCreateTree(typeDef, (typeDef) => MainClassGenerator.CreateTree(typeDef, true), "");
            //    var root = tree.GetRoot().NormalizeWhitespace();
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
