using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Tags;

using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Editing;

namespace UnrealSharp.SourceGenerators;

internal readonly record struct AsyncMethodInfo(ClassDeclarationSyntax ParentClass, MethodDeclarationSyntax Method, string Namespace, TypeSyntax ReturnType, IReadOnlyDictionary<string, string> Metadata)
{
    public readonly ClassDeclarationSyntax ParentClass = ParentClass;
    public readonly MethodDeclarationSyntax Method = Method;
    public readonly string Namespace = Namespace;
    public readonly TypeSyntax ReturnType = ReturnType;
    public readonly IReadOnlyDictionary<string, string> Metadata = Metadata;


}

[Generator()]
public class AsyncWrapperGenerator : ISourceGenerator
{
    private static SyntaxList<UsingDirectiveSyntax> UESharpUsings => List(new[]
    {
        UsingDirective(IdentifierName("UnrealSharp")),
        UsingDirective(IdentifierName("UnrealSharp.Attributes")),
        UsingDirective(IdentifierName("UnrealSharp.CSharpForUE"))
    });
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each compilation
        context.RegisterForSyntaxNotifications(() => new AsyncSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var token = context.CancellationToken;
        token.ThrowIfCancellationRequested();
        var CancellationTokenType = context.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken") ?? throw new InvalidOperationException("Cannot find CancellationToken type");
        
        if (context.SyntaxReceiver is AsyncSyntaxReceiver receiver && receiver.AsyncMethods.Count > 0)
        {
            var compilation = context.Compilation;
            foreach (var info in receiver.AsyncMethods)
            {
                var model = compilation.GetSemanticModel(info.Method.SyntaxTree);
                
                var unit = CompilationUnit().WithUsings(UESharpUsings);
                // HashSet<string> namespaces = ["UnrealSharp", "UnrealSharp.Attributes", "UnrealSharp.CSharpForUE"];
                ParameterSyntax cancellationTokenParameter;
                foreach (ParameterSyntax parameter in info.Method.ParameterList.Parameters)
                {
                    if (parameter.Type is TypeSyntax typeSyntax)
                    {
                        var typeInfo = model.GetTypeInfo(parameter.Type, token);
                        var typeSymbol = typeInfo.Type;
                        if (SymbolEqualityComparer.Default.Equals(typeSymbol, CancellationTokenType))
                        {
                            cancellationTokenParameter = parameter;
                        }
                        if (typeSymbol == null || typeSymbol.ContainingNamespace == null)
                        {
                            continue;
                        }
                        namespaces.Add(typeSymbol.ContainingNamespace.ToDisplayString());
                    }
                }
                foreach (var ns in namespaces)
                {
                    sourceBuilder.AppendLine($"using {ns};");
                }
                sourceBuilder.AppendLine();
                sourceBuilder.AppendLine($"namespace {info.Namespace}");
                sourceBuilder.AppendLine("{");
                var isStatic = info.Method.Modifiers.Any(static x => x.IsKind(SyntaxKind.StaticKeyword));
                var returnTypeName = info.ReturnType != null ? model.GetTypeInfo(info.ReturnType).Type.Name : null;
                var actionClassName = $"{info.ParentClass.Identifier.Text}{info.Method.Identifier.Text}Action";
                var actionBaseClassName = cancellationTokenParameter != null ? "UCSCancellableAsyncAction" : "UCSBlueprintAsyncActionBase";
                var delegateName = $"{actionClassName}Delegate";
                var taskTypeName = info.ReturnType != null ? $"Task<{returnTypeName}>" : "Task";
                var paramNameList = string.Join(", ", info.Method.ParameterList.Parameters.Select(p => p == cancellationTokenParameter ? "cancellationToken" : p.Identifier.Text));
                var paramDeclListNoCancellationToken = string.Join(", ", info.Method.ParameterList.Parameters.Where(p => p != cancellationTokenParameter));
                var metadataAttributeList = string.Join(", ", info.Metadata.Select(static pair => $"UMetaData({pair.Key}, {pair.Value})"));
                if (string.IsNullOrEmpty(metadataAttributeList))
                {
                    metadataAttributeList = "UMetaData(\"BlueprintInternalUseOnly\", \"true\")";
                }
                else
                {
                    metadataAttributeList = $"UMetaData(\"BlueprintInternalUseOnly\", \"true\"), {metadataAttributeList}";
                }
                if (!isStatic)
                {
                    metadataAttributeList = $"UM

        }
        foreach (var asyncMethodInfo in receiver.AsyncMethods)
        {
            var model = compilation.GetSemanticModel(asyncMethodInfo.Method.SyntaxTree);
            var sourceBuilder = new StringBuilder();

            HashSet<string> namespaces = ["UnrealSharp", "UnrealSharp.Attributes", "UnrealSharp.CSharpForUE"];

            ParameterSyntax cancellationTokenParameter;
            
            foreach (ParameterSyntax parameter in asyncMethodInfo.Method.ParameterList.Parameters)
            {
                if (parameter.Type is TypeSyntax typeSyntax)
                {
                    
                }
                var typeInfo = model.GetTypeInfo(parameter.Type, token);
                var typeSymbol = typeInfo.Type;

                if (SymbolEqualityComparer.Default.Equals(typeSymbol, cancellationTokenType))
                {
                    cancellationTokenParameter = parameter;
                }

                if (typeSymbol == null || typeSymbol.ContainingNamespace == null)
                {
                    continue;
                }

                namespaces.Add(typeSymbol.ContainingNamespace.ToDisplayString());
            }

            foreach (var ns in namespaces)
            {
                sourceBuilder.AppendLine($"using {ns};");
            }

            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"namespace {asyncMethodInfo.Namespace};");
            model.GetTypeInfo(asyncMethodInfo.Method, token).Type.ContainingNamespace.Name
            var isStatic = asyncMethodInfo.Method.Modifiers.Any(static x => x.IsKind(SyntaxKind.StaticKeyword));

            var returnTypeName = asyncMethodInfo.ReturnType != null ? model.GetTypeInfo(asyncMethodInfo.ReturnType).Type.Name : null;
            var actionClassName = $"{asyncMethodInfo.ParentClass.Identifier.Text}{asyncMethodInfo.Method.Identifier.Text}Action";
            var actionBaseClassName = cancellationTokenParameter != null ? "UCSCancellableAsyncAction" : "UCSBlueprintAsyncActionBase";
            var delegateName = $"{actionClassName}Delegate";
            var taskTypeName = asyncMethodInfo.ReturnType != null ? $"Task<{returnTypeName}>" : "Task";
            var paramNameList = string.Join(", ", asyncMethodInfo.Method.ParameterList.Parameters.Select(p => p == cancellationTokenParameter ? "cancellationToken" : p.Identifier.Text));
            var paramDeclListNoCancellationToken = string.Join(", ", asyncMethodInfo.Method.ParameterList.Parameters.Where(p => p != cancellationTokenParameter));

            var metadataAttributeList = string.Join(", ", asyncMethodInfo.Metadata.Select(static pair => $"UMetaData({pair.Key}, {pair.Value})"));
            if (string.IsNullOrEmpty(metadataAttributeList))
            {
                metadataAttributeList = "UMetaData(\"BlueprintInternalUseOnly\", \"true\")";
            }
            else
            {
                metadataAttributeList = $"UMetaData(\"BlueprintInternalUseOnly\", \"true\"), {metadataAttributeList}";
            }
            if (!isStatic)
            {
                metadataAttributeList = $"UMetaData(\"DefaultToSelf\", \"Target\"), {metadataAttributeList}";
            }

            sourceBuilder.AppendLine();
            
            if (asyncMethodInfo.ReturnType != null)
            {
                sourceBuilder.AppendLine($"public delegate void {delegateName}({returnTypeName} Result, string Exception);");
            }
            else
            {
                sourceBuilder.AppendLine($"public delegate void {delegateName}(string Exception);");
            }

            sourceBuilder.AppendLine();

            sourceBuilder.AppendLine($"public class U{delegateName} : MulticastDelegate<{delegateName}>");
            sourceBuilder.AppendLine($"{{");
            
            if (asyncMethodInfo.ReturnType != null)
            {
                sourceBuilder.AppendLine($"    protected void Invoker({returnTypeName} Result, string Exception)");
                sourceBuilder.AppendLine($"    {{");
                sourceBuilder.AppendLine($"        ProcessDelegate(IntPtr.Zero);");
                sourceBuilder.AppendLine($"    }}");
            }
            else
            {
                sourceBuilder.AppendLine($"    protected void Invoker(string Exception)");
                sourceBuilder.AppendLine($"    {{");
                sourceBuilder.AppendLine($"        ProcessDelegate(IntPtr.Zero);");
                sourceBuilder.AppendLine($"    }}");
            }
            
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"    protected override {delegateName} GetInvoker()");
            sourceBuilder.AppendLine($"    {{");
            sourceBuilder.AppendLine($"        return Invoker;");
            sourceBuilder.AppendLine($"    }}");
            sourceBuilder.AppendLine($"}}");
            sourceBuilder.AppendLine();
            
            sourceBuilder.AppendLine($"[UClass]");
            sourceBuilder.AppendLine($"public class {actionClassName} : {actionBaseClassName}");
            sourceBuilder.AppendLine($"{{");
            sourceBuilder.AppendLine($"    private {taskTypeName}? task;");
            
            if (cancellationTokenParameter != null)
            {
                sourceBuilder.AppendLine($"    private readonly CancellationTokenSource cancellationTokenSource = new();");
                sourceBuilder.AppendLine($"    private Func<CancellationToken, {taskTypeName}>? asyncDelegate;");
            }
            else
            {
                sourceBuilder.AppendLine($"    private Func<{taskTypeName}>? asyncDelegate;");
            }
            sourceBuilder.AppendLine($"");
            sourceBuilder.AppendLine($"    [UProperty(PropertyFlags.BlueprintAssignable)]");
            sourceBuilder.AppendLine($"    public TMulticastDelegate<{delegateName}> Completed {{ get; set; }}");
            sourceBuilder.AppendLine($"");
            sourceBuilder.AppendLine($"    [UProperty(PropertyFlags.BlueprintAssignable)]");
            sourceBuilder.AppendLine($"    public TMulticastDelegate<{delegateName}> Failed {{ get; set; }}");
            sourceBuilder.AppendLine($"");
            sourceBuilder.AppendLine($"    [UFunction(FunctionFlags.BlueprintCallable), {metadataAttributeList}]");
            if (isStatic)
            {
                sourceBuilder.AppendLine($"    public static {actionClassName} {asyncMethodInfo.Method.Identifier.Text}({paramDeclListNoCancellationToken})");
                sourceBuilder.AppendLine($"    {{");
                sourceBuilder.AppendLine($"        var action = NewObject<{actionClassName}>(GetTransientPackage());");
                if (cancellationTokenParameter != null)
                {
                    sourceBuilder.AppendLine($"        action.asyncDelegate = (cancellationToken) => {asyncMethodInfo.ParentClass.Identifier.Text}.{asyncMethodInfo.Method.Identifier.Text}({paramNameList});");
                }
                else
                {
                    sourceBuilder.AppendLine($"        action.asyncDelegate = () => {asyncMethodInfo.ParentClass.Identifier.Text}.{asyncMethodInfo.Method.Identifier.Text}({paramNameList});");
                }
                sourceBuilder.AppendLine($"        return action;");
                sourceBuilder.AppendLine($"    }}");
            }
            else
            {
                if (string.IsNullOrEmpty(paramDeclListNoCancellationToken))
                {
                    sourceBuilder.AppendLine($"    public static {actionClassName} {asyncMethodInfo.Method.Identifier.Text}({asyncMethodInfo.ParentClass.Identifier.Text} Target)");
                }
                else
                {
                    sourceBuilder.AppendLine($"    public static {actionClassName} {asyncMethodInfo.Method.Identifier.Text}({asyncMethodInfo.ParentClass.Identifier.Text} Target, {paramDeclListNoCancellationToken})");
                }
                sourceBuilder.AppendLine($"    {{");
                sourceBuilder.AppendLine($"        var action = NewObject<{actionClassName}>(Target);");
                if (cancellationTokenParameter != null)
                {
                    sourceBuilder.AppendLine($"        action.asyncDelegate = (cancellationToken) => Target.{asyncMethodInfo.Method.Identifier.Text}({paramNameList});");
                }
                else
                {
                    sourceBuilder.AppendLine($"        action.asyncDelegate = () => Target.{asyncMethodInfo.Method.Identifier.Text}({paramNameList});");
                }
                sourceBuilder.AppendLine($"        return action;");
                sourceBuilder.AppendLine($"    }}");
            }
            

            sourceBuilder.AppendLine($"");
            sourceBuilder.AppendLine($"    protected override void Activate()");
            sourceBuilder.AppendLine($"    {{");
            sourceBuilder.AppendLine($"        if (asyncDelegate == null) {{ throw new InvalidOperationException($\"AsyncDelegate was null\"); }}");
            if (cancellationTokenParameter != null)
            {
                sourceBuilder.AppendLine($"        task = asyncDelegate(cancellationTokenSource.Token);");
            }
            else
            {
                sourceBuilder.AppendLine($"        task = asyncDelegate();");
            }
            sourceBuilder.AppendLine($"        task.ContinueWith(OnTaskCompleted);");
            sourceBuilder.AppendLine($"    }}");
            if (cancellationTokenParameter != null)
            {
                sourceBuilder.AppendLine($"");
                sourceBuilder.AppendLine($"    protected override void Cancel()");
                sourceBuilder.AppendLine($"    {{");
                sourceBuilder.AppendLine($"        cancellationTokenSource.Cancel();");
                sourceBuilder.AppendLine($"        base.Cancel();");
                sourceBuilder.AppendLine($"    }}");
            }
            sourceBuilder.AppendLine($"");
            sourceBuilder.AppendLine($"    private void OnTaskCompleted({taskTypeName} t)");
            sourceBuilder.AppendLine($"    {{");
            // sourceBuilder.AppendLine($"        if (!IsDestroyed) {{ PrintString($\"OnTaskCompleted for {{this}} on {{UnrealSynchronizationContext.CurrentThread}}\"); }}");
            sourceBuilder.AppendLine($"        if (UnrealSynchronizationContext.CurrentThread != NamedThread.GameThread)");
            sourceBuilder.AppendLine($"        {{");
            sourceBuilder.AppendLine($"            UnrealSynchronizationContext.GetContext(NamedThread.GameThread).Post(_ => OnTaskCompleted(t), null);");
            sourceBuilder.AppendLine($"            return;");
            sourceBuilder.AppendLine($"        }}");
            if (cancellationTokenParameter != null)
            {
                sourceBuilder.AppendLine($"        if (cancellationTokenSource.IsCancellationRequested || IsDestroyed) {{ return; }}");
            }
            else
            {
                sourceBuilder.AppendLine($"        if (IsDestroyed) {{ return; }}");
            }
            sourceBuilder.AppendLine($"        if (t.IsFaulted)");
            sourceBuilder.AppendLine($"        {{");
            if (asyncMethodInfo.ReturnType != null)
            {
                sourceBuilder.AppendLine($"            Failed.InnerDelegate.Invoke(default, t.Exception?.ToString() ?? \"Faulted without exception\");");
            }
            else
            {
                sourceBuilder.AppendLine($"            Failed.InnerDelegate.Invoke(t.Exception?.ToString() ?? \"Faulted without exception\");");
            }
            sourceBuilder.AppendLine($"        }}");
            sourceBuilder.AppendLine($"        else");
            sourceBuilder.AppendLine($"        {{");
            if (asyncMethodInfo.ReturnType != null)
            {
                sourceBuilder.AppendLine($"            Completed.InnerDelegate.Invoke(t.Result, null);");
            }
            else
            {
                sourceBuilder.AppendLine($"            Completed.InnerDelegate.Invoke(null);");
            }
            sourceBuilder.AppendLine($"        }}");
            sourceBuilder.AppendLine($"    }}");
            sourceBuilder.AppendLine($"}}");

            context.AddSource($"{asyncMethodInfo.ParentClass.Identifier.Text}.{asyncMethodInfo.Method.Identifier.Text}.generated.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}

internal class AsyncSyntaxReceiver : ISyntaxContextReceiver
{
    public List<AsyncMethodInfo> AsyncMethods { get; } = [];

    /// <inheritdoc cref="ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode)" />
    public void OnVisitSyntaxNode(SyntaxNode node)
    {
        if (node is AttributeSyntax attribute)
        {
            if (attribute.Name.ToString() == Constants.UFunction)
            {
                var method = (MethodDeclarationSyntax)attribute.Parent;
                ProcessMethod(method);
            }

        }
        if (node is MethodDeclarationSyntax method) {
            ProcessMethod(method);
        } else if (node is ClassDeclarationSyntax @class) {
            foreach (var member in @class.Members.OfType<MethodDeclarationSyntax>())
            {
                var hasUFunctionAttribute = member.AttributeLists.Any(a => a.Attributes.Any(Constants.UFunction.IsEquivalentTo));
                ProcessMethod(member);
            }
        }

        // var walker = node.SyntaxTree.GetCompilationUnitRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();

        string namespaceName = string.Empty;
        SyntaxNode? currentNode = member.Parent;
        while (currentNode != null)
        {
            // Check if the current node is a NamespaceDeclarationSyntax
            if (currentNode is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration)
            {
                // Get the name of the namespace
                namespaceName = fileScopedNamespaceDeclaration.Name.ToString();
                break;
            }
            if (currentNode is NamespaceDeclarationSyntax namespaceDeclaration)
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

        var metadataAttributes = methodDeclaration.AttributeLists
            .SelectMany(a => a.Attributes)
            .Where(a => a.Name.ToString() == Constants.UMetaData);

        Dictionary<string, string> metadata = [];
        foreach (var metadataAttribute in metadataAttributes)
        {
            var key = metadataAttribute.ArgumentList.Arguments[0].Expression.ToString();
            var value = metadataAttribute.ArgumentList.Arguments.Count > 1 ? metadataAttribute.ArgumentList.Arguments[1].Expression.ToString() : "";
            metadata.Add(key, value);
        }

        if (methodDeclaration.ReturnType is IdentifierNameSyntax identifierName && identifierName.Identifier.ValueText == "Task")
        {
            // Method returns non-generic task, e.g. without return value
            AsyncMethods.Add(new(classDeclaration, methodDeclaration, namespaceName, null, metadata));
            return;
        }

        if (methodDeclaration.ReturnType is GenericNameSyntax genericName && genericName.Identifier.ValueText == "Task")
        {
            // Method returns generic task, e.g. with return value
            var taskReturnType = genericName.TypeArgumentList.Arguments.Single();
            AsyncMethods.Add(new(classDeclaration, methodDeclaration, namespaceName, taskReturnType, metadata));
            return;
        }
    }

    /// <inheritdoc cref="ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext)" />
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        // SyntaxTreeOptionsProvider optionsProvider =
        // context.SemanticModel.Compilation.GetCompilationNamespace(context.Node.)
        throw new System.NotImplementedException();
    }

    private void ProcessMethod(MethodDeclarationSyntax method)
    {
        if (method.Modifiers.Contains(Token(SyntaxKind.AsyncKeyword)))
        {
            
        }
    }
}



}
public static class MethodSyntaxExtensions
{
    public static bool IsAsyncMethod(this MethodDeclarationSyntax method)
    {
        return method.Modifiers.Any(SyntaxKind.AsyncKeyword);
    }
    public static TypeSyntax GetReturnType(this MethodDeclarationSyntax method)
    {
        var modifiers = method.ReturnType;
        return method.ReturnType;
    }

}