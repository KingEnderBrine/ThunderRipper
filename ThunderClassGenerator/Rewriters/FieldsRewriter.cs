using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderClassGenerator.Extensions;
using ThunderClassGenerator.Generators;
using ThunderClassGenerator.Utilities;
using ThunderRipperShared.Utilities;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Rewriters
{
    public class FieldsRewriter : CSharpSyntaxRewriter
    {
        private readonly SimpleTypeDef type;
        private readonly UnityVersion version;
        private readonly IEnumerable<UnityVersion> supportedVersions;
        private readonly bool isNewType;

        public FieldsRewriter(SimpleTypeDef type, UnityVersion version, IEnumerable<UnityVersion> supportedVersions, bool isNewType)
        {
            this.type = type;
            this.version = version;
            this.supportedVersions = supportedVersions;
            this.isNewType = isNewType;
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var usings = node.Usings.Select(u => u.Name.ToString());
            var additionalUsings = type.Fields
                .SelectMany(f => Recursion.Simple(f.Type, (f) => f.GenericArgs))
                .Where(t => t.Type != null)
                .Select(t => GeneratorUtilities.GetNamespaceString(t.Type))
                .Distinct()
                .Where(n => !usings.Contains(n))
                .Select(u => SF.UsingDirective(SF.ParseName(u)))
                .ToArray();
            return base.VisitCompilationUnit(node.AddUsings(additionalUsings));
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var existingFields = node.Members.OfType<FieldDeclarationSyntax>().ToArray();
            var fields = type.Fields.Where(f => !f.ExistsInBase).ToArray();

            var updatedFields = existingFields.Where(ef => fields.Any(f => FieldIsEqualToNode(f, ef))).ToArray();
            var addedFields = fields.Where(f => !existingFields.Any(ef => FieldIsEqualToNode(f, ef))).Select(GetFieldDeclaration).ToArray();

            var firstFieldIndex = node.Members.IndexOf(SyntaxKind.FieldDeclaration);
            if (firstFieldIndex == -1)
            {
                firstFieldIndex = node.Members.Count;
            }

            return node.WithMembers(SF.List(node.Members.Select(m =>
            {
                if (m is not FieldDeclarationSyntax fieldNode || isNewType)
                {
                    return m;
                }

                var leadingTrivia = fieldNode.GetLeadingTrivia();
                var ifDirective = leadingTrivia.Where(t => t.IsKind(SyntaxKind.IfDirectiveTrivia)).FirstOrDefault().GetStructure() as IfDirectiveTriviaSyntax;

                var ranges = IfDirectiveUtilities.GetDirectiveVersions(ifDirective);
                ranges = IfDirectiveUtilities.RecalculateRanges(ranges, version, supportedVersions, updatedFields.Contains(fieldNode), false);
                if (ranges.Length > 0)
                {
                    fieldNode = fieldNode.WithLeadingTrivia(leadingTrivia.Where(t => !t.IsKind(SyntaxKind.IfDirectiveTrivia)).Append(SF.Trivia(IfDirectiveUtilities.GetIfDirectiveFromVersionRanges(ranges))));
                    if (ifDirective == null)
                    {
                        fieldNode = fieldNode.WithTrailingTrivia(new[] { SF.Trivia(SF.EndIfDirectiveTrivia(true)), SF.LineFeed }.Union(fieldNode.GetTrailingTrivia()));
                    }
                }

                return fieldNode;
            }).InsertRange(addedFields, firstFieldIndex)));
        }

        private FieldDeclarationSyntax GetFieldDeclaration(FieldDef field)
        {
            var fieldNode = SF.FieldDeclaration(
                    default,
                    SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)),
                    SF.VariableDeclaration(
                        SF.ParseTypeName(field.Type.FullName),
                        SF.SeparatedList(new[] { SF.VariableDeclarator(GeneratorUtilities.GetValidFieldName(field.Name)) })));
            
            if (isNewType)
            {
                return fieldNode;
            }

            var ranges = IfDirectiveUtilities.RecalculateRanges(Array.Empty<UnityVersionRange>(), version, supportedVersions, true, true);

            if (ranges.Length > 0)
            {
                fieldNode = fieldNode
                    .WithLeadingTrivia(SF.Trivia(IfDirectiveUtilities.GetIfDirectiveFromVersionRanges(ranges)))
                    .WithTrailingTrivia(SF.Trivia(SF.EndIfDirectiveTrivia(true)), SF.LineFeed);
            }

            return fieldNode;
        }

        private static bool FieldIsEqualToNode(FieldDef field, FieldDeclarationSyntax fieldNode)
        {
            return GeneratorUtilities.GetValidFieldName(field.Name) == fieldNode.Declaration.Variables[0].Identifier.Text && field.Type.FullName == fieldNode.Declaration.Type.ToString();
        }
    }
}
