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
            var info = JsonSerializer.Deserialize<Info>(File.ReadAllText(Path.Combine(@"D:\", "info.json")));
            AssignNodeParents(info);
            var types = new TypesReader().ReadTypes(info.Classes);

            foreach (var typeDef in types)
            {
                var tree = GetOrCreateTree(typeDef);
                var root = tree.GetRoot();
                root = new AddFields(typeDef).Visit(root).NormalizeWhitespace();
                tree = tree.WithRootAndOptions(root, tree.Options);
                WriteTree(tree);
            }

            return;
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

        private static void AssignNodeParents(Info info)
        {
            foreach (var @class in info.Classes)
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
            var filePath = Path.Combine(Strings.SolutionFolder, Path.Combine(GetNamespaceString(typeDef).Split('.')), $"{typeDef.VersionnedName}.cs");
            if (File.Exists(filePath))
            {
                return CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), new CSharpParseOptions(LangVersion), filePath);
            }
            return CreateBaseClassSyntaxTree(typeDef, filePath);
        }

        public static SyntaxTree CreateBaseClassSyntaxTree(SimpleTypeDef typeDef, string filePath)
        {
            var root = SF.CompilationUnit(default, GetDefaultUsings(typeDef), default, GetDefaultMembers(typeDef));
            return CSharpSyntaxTree.Create(root, new CSharpParseOptions(LangVersion), filePath);
        }

        public static SyntaxList<UsingDirectiveSyntax> GetDefaultUsings(SimpleTypeDef typeDef)
        {
            var usings = new HashSet<string>
            {
                Strings.CollectionsGeneric,
                Strings.ThunderRipperAttributes,
                Strings.ThunderRipperAssets,
            };

            if (typeDef.BaseType != null && !string.IsNullOrWhiteSpace(typeDef.BaseType.Namespace) && !typeDef.Namespace.StartsWith(typeDef.BaseType.Namespace))
            {
                usings.Add(GetNamespaceString(typeDef.BaseType));
            }

            foreach (var field in typeDef.Fields.Values)
            {
                if (field.GenericIndex != -1)
                {
                    continue;
                }
                usings.Add(GetNamespaceString(field.Type));
                GoOverGenericArgs(field.GenericArgs);

                void GoOverGenericArgs(IEnumerable<GenericDef> args)
                {
                    foreach (var genericArg in args)
                    {
                        usings.Add(GetNamespaceString(genericArg.TypeDef));
                        GoOverGenericArgs(genericArg.GenericArgs);
                    }
                }
            }

            return SF.List(usings.Select(el => SF.UsingDirective(SF.IdentifierName(el))));
        }

        public static SyntaxList<MemberDeclarationSyntax> GetDefaultMembers(SimpleTypeDef typeDef)
        {
            var @class = SF.ClassDeclaration(default, GetClassModifiers(typeDef), SF.Identifier(typeDef.VersionnedName), GetTypeParameters(typeDef), GetBase(typeDef), default, GetFields(typeDef));
            var @namespace = SF.NamespaceDeclaration(GetNamespace(typeDef), default, default, SF.List(new MemberDeclarationSyntax[] { @class }));
            var comment =  SF.Comment(
$@"//------------------------------
//This class is managed by {nameof(ThunderClassGenerator)}
//Don't do any modifications in this file by hand
//------------------------------");
            return SF.List(new MemberDeclarationSyntax[] { @namespace.WithLeadingTrivia(comment) });
        }

        public static SyntaxList<MemberDeclarationSyntax> GetFields(SimpleTypeDef typeDef)
        {
            var fields = new List<MemberDeclarationSyntax>();

            foreach (var field in typeDef.Fields.Values)
            {
                fields.Add(SF.PropertyDeclaration(SF.List<AttributeListSyntax>(), SF.TokenList(), SF.ParseTypeName(GetFieldTypeName(field)), null, SF.Identifier(field.Name), SF.AccessorList(SF.List(new[] { SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)), SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)) }))).WithTrailingTrivia(SF.LineFeed));
            }

            return SF.List(fields);
        }

        public static string GetFieldTypeName(FieldDef field)
        {
            if (field.GenericIndex != -1)
            {
                return $"T{field.GenericIndex + 1}";
            }
            if (field.Type.GenericCount == 0)
            {
                return field.Type.VersionnedName;
            }

            return $"{field.Type.VersionnedName}<{string.Join(", ", field.GenericArgs.Select(el => GetGenericArgTypeName(el)))}>";
        }

        public static string GetGenericArgTypeName(GenericDef genericDef)
        {
            if (genericDef.TypeDef.GenericCount == 0)
            {
                return genericDef.TypeDef.VersionnedName;
            }
            return $"{genericDef.TypeDef.VersionnedName}<{string.Join(", ", genericDef.GenericArgs.Select(el => GetGenericArgTypeName(el)))}>";
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
            return string.IsNullOrWhiteSpace(typeDef.Namespace) ? Strings.OutputNamespace : typeDef is PredefinedTypeDef ? typeDef.Namespace : $"{Strings.OutputNamespace}.{typeDef.Namespace}";
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
                if (typeDef.BaseType != null)
                {
                    yield return SF.SimpleBaseType(SF.ParseTypeName(typeDef.BaseType.VersionnedName));
                }
                yield return SF.SimpleBaseType(SF.ParseTypeName(Strings.IAsset));
            }
        }
    }
}
