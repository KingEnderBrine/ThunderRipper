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
    public class ClassIfDirectiveRewriter : CSharpSyntaxRewriter
    {
        private readonly UnityVersion unityVersion;
        private readonly IEnumerable<UnityVersion> supportedVersions;
        private readonly SimpleTypeDef typeDef;

        public ClassIfDirectiveRewriter(UnityVersion unityVersion, IEnumerable<UnityVersion> supportedVersions, SimpleTypeDef typeDef)
        {
            this.unityVersion = unityVersion;
            this.supportedVersions = supportedVersions;
            this.typeDef = typeDef;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var ifDirectives = node.DescendantTrivia(n => false, true).Where(t => t.IsKind(SyntaxKind.IfDirectiveTrivia)).FirstOrDefault();
            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            return base.VisitIfDirectiveTrivia(node);
        }
    }
}
