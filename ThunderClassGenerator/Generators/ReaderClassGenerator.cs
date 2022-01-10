using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Generators
{
    public class ReaderClassGenerator
    {
        public static SyntaxTree GetOrCreateTree(SimpleTypeDef typeDef)
        {
            var filePath = Path.Combine(Strings.SolutionFolder, Path.Combine(GeneratorUtilities.GetNamespaceString(typeDef).Split('.')), typeDef.Name, $"{typeDef.VersionnedName}_ReadBinary.cs");
            //if (File.Exists(filePath))
            //{
            //    return CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), new CSharpParseOptions(LangVersion), filePath);
            //}
            return CreateSyntaxTree(typeDef, filePath);
        }

        private static SyntaxTree CreateSyntaxTree(SimpleTypeDef typeDef, string filePath)
        {
            var root = SF.CompilationUnit(default, GetUsings(typeDef), default, GetNamespaceMember(typeDef));
            return CSharpSyntaxTree.Create(root, new CSharpParseOptions(GeneratorUtilities.LangVersion), filePath);
        }

        private static SyntaxList<MemberDeclarationSyntax> GetNamespaceMember(SimpleTypeDef typeDef)
        {
            var @class = SF.ClassDeclaration(default, GetClassModifiers(typeDef), SF.Identifier(typeDef.VersionnedName), GetTypeParameters(typeDef), default, default, GetMethods(typeDef));
            var @namespace = SF.NamespaceDeclaration(SF.IdentifierName(GeneratorUtilities.GetNamespaceString(typeDef)), default, default, SF.List(new MemberDeclarationSyntax[] { @class }));
            var comment = SF.Comment(Strings.CreatedWithComment);
            return SF.List(new MemberDeclarationSyntax[] { @namespace.WithLeadingTrivia(comment) });
        }

        private static SyntaxList<MemberDeclarationSyntax> GetMethods(SimpleTypeDef typeDef)
        {
            return SF.List(new[] { GetReadMethod(typeDef) });
        }

        private static MemberDeclarationSyntax GetReadMethod(SimpleTypeDef typeDef)
        {
            var modifiers = SF.TokenList(new[] { SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword) });
            var returnType = SF.ParseTypeName("void");
            var name = SF.Identifier("ReadBinary");
            var parameterList = SF.ParameterList(SF.SeparatedList(new[] { SF.Parameter(default, default, SF.ParseTypeName("SerializedReader"), SF.Identifier("reader"), default) }));
            var body = GerReadMethodBody(typeDef);
            return SF.MethodDeclaration(default, modifiers, returnType, default, name, default, parameterList, default, body, null);
        }

        private static BlockSyntax GerReadMethodBody(SimpleTypeDef typeDef)
        {
            var statements = new List<StatementSyntax>();
            //Calling base.ReadBinary()
            statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.BaseExpression(), SF.IdentifierName("ReadBinary")), SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.IdentifierName("reader"))})))));

            foreach (var field in typeDef.Fields.Values)
            {
                if (field.ExistsInBase)
                {
                    continue;
                }
                statements.Add(SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.IdentifierName(GeneratorUtilities.GetValidFieldName(field.Name)), SF.ObjectCreationExpression(SF.ParseName(GeneratorUtilities.GetFullFieldTypeName(field.Type)), SF.ArgumentList(), default))));
                statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(GeneratorUtilities.GetValidFieldName(field.Name)), SF.IdentifierName("ReadBinary")), SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.IdentifierName("reader")) })))));
                if ((field.Type.MetaFlags & (int)MetaFlag.AlignBytesFlag) != 0)
                {
                    statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("reader"), SF.IdentifierName("Align")))));
                }
            }

            return SF.Block(SF.List(statements));
        }

        private static SyntaxTokenList GetClassModifiers(SimpleTypeDef typeDef)
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

        private static TypeParameterListSyntax GetTypeParameters(SimpleTypeDef typeDef)
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

        private static SyntaxList<UsingDirectiveSyntax> GetUsings(SimpleTypeDef typeDef)
        {
            var usings = new HashSet<string>
            {
                Strings.CollectionsGeneric,
                Strings.ThunderRipperAttributes,
                Strings.ThunderRipperAssets,
                Strings.ThunderRipperUtilities,
            };

            if (typeDef.BaseType != null && !string.IsNullOrWhiteSpace(typeDef.BaseType.Namespace) && !typeDef.Namespace.StartsWith(typeDef.BaseType.Namespace))
            {
                usings.Add(GeneratorUtilities.GetNamespaceString(typeDef.BaseType));
            }

            foreach (var field in typeDef.Fields.Values)
            {
                if (field.ExistsInBase)
                {
                    continue;
                }
                GoOverGenericArgs(field.Type);

                void GoOverGenericArgs(TypeUsageDef usageDef)
                {
                    if (usageDef.GenericIndex != -1)
                    {
                        return;
                    }
                    usings.Add(GeneratorUtilities.GetNamespaceString(usageDef.Type));
                    foreach (var genericArg in usageDef.GenericArgs)
                    {
                        GoOverGenericArgs(genericArg);
                    }
                }
            }

            return SF.List(usings.OrderBy(el => el).Select(el => SF.UsingDirective(SF.IdentifierName(el))));
        }
    }
}
