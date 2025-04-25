using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotDiSourceGenerator;

[Generator]
public class RegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("ServiceAttributes.g.cs", SourceGenerationHelper.Attributes));

        var transientProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "GodotDiSourceGenerator.TransientServiceAttribute",
            (n, _) => n is ClassDeclarationSyntax,
            (ctx, _) => (
                Symbol: (INamedTypeSymbol)ctx.TargetSymbol,
                Attr: ctx.Attributes[0],
                IsSingleton: false));

        var singletonProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "GodotDiSourceGenerator.SingletonServiceAttribute",
            (n, _) => n is ClassDeclarationSyntax,
            (ctx, _) => (
                Symbol: (INamedTypeSymbol)ctx.TargetSymbol,
                Attr: ctx.Attributes[0],
                IsSingleton: true));

        var services = transientProvider.Collect().Combine(singletonProvider.Collect())
            .SelectMany((pair, _) => pair.Left.Concat(pair.Right));

        var results = services.Select((item, _) =>
        {
            var diagnostic = new DiagnosticBuilder();
            var constructor = ConstructorSelector.SelectConstructor(item.Symbol, diagnostic);
            var descriptor = constructor is not null
                ? new ServiceDescriptor(item.Symbol.ToDisplayString(),
                    (item.Attr.ConstructorArguments[0].Value as INamedTypeSymbol)!.ToDisplayString(), item.IsSingleton,
                    constructor.Parameters.Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                        .ToList())
                : null;

            return (Descriptor: descriptor, Diagnostics: diagnostic.Build());
        });

        var diagnostics = results
            .SelectMany((result, _) => result.Diagnostics)
            .Collect();
        
        context.RegisterSourceOutput(diagnostics, (ctx, arr) =>
        {
            foreach (var info in arr)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(info.Descriptor, info.Location?.ToLocation(), info.Arguments));
            }
        });

        var descriptors = results
            .Select((result, _) => result.Descriptor)
            .Where(d => d is not null)
            .Collect();

        context.RegisterSourceOutput(descriptors, static (ctx, arr) =>
        {
            var descriptors = arr.Distinct().ToList();
            Emitter.Emit(ctx, descriptors);
        });
    }
}