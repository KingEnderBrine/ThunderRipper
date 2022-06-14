using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRipperShared.Utilities;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Rewriters
{
    public class TypeMappingRewriter : CSharpSyntaxRewriter
    {
        private readonly IEnumerable<SimpleTypeDef> types;
        private readonly UnityVersion version;
        private readonly IEnumerable<UnityVersion> supportedVersions;

        public TypeMappingRewriter(IEnumerable<SimpleTypeDef> types, UnityVersion version, IEnumerable<UnityVersion> supportedVersions)
        {
            this.types = types;
            this.version = version;
            this.supportedVersions = supportedVersions;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Members.OfType<FieldDeclarationSyntax>().Any(f => f.Declaration.Variables[0].Identifier.ValueText == "TypeIDToType"))
            {
                return base.VisitClassDeclaration(node);
            }
            return base.VisitClassDeclaration(node.WithMembers(node.Members.Add(CreateField())));
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.Declaration.Variables[0].Identifier.ValueText == "TypeIDToType")
            {
                return new MultilineCollectionRewriter().Visit(base.VisitFieldDeclaration(node));
            }

            return node;
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var existingMappings = node.Expressions.Select(e => GetMappingFromCreationExpression(e as AssignmentExpressionSyntax)).ToArray();
            var index = 0;
            return node.WithExpressions(SF.SeparatedList(node.Expressions.Insert(index, CreateRowExpression(types.FirstOrDefault()))));
        }

        private static FieldDeclarationSyntax CreateField()
        {
            return SF.FieldDeclaration(
                    default,
                    SF.TokenList(SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.StaticKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword)),
                    SF.VariableDeclaration(
                        SF.ParseTypeName("Dictionary<int, Type>"),
                        SF.SeparatedList(new[] {
                            SF.VariableDeclarator(
                                SF.Identifier("TypeIDToType"),
                                default,
                                SF.EqualsValueClause(
                                    SF.ObjectCreationExpression(
                                        SF.ParseTypeName("Dictionary<int, Type>"),
                                        default,
                                        SF.InitializerExpression(
                                            SyntaxKind.ObjectInitializerExpression,
                                            SF.SeparatedList<ExpressionSyntax>()))))
                        })))
                .NormalizeWhitespace()
                .WithLeadingTrivia(SF.TriviaList(SF.Tab, SF.Tab))
                .WithTrailingTrivia(SF.LineFeed);
        }

        private static AssignmentExpressionSyntax CreateRowExpression(SimpleTypeDef type)
        {
            return SF.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SF.ImplicitElementAccess(SF.BracketedArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(type.TypeID)))}))),
                SF.TypeOfExpression(SF.ParseTypeName(type.VersionnedName)))
                .NormalizeWhitespace();
        }

        private static (int, string) GetMappingFromCreationExpression(AssignmentExpressionSyntax expression)
        {
            if (expression == null)
            {
                return default;
            }

            var leftFirstArgument = (expression.Left as ImplicitElementAccessSyntax)?.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (leftFirstArgument is not LiteralExpressionSyntax literal || !literal.Token.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return default;
            }

            var typeName = (expression.Right as TypeOfExpressionSyntax)?.Type.ToString();
            if (typeName == null)
            {
                return default;
            }

            return ((int)literal.Token.Value, typeName);
        }
    }
}
