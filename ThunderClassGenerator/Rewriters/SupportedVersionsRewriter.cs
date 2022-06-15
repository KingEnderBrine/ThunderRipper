using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using ThunderClassGenerator.Utilities;
using ThunderRipperShared.Utilities;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Rewriters
{
    public class SupportedVersionsRewriter : CSharpSyntaxRewriter
    {
        private readonly UnityVersion version;
        public SupportedVersionsRewriter(UnityVersion version)
        {
            this.version = version;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Members.OfType<FieldDeclarationSyntax>().Any(f => f.Declaration.Variables[0].Identifier.ValueText == "SupportedVersions"))
            {
                return base.VisitClassDeclaration(node);
            }
            return base.VisitClassDeclaration(node.WithMembers(node.Members.Add(CreateField())));
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.Declaration.Variables[0].Identifier.ValueText == "SupportedVersions")
            {
                return new MultilineCollectionRewriter().Visit(base.VisitFieldDeclaration(node));
            }

            return node;
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            if (node.Parent is not ArrayCreationExpressionSyntax)
            {
                return base.VisitInitializerExpression(node);
            }

            var index = node.Expressions.LastIndexOf(e => e is ObjectCreationExpressionSyntax objectCreation && (SupportedVersionsUtilities.GetVersionFromCreationExpression(objectCreation) < version)) + 1;

            return node.WithExpressions(SF.SeparatedList(node.Expressions.Insert(index, CreateVersionExpression(version))));
        }

        private static FieldDeclarationSyntax CreateField()
        {
            return SF.FieldDeclaration(
                    default,
                    SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.StaticKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword)),
                    SF.VariableDeclaration(
                        SF.ParseTypeName("UnityVersion[]"),
                        SF.SeparatedList(new[] {
                            SF.VariableDeclarator(
                                SF.Identifier("SupportedVersions"),
                                default,
                                SF.EqualsValueClause(
                                    SF.ArrayCreationExpression(
                                        SF.ArrayType(SF.ParseTypeName("UnityVersion[]")),
                                        SF.InitializerExpression(
                                            SyntaxKind.ArrayInitializerExpression,
                                            SF.SeparatedList<ExpressionSyntax>()))))
                        })))
                .NormalizeWhitespace()
                .WithLeadingTrivia(SF.TriviaList(SF.Tab, SF.Tab))
                .WithTrailingTrivia(SF.LineFeed);
        }

        private static ObjectCreationExpressionSyntax CreateVersionExpression(UnityVersion version)
        {
            return SF.ObjectCreationExpression(
                SF.ParseTypeName(nameof(UnityVersion)),
                SF.ArgumentList(
                    SF.SeparatedList(new[] {
                        SF.Argument(
                            SF.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SF.Literal(version.ToString())))
                    }))
                , default)
                .NormalizeWhitespace();
        }
    }
}
