
using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace UnrealSharp.SourceGenerators.Extensions
{
    public static class FactoryExtensions
    {
        public static AttributeSyntax Attribute(string name)
        {
            return SyntaxFactory.Attribute(IdentifierName(name));
        }
        
    }
}