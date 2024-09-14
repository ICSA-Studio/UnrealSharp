
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;


namespace UnrealSharp.SourceGenerators.Extensions
{
    public static class SymbolExtensions
    {
        internal static bool HasAttribute(this ISymbol symbol, string? name = null)
        {
            var attributes = symbol.GetAttributes();
            return !attributes.IsEmpty && name == null || attributes.Any(x => x.AttributeClass?.Name == name);
        }

        internal static bool InheritsFrom(this ITypeSymbol symbol, string attributeName, string parameterName)
        {
            return symbol.GetAttributes().Any(x => x.AttributeClass?.Name == attributeName && x.NamedArguments.Any(y => y.Key == parameterName));
        }

        internal static bool InheritsFrom(this INamedTypeSymbol symbol, string baseTypeName)
        {
            INamedTypeSymbol? current = symbol;

            while (current is not null)
            {
                if (current.Name == baseTypeName)
                {
                    return true;
                }
                current = current.BaseType;
            }
            return false;
        }
    }
}