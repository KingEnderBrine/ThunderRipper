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
            if (typeDef.IsStruct)
            {
                return base.VisitClassDeclaration(node);
            }

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (!typeDef.IsStruct)
            {
                return base.VisitStructDeclaration(node);
            }

            return base.VisitStructDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return base.VisitPropertyDeclaration(node);
        }



        public static string NormalizePropertyName(UnityNode node)
        {
            var startIsCorrect = Regex.IsMatch(node.Name, @"^[\p{L}\p{Nl}_]");
            var replacement = Regex.Replace(node.Name, @"[^\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]", "_", RegexOptions.Compiled);

            return startIsCorrect ? replacement : $"_{replacement}";
        }
    }
}
