using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace UnrealSharp.SourceGenerators;

public static class Constants
{
    public const string UObject = nameof(UObject);
    public const string AActor = nameof(AActor);
    public const string UMetaData = nameof(UMetaData);
    public const string UFunction = nameof(UFunction);
    public const string UInterface = nameof(UInterface);
    public const string UClass = nameof(UClass);
    public const string UStruct = nameof(UStruct);
    public const string UEnum = nameof(UEnum);
    public const string UProperty = nameof(UProperty);
    public const string NativeClass = nameof(NativeClass);
    public const string NativeFunction = nameof(NativeFunction);
    public const string NativeDelegate = nameof(NativeDelegate);
    public const string NativeEnum = nameof(NativeEnum);
    public const string NativeStruct = nameof(NativeStruct);
    public const string NativeProperty = nameof(NativeProperty);
    public const string NativeField = nameof(NativeField);
    public const string NativeType = nameof(NativeType);
    public const string NativeMethod = nameof(NativeMethod);
    public const string NativeCallback = nameof(NativeCallback);
    public const string NativeEvent = nameof(NativeEvent);
    public const string NativeConstructor = nameof(NativeConstructor);
    public const string NativeDestructor = nameof(NativeDestructor);
    public const string NativeOperator = nameof(NativeOperator);
    public const string NativeConversion = nameof(NativeConversion);
    public const string NativeCast = nameof(NativeCast);
    public const string Binding = nameof(Binding);
    public const string GeneratedType = nameof(GeneratedType);
    
}

public static class Attributes
{
    public static AttributeSyntax UFunction => Attribute(IdentifierName(Constants.UFunction));
    public static AttributeSyntax UMetaData => Attribute(IdentifierName(Constants.UMetaData));
    public const string UInterface = "UInterfaceAttribute";
    public const string UClass = "UClassAttribute";
    public const string UStruct = "UStructAttribute";
    public const string UEnum = "UEnumAttribute";
    public const string UProperty = "UPropertyAttribute";
    public const string Binding = "BindingAttribute";
    public const string GeneratedType = "GeneratedTypeAttribute";
    public const string NativeClass = "NativeClassAttribute";
    public const string NativeFunction = "NativeFunctionAttribute";
    public const string NativeDelegate = "NativeDelegateAttribute";
    public const string NativeEnum = "NativeEnumAttribute";
    public const string NativeStruct = "NativeStructAttribute";
    public const string NativeProperty = "NativePropertyAttribute";
    public const string NativeField = "NativeFieldAttribute";
    public const string NativeType = "NativeTypeAttribute";
    public const string NativeMethod = "NativeMethodAttribute";
    public const string NativeCallback = "NativeCallbackAttribute";
    public const string NativeEvent = "NativeEventAttribute";
    public const string NativeConstructor = "NativeConstructorAttribute";
    public const string NativeDestructor = "NativeDestructorAttribute";
    public const string NativeOperator = "NativeOperatorAttribute";
    public const string NativeConversion = "NativeConversionAttribute";
    public const string NativeCast = "NativeCastAttribute";
}
