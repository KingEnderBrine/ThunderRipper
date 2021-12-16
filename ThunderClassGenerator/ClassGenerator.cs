using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using ThunderClassGenerator.Rewriters;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator
{
    public static class ClassGenerator
    {
        private static LanguageVersion LangVersion => LanguageVersion.CSharp7;

        public static void Main(string[] args)
        {
            TypesReader.Test();
            return;
            var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(Path.Combine(@"D:\test\classdb\2017.1.0", "info.json")));
            var nodes = new HashSet<UnityNode>();
            foreach (var @class in info.Classes)
            {
                //Recursive(@class.EditorRootNode);
                Recursive(@class.ReleaseRootNode);
            }
            var ordered = nodes.OrderBy(el => el.TypeName);
            return;
            void Recursive(UnityNode node)
            {
                if (node == null)
                {
                    return;
                }
                nodes.Add(node);

                if (node.SubNodes != null)
                {
                    foreach (var cNode in node.SubNodes)
                    {
                        Recursive(cNode);
                    }
                }
            }
            //ImportClassesFile(@"D:\test\classdb\2017.1.0");
            ImportInfoFile(@"D:\test\classdb\2017.1.0");
            //TestCodeModel();
        }

        public static void TestCodeModel()
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText($@"{Strings.SolutionFolder}\ThunderRipper\Class1.cs"));
            var root = tree.GetRoot() as CompilationUnitSyntax;
            var namespaceSyntax = (root.Members[0] as NamespaceDeclarationSyntax).AddMembers(SyntaxFactory.ClassDeclaration(default, default, SyntaxFactory.Identifier("AAAAAA"), default, default, default, default));
            root = root.ReplaceNode(root.Members[0], namespaceSyntax).NormalizeWhitespace();
            File.WriteAllText($@"D:\test3.cs", root.SyntaxTree.ToString());
        }

        public static void ImportClassesFile(string path)
        {
            var classes = JsonSerializer.Deserialize<Dictionary<int, string>>(File.ReadAllText(Path.Combine(path, "classes.json")));
            using (var file = new StreamWriter(File.Create(Path.Combine(Strings.SolutionFolder, "ThunderRipper", "Unity", "UtilitiesIDToType.cs"))))
            {
                file.WriteLine(FileStrings.UtilitiesFileHeader);
                file.WriteLine(@"        public static readonly IReadOnlyDictionary<int, System.Type> IDToType = new Dictionary<int, System.Type>");
                file.WriteLine("        {");
                foreach (var row in classes)
                {
                    file.WriteLine($"            [{row.Key}] = typeof({row.Value}),");
                }
                file.WriteLine("        };");
                file.Write(FileStrings.UtilitiesFileFooter);
            }
        }

        public static void ImportInfoFile(string path)
        {
            var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(Path.Combine(path, "info.json")));
            var supportedVersionsTree = GetOrCreateTree(new SimpleTypeDef { Name = "SupportedVersions" });
            var supportedVersions = ReadSupportedVersions(supportedVersionsTree);
            if (supportedVersions.Contains(info.Version))
            {
                return;
            }

            var typeDefs = UnityClassesProcessor.GenerateTypeDefs(info.Classes);
            foreach (var typeDef in typeDefs)
            {
                var tree = GetOrCreateTree(typeDef);
                var root = tree.GetRoot();
                root = new AddFields(typeDef).Visit(root).NormalizeWhitespace();
                tree = tree.WithRootAndOptions(root, tree.Options);
                WriteTree(tree);
            }

            supportedVersionsTree = supportedVersionsTree.WithRootAndOptions(new AddSupportedVersion(info.Version).Visit(supportedVersionsTree.GetRoot()).NormalizeWhitespace(), supportedVersionsTree.Options);
            WriteTree(supportedVersionsTree);



            /*
            var nonProcessedTypes = new Dictionary<string, UnityNode>();
            var processedTypes = new Dictionary<string, DataTypeBuilder>();
            foreach (var @class in info.Classes)
            {
                var builder = new DataTypeBuilder(@class.Namespace, @class.Name, @class.Base);
                if (!processedTypes.ContainsKey(@class.Name))
                {
                    nonProcessedTypes.Remove(@class.Name);
                    processedTypes.Add(@class.Name, builder);
                }
                ProcessNode(builder, @class.EditorRootNode);
                ProcessNode(builder, @class.ReleaseRootNode);
            }

            while (nonProcessedTypes.Count > 0)
            {
                var row = nonProcessedTypes.First();
                var structType = row.Value;
                //TODO: Add detection of generic fields if there will be any
                //currently only generic type is PPtr and it doesn't have any generic fields
                var isGeneric = IsGenericType(structType.TypeName, out var nameWithoutGeneric, out var genericCount);
                var builder = new DataTypeBuilder(null, $"{nameWithoutGeneric}{(isGeneric ? $"<{string.Join(", ", Enumerable.Range(1, genericCount).Select(el => $"T{el}"))}>" : "")}", null, true);
                if (!processedTypes.ContainsKey(row.Key))
                {
                    nonProcessedTypes.Remove(row.Key);
                    processedTypes.Add(row.Key, builder);
                }
                ProcessNode(builder, structType);
            }

            var outputFolder = Path.Combine(Strings.SolutionFolder, "ThunderRipper", "Unity");
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            foreach (var row in processedTypes)
            {
                using (var file = new StreamWriter(File.Create(Path.Combine(outputFolder, $"{@row.Key}.cs"))))
                {
                    file.Write(row.Value.ToString());
                }
            }

            void ProcessNode(DataTypeBuilder builder, UnityNode node)
            {
                if (node == null)
                {
                    return;
                }
                foreach (var cNode in node.SubNodes)
                {
                    var property = builder.AddOrGetProperty(NormalizePropertyName(cNode), out var isNew);
                    if (isNew)
                    {
                        property.AddAttribute("SerializedName", $"\"{cNode.Name}\"");
                    }
                    property.Type = NormalizeNodeTypeName(cNode, out var specialType, out var collectionItemTypes);
                    if (specialType)
                    {
                        foreach (var row in collectionItemTypes)
                        {
                            IsGenericType(row.Key, out var typeNameWithoutGeneric, out _);
                            if (!processedTypes.ContainsKey(typeNameWithoutGeneric) && !nonProcessedTypes.ContainsKey(typeNameWithoutGeneric))
                            {
                                nonProcessedTypes.Add(typeNameWithoutGeneric, row.Value);
                            }
                        }
                        continue;
                    }
                    IsGenericType(property.Type, out var nameWithoutGeneric, out _);

                    if (!processedTypes.ContainsKey(nameWithoutGeneric) && !nonProcessedTypes.ContainsKey(nameWithoutGeneric))
                    {
                        nonProcessedTypes.Add(nameWithoutGeneric, cNode);
                    }
                }
            }*/
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

        private static IEnumerable<string> ReadSupportedVersions(SyntaxTree supportedVersionsTree)
        {
            var root = supportedVersionsTree.GetCompilationUnitRoot();
            var property = root
                ?.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault()
                ?.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault()
                ?.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault();
            if (property == null)
            {
                return Array.Empty<string>();
            }
            var expressions = (property.Initializer.Value as ArrayCreationExpressionSyntax).Initializer.Expressions;
            return expressions.Select(el => (el as LiteralExpressionSyntax).Token.Value as string);
        }

        private static PropertyDeclarationSyntax CreateVersionsProperty()
        {
            var modifiers = SF.TokenList(
                SF.Token(SyntaxKind.PublicKeyword),
                SF.Token(SyntaxKind.StaticKeyword));
            var accessors = SF.AccessorList(SF.List(new [] { SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration) }));
            var initializer = SF.EqualsValueClause(SF.InitializerExpression(SyntaxKind.ArrayCreationExpression));

            return SF.PropertyDeclaration(default, modifiers, SF.ParseTypeName("IEnumerable<string>"), default, SF.Identifier("Versions"), accessors, default, initializer);
        }

        public static SyntaxTree GetOrCreateTree(SimpleTypeDef typeDef)
        {
            var filePath = Path.Combine(Strings.SolutionFolder, Path.Combine(GetNamespaceString(typeDef).Split('.')), $"{typeDef.Name}.cs");
            if (File.Exists(filePath))
            {
                return CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), new CSharpParseOptions(LangVersion), filePath);
            }
            return CreateBaseClassSyntaxTree(typeDef, filePath);
        }

        public static SyntaxTree CreateBaseClassSyntaxTree(SimpleTypeDef typeDef, string filePath)
        {
            var root = SF.CompilationUnit(default, GetDefaultUsings(), default, GetDefaultMembers(typeDef));
            return CSharpSyntaxTree.Create(root, new CSharpParseOptions(LangVersion), filePath);
        }

        public static SyntaxList<UsingDirectiveSyntax> GetDefaultUsings()
        {
            var usings = new[]
            {
                SF.UsingDirective(SF.IdentifierName(Strings.CollectionsGeneric)),
                SF.UsingDirective(SF.IdentifierName(Strings.ThunderRipperAttributes)),
                SF.UsingDirective(SF.IdentifierName(Strings.ThunderRipperAssets)),
            };

            return SF.List(usings);
        }

        public static SyntaxList<MemberDeclarationSyntax> GetDefaultMembers(SimpleTypeDef typeDef)
        {
            var @class = SF.ClassDeclaration(default, GetClassModifiers(typeDef), SF.Identifier(typeDef.Name), GetTypeParameters(typeDef), GetBase(typeDef), default, default);
            var @namespace = SF.NamespaceDeclaration(GetNamespace(typeDef), default, default, SF.List(new MemberDeclarationSyntax[] { @class }));
            var comment =  SF.Comment(
$@"//------------------------------
//This class is managed by {nameof(ThunderClassGenerator)}
//Don't do any modifications in this file by hand
//------------------------------");
            return SF.List(new MemberDeclarationSyntax[] { @namespace.WithLeadingTrivia(comment) });
        }

        public static TypeParameterListSyntax GetTypeParameters(SimpleTypeDef typeDef)
        {
            if (typeDef.GenericCount == 0)
            {
                return default;
            }

            var types = new List<TypeParameterSyntax>();
            for (var i = 0; i < typeDef.GenericCount; i++)
            {
                types.Add(SF.TypeParameter($"T{i + 1}"));
            }

            return SF.TypeParameterList(SF.SeparatedList(types));
        }

        public static NameSyntax GetNamespace(SimpleTypeDef typeDef)
        {
            return SF.IdentifierName(GetNamespaceString(typeDef));
        }

        public static string GetNamespaceString(SimpleTypeDef typeDef)
        {
            return string.IsNullOrWhiteSpace(typeDef.Namespace) ? Strings.OutputNamespace : $"{Strings.OutputNamespace}.{typeDef.Namespace}";
        }

        public static SyntaxTokenList GetClassModifiers(SimpleTypeDef typeDef)
        {
            return SF.TokenList(GetTokens());

            IEnumerable<SyntaxToken> GetTokens()
            {
                yield return SF.Token(SyntaxKind.PublicKeyword);
                if (typeDef.IsAbstract)
                {
                    yield return SF.Token(SyntaxKind.AbstractKeyword);
                }
                //Partial must always be right before class
                yield return SF.Token(SyntaxKind.PartialKeyword);
            }
        }

        public static BaseListSyntax GetBase(SimpleTypeDef typeDef)
        {
            return SF.BaseList(SF.SeparatedList(GetBases()));

            IEnumerable<BaseTypeSyntax> GetBases()
            {
                if (!string.IsNullOrWhiteSpace(typeDef.Base))
                {
                    yield return SF.SimpleBaseType(SF.ParseTypeName(typeDef.Base));
                }
                yield return SF.SimpleBaseType(SF.ParseTypeName(Strings.IAsset));
            }
        }
    }
}
