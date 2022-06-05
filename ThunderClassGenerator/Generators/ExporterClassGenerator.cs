using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Generators
{
    public class ExporterClassGenerator
    {
        public static SyntaxTree GetOrCreateTree(SimpleTypeDef typeDef)
        {
            var filePath = Path.Combine(Strings.SolutionFolder, Path.Combine(GeneratorUtilities.GetNamespaceString(typeDef).Split('.')), typeDef.Name, $"{typeDef.VersionnedName}_ExportYAML.cs");
            //if (File.Exists(filePath))
            //{
            //    return CSharpSyntaxTree.ParseText(File.ReadAllText(filePath), new CSharpParseOptions(LangVersion), filePath);
            //}
            return CreateSyntaxTree(typeDef, filePath);
        }

        private static SyntaxTree CreateSyntaxTree(SimpleTypeDef typeDef, string filePath)
        {
            var root = SF.CompilationUnit(default, GeneratorUtilities.GetUsings(typeDef), default, GeneratorUtilities.GetNamespaceMember(typeDef, GetMethods(typeDef)));
            return CSharpSyntaxTree.Create(root, new CSharpParseOptions(GeneratorUtilities.LangVersion), filePath);
        }

        private static SyntaxList<MemberDeclarationSyntax> GetMethods(SimpleTypeDef typeDef)
        {
            return SF.List(new[] { GetExportMethod(typeDef) });
        }

        private static MemberDeclarationSyntax GetExportMethod(SimpleTypeDef typeDef)
        {
            var modifiers = SF.TokenList(new[] { SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword) });
            var returnType = SF.ParseTypeName("YAMLNode");
            var name = SF.Identifier("ExportYAML");
            var body = GerReadMethodBody(typeDef);
            return SF.MethodDeclaration(default, modifiers, returnType, default, name, default, SF.ParameterList(), default, body, null);
        }

        private static BlockSyntax GerReadMethodBody(SimpleTypeDef typeDef)
        {
            var statements = new List<StatementSyntax>();
            //Calling var node = base.ExportYAML()
            statements.Add(SF.LocalDeclarationStatement(SF.VariableDeclaration(SF.ParseTypeName("var"), SF.SeparatedList(new[] { SF.VariableDeclarator(SF.Identifier("node"), default, SF.EqualsValueClause(SF.BinaryExpression(SyntaxKind.AsExpression, SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.BaseExpression(), SF.IdentifierName("ExportYAML"))), SF.ParseTypeName("YAMLMappingNode")))) }))));
            
            foreach (var field in typeDef.Fields)
            {
                if (field.ExistsInBase)
                {
                    continue;
                }

                statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("node"), SF.IdentifierName("Add")), SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(field.Name))), SF.Argument(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.ThisExpression(), SF.IdentifierName(GeneratorUtilities.GetValidFieldName(field.Name))), SF.IdentifierName("ExportYAML")), SF.ArgumentList())) })))));
            }

            statements.Add(SF.ReturnStatement(SF.IdentifierName("node")));

            return SF.Block(SF.List(statements));
        }
    }
}
