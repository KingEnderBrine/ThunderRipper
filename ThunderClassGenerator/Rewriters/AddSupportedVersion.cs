using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Rewriters
{
    public class AddSupportedVersion : CSharpSyntaxRewriter
    {
        private readonly string version;
        public AddSupportedVersion(string version)
        {
            this.version = version;
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            if (node.Parent is not ArrayCreationExpressionSyntax)
            {
                return base.VisitInitializerExpression(node);
            }

            return node.AddExpressions(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(version)));
        }
    }
}
