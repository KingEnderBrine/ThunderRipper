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

namespace ThunderClassGenerator.Utilities
{
    public static class IfDirectiveUtilities
    {
        public static UnityVersionRange[] GetDirectiveVersions(IfDirectiveTriviaSyntax ifDirective)
        {
            var ranges = new List<UnityVersionRange>();
            var current = ifDirective.Condition;
            while (current is BinaryExpressionSyntax binaryExpression)
            {
                if (binaryExpression.Right is ParenthesizedExpressionSyntax parenthesizedExpression)
                {
                    var versionsExpression = parenthesizedExpression.Expression as BinaryExpressionSyntax;
                    var minVersion = GetVersionFromIdentifier(versionsExpression.Left as IdentifierNameSyntax);
                    var maxVersion = GetVersionFromIdentifier((versionsExpression.Right as PrefixUnaryExpressionSyntax).Operand as IdentifierNameSyntax);
                    ranges.Add(new UnityVersionRange(minVersion, maxVersion));
                }
                else if (binaryExpression.Right is IdentifierNameSyntax identifier)
                {
                    ranges.Add(new UnityVersionRange(GetVersionFromIdentifier(identifier), default));
                }
                else if (binaryExpression.Right is PrefixUnaryExpressionSyntax prefixUnary)
                {
                    ranges.Add(new UnityVersionRange(GetVersionFromIdentifier(prefixUnary.Operand as IdentifierNameSyntax), default));
                }
                current = binaryExpression.Left;
            }

            return ranges.ToArray();
        }

        public static UnityVersionRange[] RecalculateRanges(UnityVersionRange[] ranges, UnityVersion version, IEnumerable<UnityVersion> supportedVersions, bool existsInVersion)
        {

        }

        public static IfDirectiveTriviaSyntax GetIfDirectiveFromVersionRanges(UnityVersionRange[] ranges)
        {
            ExpressionSyntax condition = SF.IdentifierName("CG");
            foreach (var range in ranges.OrderBy(r => r))
            {
                condition = SF.BinaryExpression(SyntaxKind.LogicalOrExpression, condition, GetNodeForVersionRange(range));
            }

            return SF.IfDirectiveTrivia(condition, true, true, true);
        }

        private static ExpressionSyntax GetNodeForVersionRange(UnityVersionRange range)
        {
            var min = range.HasMin ? SF.IdentifierName(range.min.ToDirectiveString()) : null;
            var max = range.HasMax ? SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SF.IdentifierName(range.max.ToDirectiveString())) : null;

            if (min != null && max != null)
            {
                return SF.ParenthesizedExpression(SF.BinaryExpression(SyntaxKind.LogicalAndExpression, min, max));
            }

            if (min != null)
            {
                return min;
            }

            if (max != null)
            {
                return max;
            }

            throw new NotSupportedException($"\"{nameof(range)}\" can't be default");
        }

        private static UnityVersion GetVersionFromIdentifier(IdentifierNameSyntax identifier)
        {
            return new UnityVersion(identifier.Identifier.ValueText);
        }
    }
}
