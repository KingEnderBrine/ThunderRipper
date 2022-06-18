using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRipperShared.Utilities;

namespace ThunderClassGenerator.Utilities
{
    public static class SupportedVersionsUtilities
    {
        public static UnityVersion[] GetSupportedVersionsFromRoot(SyntaxNode root)
        {
            var field = root.DescendantNodes().OfType<FieldDeclarationSyntax>().FirstOrDefault(f => f.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText == "SupportedVersions");
            if (field == null)
            {
                return Array.Empty<UnityVersion>();
            }

            return field.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().Select(GetVersionFromCreationExpression).OrderBy(v => v).ToArray();
        }

        public static UnityVersion GetVersionFromCreationExpression(ObjectCreationExpressionSyntax expression)
        {
            var firstArgument = expression.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (firstArgument is not LiteralExpressionSyntax literal || !literal.Token.IsKind(SyntaxKind.StringLiteralToken))
            {
                return default;
            }

            return new UnityVersion(literal.Token.ValueText);
        }
    }
}
