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
    }
}
