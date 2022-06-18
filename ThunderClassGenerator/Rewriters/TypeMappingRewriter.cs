using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderClassGenerator.Utilities;
using ThunderRipperShared.Utilities;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Rewriters
{
    public class TypeMappingRewriter : CSharpSyntaxRewriter
    {
        private static readonly Regex versionRegex = new Regex(@".*?_V(?'version'\d+)", RegexOptions.Compiled);
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
                return base.VisitFieldDeclaration(node);
            }

            return node;
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            var existingMappings = node.Expressions.Select(e => GetMappingFromCreationExpression(e as AssignmentExpressionSyntax)).Where(m => m != default).ToArray();
            
            var updatedMappings = existingMappings.Join(types.Where(t => t.TypeID >= 0), m => (m.Item1, m.Item2), t => (t.TypeID, t.VersionnedName), (m, t) => (m.Item1, m.Item2, IfDirectiveUtilities.RecalculateRanges(m.Item3, version, supportedVersions, true, false))).ToArray();
            var addedMappings = types.Where(t => t.TypeID >= 0).Select(t => (t.TypeID, t.VersionnedName, Array.Empty<UnityVersionRange>())).ExceptBy(updatedMappings.Select(m => (m.Item1, m.Item2)), m => (m.Item1, m.Item2)).Select(m => (m.Item1, m.Item2, IfDirectiveUtilities.RecalculateRanges(m.Item3, version, supportedVersions, true, true))).ToArray();
            var notUpdatedMappings = existingMappings.ExceptBy(updatedMappings.Select(m => (m.Item1, m.Item2)), m => (m.Item1, m.Item2)).Select(m => (m.Item1, m.Item2, IfDirectiveUtilities.RecalculateRanges(m.Item3, version, supportedVersions, false, false))).ToArray();

            var expressions = new List<ExpressionSyntax>();
            var previousHadRanges = false;
            foreach (var mapping in updatedMappings.Union(notUpdatedMappings).Union(addedMappings).OrderBy(m => m.Item1).ThenBy(m => int.Parse(versionRegex.Match(m.Item2).Groups["version"].Value)))
            {
                expressions.Add(CreateRowExpression(mapping.Item1, mapping.Item2, mapping.Item3, previousHadRanges));
                previousHadRanges = mapping.Item3.Length > 0;
            }
            if (previousHadRanges)
            {
                node = node.WithCloseBraceToken(SF.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(SF.Trivia(SF.EndIfDirectiveTrivia(true)), SF.LineFeed));
            }

            node = node.WithExpressions(SF.SeparatedList(expressions, expressions.Select(e => SF.Token(SyntaxKind.CommaToken))));
            return node;
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
                                            SF.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(SF.LineFeed),
                                            SF.SeparatedList<ExpressionSyntax>(),
                                            SF.Token(SyntaxKind.CloseBraceToken)))))
                        })))
                .WithLeadingTrivia(SF.TriviaList(SF.Tab, SF.Tab))
                .WithTrailingTrivia(SF.LineFeed);
        }

        private static AssignmentExpressionSyntax CreateRowExpression(int typeID, string name, UnityVersionRange[] ranges, bool previousHadRanges)
        {
            var expression = SF.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SF.ImplicitElementAccess(SF.BracketedArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(typeID))) }))),
                    SF.TypeOfExpression(SF.ParseTypeName(name)));
            
            var leadingTrivia = new List<SyntaxTrivia>();
            if (previousHadRanges)
            {
                leadingTrivia.Add(SF.Trivia(SF.EndIfDirectiveTrivia(true)));
            }

            if (ranges.Length > 0)
            {
                leadingTrivia.Add(SF.Trivia(IfDirectiveUtilities.GetIfDirectiveFromVersionRanges(ranges)));
            }

            return expression.WithLeadingTrivia(SF.TriviaList(leadingTrivia)).NormalizeWhitespace();
        }

        private static (int, string, UnityVersionRange[]) GetMappingFromCreationExpression(AssignmentExpressionSyntax expression)
        {
            if (expression == null)
            {
                return default;
            }

            var ranges = Array.Empty<UnityVersionRange>();
            if (expression.GetLeadingTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.IfDirectiveTrivia)).GetStructure() is IfDirectiveTriviaSyntax ifTrivia)
            {
                ranges = IfDirectiveUtilities.GetDirectiveVersions(ifTrivia);
            }

            var leftFirstArgument = (expression.Left as ImplicitElementAccessSyntax)?.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (leftFirstArgument is not LiteralExpressionSyntax literal || !literal.Token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return default;
            }

            var typeName = (expression.Right as TypeOfExpressionSyntax)?.Type.ToString();
            if (typeName == null)
            {
                return default;
            }

            return ((int)literal.Token.Value, typeName, ranges);
        }
    }
}
