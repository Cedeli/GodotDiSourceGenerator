using GodotDiSourceGenerator.Attributes;
using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

[Generator]
public class GodotDiGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var transientProvider = ServiceProviderFactory.CreateProvider(context,
            typeof(TransientServiceAttribute).FullName!, Lifetime.Transient);
        var scopedProvider = ServiceProviderFactory.CreateProvider(context,
            typeof(ScopedServiceAttribute).FullName!, Lifetime.Scoped);
        var singletonProvider = ServiceProviderFactory.CreateProvider(context,
            typeof(SingletonServiceAttribute).FullName!, Lifetime.Singleton);

        var services = new[]
        {
            transientProvider,
            scopedProvider,
            singletonProvider
        }.MergeProviders();

        var results = services.Select((item, _) =>
        {
            var diagnostic = new DiagnosticBuilder();
            var constructor = ConstructorSelector.SelectConstructor(item.Symbol, diagnostic);
            var descriptor = constructor is not null
                ? new ServiceDescriptor(item.Symbol.ToDisplayString(),
                    (item.Attr.ConstructorArguments[0].Value as INamedTypeSymbol)!.ToDisplayString(), item.Life,
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

        var scopeRootProvider =
            ServiceProviderFactory.CreateProvider(context, typeof(ScopeRootAttribute).FullName!).Collect();

        context.RegisterSourceOutput(scopeRootProvider, static (ctx, arr) =>
        {
            var symbols = arr.Distinct(SymbolEqualityComparer.Default).ToList();
            Emitter.EmitScope(ctx, symbols);
        });

        context.RegisterSourceOutput(descriptors, static (ctx, arr) =>
        {
            var descriptors = arr.Distinct().ToList();
            Emitter.EmitRegistry(ctx, descriptors);
        });
    }
}