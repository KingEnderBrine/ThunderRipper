using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThunderClassGenerator.Rewriters
{
    public class AddFields : CSharpSyntaxRewriter
    {
        private readonly SimpleTypeDef typeDef;

        public AddFields(SimpleTypeDef typeDef)
        {
            this.typeDef = typeDef;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return base.VisitPropertyDeclaration(node);
        }
    }
}
