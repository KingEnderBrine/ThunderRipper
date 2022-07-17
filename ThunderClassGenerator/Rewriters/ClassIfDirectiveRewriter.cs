using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderClassGenerator.Utilities;
using ThunderRipperShared.Utilities;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Rewriters
{
    public class ClassIfDirectiveRewriter : CSharpSyntaxRewriter
    {
        private readonly UnityVersion version;
        private readonly IEnumerable<UnityVersion> supportedVersions;
        private readonly bool existsInVersion;
        private readonly bool isNew;

        public ClassIfDirectiveRewriter(UnityVersion version, IEnumerable<UnityVersion> supportedVersions, bool existsInVersion, bool isNew)
        {
            this.version = version;
            this.supportedVersions = supportedVersions;
            this.existsInVersion = existsInVersion;
            this.isNew = isNew;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var ifTrivia = node.GetLeadingTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.IfDirectiveTrivia));
            var ifDirective = ifTrivia.GetStructure() as IfDirectiveTriviaSyntax;
            var ranges = IfDirectiveUtilities.GetDirectiveVersions(ifDirective);
            ranges = IfDirectiveUtilities.RecalculateRanges(ranges, version, supportedVersions, existsInVersion, isNew);
            if (ranges.Length > 0)
            {
                if (ifDirective != null)
                {
                    node = node.ReplaceTrivia(ifTrivia, SF.Trivia(IfDirectiveUtilities.GetIfDirectiveFromVersionRanges(ranges)));
                }
                else
                {
                    node = node
                        .WithLeadingTrivia(SF.Trivia(IfDirectiveUtilities.GetIfDirectiveFromVersionRanges(ranges)))
                        .WithTrailingTrivia(SF.Trivia(SF.EndIfDirectiveTrivia(true)), SF.LineFeed);
                }
            }
            return node;
        }
    }
}
