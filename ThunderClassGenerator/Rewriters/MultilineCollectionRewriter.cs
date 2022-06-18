using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Rewriters
{
    public class MultilineCollectionRewriter : CSharpSyntaxRewriter
    {
        private SyntaxTriviaList fieldIndentation;

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var trivia = node.GetLeadingTrivia();
            fieldIndentation = node.GetLeadingTrivia().Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)).ToSyntaxTriviaList();

            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            return base.VisitArrayCreationExpression(node.WithType(node.Type.WithoutTrivia()));
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            return node
                .WithoutTrivia()
                .WithOpenBraceToken(SF.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(fieldIndentation.Prepend(SF.LineFeed)).WithTrailingTrivia(fieldIndentation.Prepend(SF.LineFeed)))
                .WithCloseBraceToken(SF.Token(SyntaxKind.CloseBraceToken))
                .WithExpressions(SF.SeparatedList(node.Expressions.Select(e => e.WithoutTrivia()), node.Expressions.Select(e => SF.Token(SyntaxKind.CommaToken).WithLeadingTrivia(SF.Tab).WithTrailingTrivia(fieldIndentation.Prepend(SF.LineFeed)))));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) => node;
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node) => node;
        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) => node;
        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => node;
        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node) => node;

    }
}
