
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnrealSharp.SourceGenerators.Extensions
{
    public static class SyntaxExtensions
    {
        internal static bool HasAttribute<TMember>(this TMember member, string name) where TMember : MemberDeclarationSyntax
        {
            return member.AttributeLists.Any(x => x.Attributes.Any(y => y.Name.ToString().Contains(name)));
        }

    }


}