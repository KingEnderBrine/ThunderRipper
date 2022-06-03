using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Generators
{
    public static class GeneratorUtilities
    {
        public static LanguageVersion LangVersion => LanguageVersion.CSharp7;
        public static string[] Keywords { get; } = new[]
        {
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while",
        };

        public static string GetValidFieldName(string fieldName)
        {
            var startIsCorrect = Regex.IsMatch(fieldName, @"^[\p{L}\p{Nl}_]");
            var replacement = Regex.Replace(fieldName, @"[^\p{Cf}\p{L}\p{Mc}\p{Mn}\p{Nd}\p{Nl}\p{Pc}]", "_", RegexOptions.Compiled);

            replacement = startIsCorrect && !Keywords.Contains(replacement) ? replacement : $"_{replacement}";
            return replacement;
        }

        public static string GetNamespaceString(SimpleTypeDef typeDef)
        {
            return string.IsNullOrWhiteSpace(typeDef.Namespace) ? Strings.OutputNamespace : typeDef is PredefinedTypeDef ? typeDef.Namespace : $"{Strings.OutputNamespace}.{typeDef.Namespace}";
        }

        public static string GetFullFieldTypeName(TypeUsageDef usageDef)
        {
            if (usageDef.GenericIndex != -1)
            {
                return $"T{usageDef.GenericIndex + 1}";
            }
            if (usageDef.Type.GenericCount == 0)
            {
                return usageDef.Type.VersionnedName;
            }

            return $"{usageDef.Type.VersionnedName}<{string.Join(", ", usageDef.GenericArgs.Select(el => GetFullFieldTypeName(el)))}>";
        }

        public static AttributeListSyntax CreateSimpleAttribute(string name, IEnumerable<AttributeArgumentSyntax> args = null)
        {
            args ??= Array.Empty<AttributeArgumentSyntax>();
            return SF.AttributeList(SF.SeparatedList(
                new[]
                {
                        SF.Attribute(
                            SF.IdentifierName(name),
                            SF.AttributeArgumentList(SF.SeparatedList(args)))
                }));
        }

        public static SyntaxList<MemberDeclarationSyntax> GetNamespaceMember(SimpleTypeDef typeDef, SyntaxList<MemberDeclarationSyntax> methods)
        {
            var @class = SF.ClassDeclaration(default, GetClassModifiers(typeDef), SF.Identifier(typeDef.VersionnedName), GetTypeParameters(typeDef), default, default, methods);
            var @namespace = SF.NamespaceDeclaration(SF.IdentifierName(GetNamespaceString(typeDef)), default, default, SF.List(new MemberDeclarationSyntax[] { @class }));
            var comment = SF.Comment(Strings.CreatedWithComment);
            return SF.List(new MemberDeclarationSyntax[] { @namespace.WithLeadingTrivia(comment) });
        }

        public static SyntaxTokenList GetClassModifiers(SimpleTypeDef typeDef)
        {
            return SF.TokenList(GetTokens());

            IEnumerable<SyntaxToken> GetTokens()
            {
                yield return SF.Token(SyntaxKind.PublicKeyword);
                if (typeDef.IsAbstract)
                {
                    yield return SF.Token(SyntaxKind.AbstractKeyword);
                }
                //Partial must always be right before class
                yield return SF.Token(SyntaxKind.PartialKeyword);
            }
        }

        public static TypeParameterListSyntax GetTypeParameters(SimpleTypeDef typeDef)
        {
            if (typeDef.GenericCount == 0)
            {
                return default;
            }

            var types = new List<TypeParameterSyntax>();
            for (var i = 0; i < typeDef.GenericCount; i++)
            {
                types.Add(SF.TypeParameter($"T{i + 1}"));
            }

            return SF.TypeParameterList(SF.SeparatedList(types));
        }
        public static SyntaxList<UsingDirectiveSyntax> GetUsings(SimpleTypeDef typeDef)
        {
            var usings = new HashSet<string>
            {
                Strings.CollectionsGeneric,
                Strings.ThunderRipperAttributes,
                Strings.ThunderRipperAssets,
                Strings.ThunderRipperUtilities,
                Strings.ThunderRipperYAML,
                Strings.ThunderRipperYAMLExtensions,
                "System",
                "System.Linq.Expressions",
                "System.Linq",
            };


            if (typeDef.BaseType != null && !string.IsNullOrWhiteSpace(typeDef.BaseType.Namespace) && !typeDef.Namespace.StartsWith(typeDef.BaseType.Namespace))
            {
                usings.Add(GetNamespaceString(typeDef.BaseType));
            }

            foreach (var field in typeDef.Fields.Values)
            {
                if (field.ExistsInBase)
                {
                    continue;
                }
                GoOverGenericArgs(field.Type);

                void GoOverGenericArgs(TypeUsageDef usageDef)
                {
                    if (usageDef.GenericIndex != -1)
                    {
                        return;
                    }
                    usings.Add(GetNamespaceString(usageDef.Type));
                    foreach (var genericArg in usageDef.GenericArgs)
                    {
                        GoOverGenericArgs(genericArg);
                    }
                }
            }

            return SF.List(usings.OrderBy(el => el).Select(el => SF.UsingDirective(SF.IdentifierName(el))));
        }
    }
}
