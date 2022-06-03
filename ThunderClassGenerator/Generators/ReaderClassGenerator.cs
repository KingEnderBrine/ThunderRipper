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
            var root = SF.CompilationUnit(default, GeneratorUtilities.GetUsings(typeDef), default, GeneratorUtilities.GetNamespaceMember(typeDef, GetMembers(typeDef)));
            return CSharpSyntaxTree.Create(root, new CSharpParseOptions(GeneratorUtilities.LangVersion), filePath);
        }

        private static SyntaxList<MemberDeclarationSyntax> GetMembers(SimpleTypeDef typeDef)
        {
            return SF.List(GetReadAction(typeDef).Concat(GetReadMethods(typeDef)));
        }

        private static MemberDeclarationSyntax[] GetReadAction(SimpleTypeDef typeDef)
        {
            return new MemberDeclarationSyntax[]
            {
                SF.FieldDeclaration(
                    default,
                    SF.TokenList(SF.Token(SyntaxKind.PrivateKeyword), SF.Token(SyntaxKind.StaticKeyword), SF.Token(SyntaxKind.ReadOnlyKeyword)),
                    SF.VariableDeclaration(
                        SF.ParseTypeName("Action<AssetBase, SerializedReader>"),
                        SF.SeparatedList(new[]
                        {
                            SF.VariableDeclarator(
                                SF.Identifier("readBinaryAction"),
                                default,
                                SF.EqualsValueClause(
                                    SF.InvocationExpression(
                                        SF.IdentifierName("CompileReadBinary"),
                                        SF.ArgumentList(
                                            SF.SeparatedList(new[]
                                            {
                                                SF.Argument(
                                                    SF.InvocationExpression(
                                                        SF.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SF.InvocationExpression(
                                                                SF.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SF.InvocationExpression(
                                                                        SF.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SF.ArrayCreationExpression(
                                                                                SF.ArrayType(SF.ParseTypeName("(Expression<Action<AssetBase, SerializedReader>>, int)[]")),
                                                                                SF.InitializerExpression(
                                                                                    SyntaxKind.ArrayInitializerExpression,
                                                                                    SF.SeparatedList(
                                                                                        typeDef.Fields.Values
                                                                                            .Where(f => !f.ExistsInBase)
                                                                                            .Select(f => (ExpressionSyntax)SF.TupleExpression(
                                                                                                SF.SeparatedList(new []
                                                                                                {
                                                                                                    SF.Argument(
                                                                                                        SF.ParenthesizedLambdaExpression(
                                                                                                            SF.ParameterList(SF.SeparatedList(new[] 
                                                                                                            {
                                                                                                                SF.Parameter(SF.Identifier("asset")),
                                                                                                                SF.Parameter(SF.Identifier("reader"))
                                                                                                            })),
                                                                                                            default,
                                                                                                            SF.InvocationExpression(SF.IdentifierName("ReadBinary_" + GeneratorUtilities.GetValidFieldName(f.Name)), SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.BinaryExpression(SyntaxKind.AsExpression, SF.IdentifierName("asset"), SF.ParseTypeName(typeDef.VersionnedName))), SF.Argument(SF.IdentifierName("reader"))}))))),
                                                                                                    SF.Argument(SF.IdentifierName("_order_" + GeneratorUtilities.GetValidFieldName(f.Name)))
                                                                                                })))))),
                                                                            SF.IdentifierName("OrderBy")),
                                                                        SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.SimpleLambdaExpression(SF.Parameter(SF.Identifier("i")), default, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("i"), SF.IdentifierName("Item2"))))}))),
                                                                    SF.IdentifierName("Select")),
                                                                SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.SimpleLambdaExpression(SF.Parameter(SF.Identifier("i")), default, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("i"), SF.IdentifierName("Item1")))) }))),
                                                            SF.IdentifierName("ToArray"))))
                                            })))))
                        })
                    )),
                SF.PropertyDeclaration(
                    default,
                    SF.TokenList(SF.Token(SyntaxKind.ProtectedKeyword), SF.Token(SyntaxKind.OverrideKeyword)),
                    SF.ParseTypeName("Action<AssetBase, SerializedReader>"),
                    default,
                    SF.Identifier("ReadBinaryAction"),
                    default,
                    SF.ArrowExpressionClause(SF.BinaryExpression(SyntaxKind.AddExpression, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.BaseExpression(), SF.IdentifierName("ReadBinaryAction")), SF.IdentifierName("readBinaryAction"))),
                    default,
                    SF.Token(SyntaxKind.SemicolonToken))
            };
        }
        
        //private static readonly Action<AssetBase, SerializedReader> readBinaryAction = CompileReadBinary(new (Expression<Action<AssetBase, SerializedReader>>, int)[] { ((a, r) => ReadTest(a, r), 1) }.OrderBy(e => e.Item2).Select(e => e.Item1).ToArray());

        private static MemberDeclarationSyntax[] GetReadMethods(SimpleTypeDef typeDef)
        {
            return typeDef.Fields.Values.Where(f => !f.ExistsInBase).Select(f => GetReadMethod(typeDef, f)).ToArray();
        }

        private static MemberDeclarationSyntax GetReadMethod(SimpleTypeDef typeDef, FieldDef fieldDef)
        {
            var modifiers = SF.TokenList(new[] { SF.Token(SyntaxKind.ProtectedKeyword), SF.Token(SyntaxKind.StaticKeyword) });
            var returnType = SF.ParseTypeName("void");
            var name = SF.Identifier("ReadBinary_" + GeneratorUtilities.GetValidFieldName(fieldDef.Name));
            var parameterList = SF.ParameterList(SF.SeparatedList(new[] { SF.Parameter(default, default, SF.ParseTypeName(typeDef.VersionnedName), SF.Identifier("asset"), default), SF.Parameter(default, default, SF.ParseTypeName("SerializedReader"), SF.Identifier("reader"), default) }));
            var body = GetReadMethodBody(fieldDef);
            return SF.MethodDeclaration(default, modifiers, returnType, default, name, default, parameterList, default, body, null);
        }

        private static BlockSyntax GetReadMethodBody(FieldDef fieldDef)
        {
            var statements = new List<StatementSyntax>();
            statements.Add(SF.ExpressionStatement(SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("asset"), SF.IdentifierName(GeneratorUtilities.GetValidFieldName(fieldDef.Name))), SF.ObjectCreationExpression(SF.ParseName(GeneratorUtilities.GetFullFieldTypeName(fieldDef.Type)), SF.ArgumentList(), default))));
            statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("asset"), SF.IdentifierName(GeneratorUtilities.GetValidFieldName(fieldDef.Name))), SF.IdentifierName("ReadBinary")), SF.ArgumentList(SF.SeparatedList(new[] { SF.Argument(SF.IdentifierName("reader")) })))));
            if ((fieldDef.Type.MetaFlags & (int)MetaFlag.AlignBytesFlag) != 0)
            {
                statements.Add(SF.ExpressionStatement(SF.InvocationExpression(SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName("reader"), SF.IdentifierName("Align")))));
            }

            return SF.Block(SF.List(statements));
        }
    }
}
