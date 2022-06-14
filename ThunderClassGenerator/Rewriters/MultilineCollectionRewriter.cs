using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            return base.VisitObjectCreationExpression(node.WithType(node.Type.WithoutTrivia()));
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            return node
                .WithoutTrivia()
                .WithOpenBraceToken(SF.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(fieldIndentation.Insert(0, SF.LineFeed)))
                .WithCloseBraceToken(SF.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(fieldIndentation.Insert(0, SF.LineFeed)))
                .WithExpressions(SF.SeparatedList(node.Expressions.Select(e => e.WithoutTrivia().WithLeadingTrivia(fieldIndentation.Insert(0, SF.LineFeed).Add(SF.Tab)))));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) => node;
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node) => node;
        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) => node;
        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => node;
        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node) => node;

    }
}
