using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UnrealSharp.SourceGenerators.Extensions;

namespace UnrealSharp.SourceGenerators;

[Generator]
public class NativeCallbacksWrapperGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each compilation
        context.RegisterForSyntaxNotifications(() => new NativeCallbacksSyntaxContextReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not NativeCallbacksSyntaxReceiver receiver)
            return;

        var compilation = context.Compilation;
        
        foreach (var classInfo in receiver.Candidates)
        {
            var model = compilation.GetSemanticModel(classInfo.ClassDeclaration.SyntaxTree);
            var sourceBuilder = new StringBuilder();

            List<INamespaceSymbol> namespaces = [];
            foreach (var delegateInfo in classInfo.Delegates)
            {
                foreach (var parameter in delegateInfo.Parameters)
                {
                    var typeInfo = model.GetTypeInfo(parameter.Type);
                    var typeSymbol = typeInfo.Type;
                    
                    if (typeSymbol == null || typeSymbol.ContainingNamespace == null)
                    {
                        continue;
                    }
                    
                    if (namespaces.Contains(typeSymbol.ContainingNamespace))
                    {
                        continue;
                    }
                    
                    namespaces.Add(typeSymbol.ContainingNamespace);
                }
            }
            
            foreach(var ns in namespaces)
            {
                sourceBuilder.AppendLine($"using {ns.ToDisplayString()};");
            }
            
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"namespace {classInfo.Namespace}");
            sourceBuilder.AppendLine("{");
            sourceBuilder.AppendLine($"    public static unsafe partial class {classInfo.Name}");
            sourceBuilder.AppendLine("    {");

            foreach (var delegateInfo in classInfo.Delegates)
            {
                int lastIndex = delegateInfo.Parameters.Count - 1;
                DelegateParameterInfo returnValueType = delegateInfo.Parameters[lastIndex];
                
                // Remove return value. We don't need it anymore.
                delegateInfo.Parameters.RemoveAt(lastIndex);
                
                sourceBuilder.Append($"        public static {returnValueType.Type} Call{delegateInfo.Name}(");

                // Handle parameters
                bool firstParameter = true;
                char paramName = 'a';

                foreach (var parameter in delegateInfo.Parameters)
                {
                    if (!firstParameter)
                    {
                        sourceBuilder.Append(", ");
                    }

                    firstParameter = false;

                    if (parameter.IsOutParameter)
                    {
                        sourceBuilder.Append("out ");
                    }
                    
                    if (parameter.IsRefParameter)
                    {
                        sourceBuilder.Append("ref ");
                    }
                    
                    string typeFullName = model.GetTypeInfo(parameter.Type).Type.ToDisplayString();
                    sourceBuilder.Append($"{typeFullName} {paramName}");
                    paramName++;
                }

                sourceBuilder.AppendLine(")");
                sourceBuilder.AppendLine("        {");

                string delegateName = $"{classInfo.Name}.{delegateInfo.Name}";
                
                sourceBuilder.AppendLine($"             if ({delegateName} == null)");
                sourceBuilder.AppendLine("             {");
                sourceBuilder.AppendLine($"                 throw new System.InvalidOperationException(\"{delegateName} is null\");");
                sourceBuilder.AppendLine("             }");
                sourceBuilder.AppendLine();

                // Method body
                if (returnValueType.Type.ToString() != "void")
                {
                    sourceBuilder.Append($"            return {delegateName}(");
                }
                else
                {
                    sourceBuilder.Append($"            {delegateName}(");
                }

                // Handling method parameters
                firstParameter = true;
                paramName = 'a';
                foreach (var parameter in delegateInfo.Parameters)
                {
                    if (!firstParameter)
                    {
                        sourceBuilder.Append(", ");
                    }

                    firstParameter = false;

                    if (parameter.IsOutParameter)
                    {
                        sourceBuilder.Append("out ");
                    }
                    
                    if (parameter.IsRefParameter)
                    {
                        sourceBuilder.Append("ref ");
                    }
                    
                    sourceBuilder.Append(paramName);
                    paramName++;
                }

                sourceBuilder.AppendLine(");");
                sourceBuilder.AppendLine("        }");
            }
            
            // End class definition
            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");

            context.AddSource($"{classInfo.Name}.generated.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}

internal class NativeCallbacksSyntaxContextReceiver : ISyntaxContextReceiver
{
    public List<INamedTypeSymbol> Candidates { get; } = [];

    // public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    // {
    //     if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
    //     {
    //         return;
    //     }
        
    //     var hasNativeCallbacksAttribute = classDeclaration.AttributeLists
    //         .SelectMany(a => a.Attributes)
    //         .Any(a => a.Name.ToString() == "NativeCallbacks");

    //     if (!hasNativeCallbacksAttribute)
    //     {
    //         return;
    //     }
            
    //     string namespaceName = null;
    //     SyntaxNode? currentNode = classDeclaration.Parent;
    //     while (currentNode != null)
    //     {
    //         // Check if the current node is a NamespaceDeclarationSyntax
    //         if (currentNode is FileScopedNamespaceDeclarationSyntax namespaceDeclaration)
    //         {
    //             // Get the name of the namespace
    //             namespaceName = namespaceDeclaration.Name.ToString();
    //             break;
    //         }

    //         // Move up to the next parent node
    //         currentNode = currentNode.Parent;
    //     }

    //     if (namespaceName == null)
    //     {
    //         return;
    //     }
                
    //     var classInfo = new ClassInfo
    //     {
    //         ClassDeclaration = classDeclaration,
    //         Name = classDeclaration.Identifier.ValueText,
    //         Namespace = namespaceName,
    //         Delegates = []
    //     };

    //     // Find all delegate members in the class
    //     foreach (var member in classDeclaration.Members)
    //     {
    //         if (member is not FieldDeclarationSyntax fieldDeclaration ||
    //             fieldDeclaration.Declaration.Type is not FunctionPointerTypeSyntax functionPointerTypeSyntax)
    //         {
    //             continue;
    //         }
            
    //         var delegateInfo = new DelegateInfo
    //         {
    //             Name = fieldDeclaration.Declaration.Variables.First().Identifier.ValueText,
    //             Parameters = []
    //         };

    //         // Iterate through each parameter in the function pointer type syntax
    //         foreach (var param in functionPointerTypeSyntax.ParameterList.Parameters)
    //         {
    //             var parameter = new DelegateParameterInfo
    //             {
    //                 // Check if the parameter has the 'out' modifier
    //                 IsOutParameter = param.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.OutKeyword)),
    //                 IsRefParameter = param.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.RefKeyword)),
    //                 Type = param.Type,
    //             };
                            
    //             delegateInfo.Parameters.Add(parameter);
    //         }

    //         classInfo.Delegates.Add(delegateInfo);
    //     }
                
    //     ClassesWithNativeCallbacks.Add(classInfo);
    // }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax declaration && declaration.AttributeLists.Count > 0)
        {
            var model = context.SemanticModel;
            if (model.GetDeclaredSymbol(declaration) is INamedTypeSymbol symbol && symbol.GetAttributes().Any(x => x.AttributeClass?.Name == Constants.NativeCallback))
            {
                Candidates.Add(symbol);
            }
        }
    }
}
internal class NativeCallbacksSyntaxReceiver : ISyntaxReceiver
{
    public List<INamedTypeSymbol> Candidates { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax declaration)
            return;

