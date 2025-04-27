using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotDiSourceGenerator;

internal static class ServiceProviderFactory
{
    internal static IncrementalValuesProvider<(INamedTypeSymbol Symbol, AttributeData Attr, Lifetime Life)>
        CreateProvider(IncrementalGeneratorInitializationContext context, string attributeName, Lifetime life)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(attributeName,
            (node, _) => node is ClassDeclarationSyntax,
            (ctx, _) => (Symbol: (INamedTypeSymbol)ctx.TargetSymbol!, Attr: ctx.Attributes[0], Life: life));
    }

    internal static IncrementalValuesProvider<INamedTypeSymbol> CreateProvider(
        IncrementalGeneratorInitializationContext context, string attributeName)
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(attributeName,
            (node, _) => node is ClassDeclarationSyntax,
            (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol);
    }
}