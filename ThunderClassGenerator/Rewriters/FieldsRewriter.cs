using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderClassGenerator.Generators;
using ThunderRipperShared.Utilities;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Rewriters
{
    public class FieldsRewriter : CSharpSyntaxRewriter
    {
        private readonly SimpleTypeDef typeDef;
        private readonly UnityVersion unityVersion;

        public FieldsRewriter(SimpleTypeDef typeDef, UnityVersion unityVersion)
        {
            this.typeDef = typeDef;
            this.unityVersion = unityVersion;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var existingFields = node.Members.OfType<FieldDeclarationSyntax>().ToArray();
            var fieldsToAdd = typeDef.Fields.Where(f => !f.ExistsInBase && !existingFields.Any(ef => GeneratorUtilities.GetValidFieldName(f.Name) == ef.Declaration.Variables[0].Identifier.Text && f.Type.FullName == ef.Declaration.Type.ToString()));
            return base.VisitClassDeclaration(node.WithMembers(node.Members.AddRange(fieldsToAdd.Select(GetFieldDeclaration))));
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            if (node.Parent is FieldDeclarationSyntax)
            {

            }
            return base.VisitIfDirectiveTrivia(node);
        }

        private FieldDeclarationSyntax GetFieldDeclaration(FieldDef field)
        {
            return SF.FieldDeclaration(
                    default,
                    SF.TokenList(SF.Token(SyntaxKind.PublicKeyword)),
                    SF.VariableDeclaration(
                        SF.ParseTypeName(field.Type.FullName),
                        SF.SeparatedList(new[] { SF.VariableDeclarator(GeneratorUtilities.GetValidFieldName(field.Name)) })))
                .NormalizeWhitespace()
                .WithLeadingTrivia(SF.TriviaList(SF.Tab, SF.Tab))
                .WithTrailingTrivia(SF.LineFeed);
            //SF.Tab, SF.Tab, SF.Trivia(SF.IfDirectiveTrivia(SF.BinaryExpression(SyntaxKind.LogicalOrExpression, SF.IdentifierName("CG"), SF.IdentifierName("")), true, true, true)), SF.LineFeed
        }
    }
}