        string namespaceName = null;
        SyntaxNode? currentNode = declaration.Parent;
        while (currentNode != null)
        {
            // Check if the current node is a NamespaceDeclarationSyntax
            if (currentNode is FileScopedNamespaceDeclarationSyntax namespaceDeclaration)
            {
                // Get the name of the namespace
                namespaceName = namespaceDeclaration.Name.ToString();
                break;
            }

            // Move up to the next parent node
            currentNode = currentNode.Parent;
        }

        if (namespaceName == null)
        {
            return;
        }

        var classInfo = new ClassInfo
        {
            ClassDeclaration = declaration,
            Name = declaration.Identifier.ValueText,
            Namespace = namespaceName,
            Delegates = []
        };

        // Find all delegate members in the class
        foreach (var member in declaration.Members)
        {
            if (member is not FieldDeclarationSyntax fieldDeclaration ||
                fieldDeclaration.Declaration.Type is not FunctionPointerTypeSyntax functionPointerTypeSyntax)
            {
                continue;
            }

            var delegateInfo = new DelegateInfo
            {
                Name = fieldDeclaration.Declaration.Variables.First().Identifier.ValueText,
                Parameters = []
            };

            // Iterate through each parameter in the function pointer type syntax
            foreach (var param in functionPointerTypeSyntax.ParameterList.Parameters)
            {
                var parameter = new DelegateParameterInfo
                {
                    // Check if the parameter has the 'out' modifier
                    IsOutParameter = param.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.OutKeyword)),
                    IsRefParameter = param.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.RefKeyword)),
                    Type = param.Type,
                };

                delegateInfo.Parameters.Add(parameter);
            }

            classInfo.Delegates.Add(delegateInfo);
        }

        Candidates.Add(classInfo);
    }
}

internal struct ClassInfo
{
    public ClassDeclarationSyntax ClassDeclaration { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public List<DelegateInfo> Delegates { get; set; }
}

internal struct DelegateInfo
{
    public string Name { get; set; }
    public List<DelegateParameterInfo> Parameters { get; set; }
}

internal struct DelegateParameterInfo
{
    public TypeSyntax Type { get; set; }
    public bool IsOutParameter { get; set; }
    public bool IsRefParameter { get; set; }
}