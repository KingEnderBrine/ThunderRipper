using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Generators
{
    public class MainClassGenerator
    {
        public static SyntaxTree CreateTree(SimpleTypeDef typeDef, bool mainFile)
        {
            var root = SF.CompilationUnit(default, GeneratorUtilities.GetUsings(typeDef), default, GetNamespaceMember(typeDef, mainFile));
            return CSharpSyntaxTree.Create(root, new CSharpParseOptions(GeneratorUtilities.LangVersion));
        }

        private static SyntaxList<MemberDeclarationSyntax> GetNamespaceMember(SimpleTypeDef typeDef, bool mainFile)
        {
            var @class = mainFile ?
                SF.ClassDeclaration(default, GetClassModifiers(typeDef), SF.Identifier(typeDef.VersionnedName), GetTypeParameters(typeDef), GetBase(typeDef), GetClassConstraints(typeDef), GetFields(typeDef))
                : SF.ClassDeclaration(default, GetClassModifiers(typeDef), SF.Identifier(typeDef.VersionnedName), GetTypeParameters(typeDef), default, default, default);
            var @namespace = SF.NamespaceDeclaration(GeneratorUtilities.GetNamespaceIdentifier(typeDef), default, default, SF.List(new MemberDeclarationSyntax[] { @class }));
            
            return SF.List(new MemberDeclarationSyntax[] { @namespace });
        }

        private static SyntaxList<TypeParameterConstraintClauseSyntax> GetClassConstraints(SimpleTypeDef typeDef)
        {
            var constraints = new List<TypeParameterConstraintClauseSyntax>();
            if (typeDef.GenericNodesPaths.Any())
            {
                for (var i = 0; i < typeDef.GenericCount; i++)
                {
                    constraints.Add(SF.TypeParameterConstraintClause(SF.IdentifierName($"T{i + 1}"), SF.SeparatedList(new TypeParameterConstraintSyntax[] { SF.TypeConstraint(SF.ParseTypeName("IBinaryReadable")), SF.TypeConstraint(SF.ParseTypeName("IYAMLExportable")), SF.ConstructorConstraint() })));
                }
            }

            return SF.List(constraints);
        }

        private static SyntaxList<MemberDeclarationSyntax> GetFields(SimpleTypeDef typeDef)
        {
            var members = new List<MemberDeclarationSyntax>();

            members.Add(
                    SF.PropertyDeclaration(
                        default,
                        SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)),
                        SF.ParseTypeName("int"),
                        default,
                        SF.Identifier("Version"),
                        default,
                        SF.ArrowExpressionClause(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(typeDef.Version))),
                        default)
                    .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken))
                    .WithTrailingTrivia(SF.LineFeed));

            members.Add(
                    SF.PropertyDeclaration(
                        default,
                        SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword)),
                        SF.ParseTypeName("MappingStyle"),
                        default,
                        SF.Identifier("MappingStyle"),
                        default,
                        SF.ArrowExpressionClause(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.ParseTypeName("MappingStyle"), SF.IdentifierName(typeDef.FlowMapping ? "Flow" : "Block"))),
                        default)
                    .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken))
                    .WithTrailingTrivia(SF.LineFeed));

            /*var addedFieldCount = 0;
            foreach (var field in typeDef.Fields)
            {
                if (field.ExistsInBase)
                {
                    continue;
                }
                fields.Add(
                    SF.FieldDeclaration(
                        SF.List(GetFieldAttributes(field, addedFieldCount)),
                        SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)),
                        SF.VariableDeclaration(
                            SF.ParseTypeName(field.Type.FullName),
                            SF.SeparatedList(new[] { SF.VariableDeclarator(GeneratorUtilities.GetValidFieldName(field.Name)) })))
                    .WithTrailingTrivia(SF.LineFeed));
                addedFieldCount++;
            }*/

            return SF.List(members);
        }

        /*private static IEnumerable<AttributeListSyntax> GetFieldAttributes(FieldDef field, int order)
        {
            if ((field.Type.MetaFlags & (int)MetaFlag.AlignBytesFlag) != 0)
            {
                yield return GeneratorUtilities.CreateSimpleAttribute("Align");
            }

            yield return GeneratorUtilities.CreateSimpleAttribute("Order", new[] { SF.AttributeArgument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(order))) });
            yield return GeneratorUtilities.CreateSimpleAttribute("SerializedName", new[] { SF.AttributeArgument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(field.Name))) });
        }*/

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

        private static BaseListSyntax GetBase(SimpleTypeDef typeDef)
        {
            return SF.BaseList(SF.SeparatedList(new BaseTypeSyntax[] { SF.SimpleBaseType(SF.ParseTypeName(typeDef.BaseType?.VersionnedName ?? Strings.AssetBase)) }));
        }
    }
}
