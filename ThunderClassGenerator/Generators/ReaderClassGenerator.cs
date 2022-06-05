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
    public class ReaderClassGenerator
    {
        public static SyntaxTree GetOrCreateTree(SimpleTypeDef typeDef)
        {
            var filePath = Path.Combine(Strings.SolutionFolder, Path.Combine(GeneratorUtilities.GetNamespaceString(typeDef).Split('.')), typeDef.Name, $"{typeDef.VersionnedName}_ReadBinary.cs");
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
            return SF.List(new[] { GetReadMethod(typeDef) });
        }

        private static MemberDeclarationSyntax GetReadMethod(SimpleTypeDef typeDef)
        {
            var modifiers = SF.TokenList(new[] { SF.Token(SyntaxKind.PublicKeyword), SF.Token(SyntaxKind.OverrideKeyword) });
            var returnType = SF.ParseTypeName("void");
            var name = SF.Identifier("ReadBinary");
            var parameterList = SF.ParameterList(SF.SeparatedList(new[] { SF.Parameter(default, default, SF.ParseTypeName("SerializedReader"), SF.Identifier("reader"), default) }));
            var body = GetReadMethodBody(typeDef);
            return SF.MethodDeclaration(default, modifiers, returnType, default, name, default, parameterList, default, body, null);
        }

        private static BlockSyntax GetReadMethodBody(SimpleTypeDef typeDef)
        {
            var statements = new List<StatementSyntax>();
            //Calling base.ReadBinary()
            statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.BaseExpression(), SF.IdentifierName("ReadBinary")), SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.IdentifierName("reader"))})))));
            
            foreach (var field in typeDef.Fields)
            {
                if (field.ExistsInBase)
                {
                    continue;
                }
                statements.Add(SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.ThisExpression(), SF.IdentifierName(GeneratorUtilities.GetValidFieldName(field.Name))), SF.ObjectCreationExpression(SF.ParseName(GeneratorUtilities.GetFullFieldTypeName(field.Type)), SF.ArgumentList(), default))));
            }

            foreach (var field in typeDef.Fields)
            {
                if (field.ExistsInBase)
                {
                    continue;
                }

                statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.ThisExpression(), SF.IdentifierName(GeneratorUtilities.GetValidFieldName(field.Name))), SF.IdentifierName("ReadBinary")), SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.IdentifierName("reader")) })))));
                if ((field.Type.MetaFlags & (int)MetaFlag.AlignBytesFlag) != 0)
                {
                    statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("reader"), SF.IdentifierName("Align")))));
                }
            }

            return SF.Block(SF.List(statements));
        }
    }
    /*public bool IsSupported_data(UnityVersion version, int assetVersion)
        {
            if (version > new UnityVersion(2000, 0, 0) && assetVersion == 1)
            {
                return true;
            }
            return false;
        }

        public int GetOrder_data(UnityVersion version, int assetVersion)
        {
            if (version > new UnityVersion(2000, 0, 0) && assetVersion == 1)
            {
                return 0;
            }
            return -1;
        }

        public int GetVersion_data(UnityVersion version, int assetVersion)
        {
            if (version > new UnityVersion(2000, 0, 0) && assetVersion == 1)
            {
                return 2;
            }
            return 1;
        }

        public void ReadBinary_data(SerializedReader reader, UnityVersion version, int assetVersion)
        {
            data = new AssetList<UIntWrapper>();
            data.ReadBinary(reader, version, GetVersion_data(version, assetVersion));
        }*/
}
