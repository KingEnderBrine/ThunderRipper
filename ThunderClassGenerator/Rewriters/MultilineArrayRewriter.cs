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
    public class MultilineArrayRewriter : CSharpSyntaxRewriter
    {
        private SyntaxTriviaList fieldIndentation;

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var trivia = node.GetLeadingTrivia();
            fieldIndentation = node.GetLeadingTrivia().Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia)).ToSyntaxTriviaList();

            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            return node
                .WithoutTrivia()
                .WithExpressions(SF.SeparatedList(node.Expressions.Select(e => e.WithoutTrivia().WithLeadingTrivia(fieldIndentation.Insert(0, SF.LineFeed).Add(SF.Tab)))))
                .WithOpenBraceToken(SF.Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(SF.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(fieldIndentation.Insert(0, SF.LineFeed)))
                .WithLeadingTrivia(fieldIndentation.Insert(0, SF.LineFeed));
        }
    }
}
