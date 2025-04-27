using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

internal static class ConstructorSelector
{
    internal static IMethodSymbol? SelectConstructor(INamedTypeSymbol symbol, DiagnosticBuilder builder)
    {
        var constructors = symbol.Constructors
            .Where(c => c.DeclaredAccessibility != Accessibility.Private && !c.IsStatic)
            .ToImmutableArray();

        if (constructors.Length == 0)
        {
            builder.Report(
                Rules.NoPublicConstructor,
                symbol.Locations[0],
                symbol.Name);
            return null;
        }

        var marked = constructors
            .Where(c => c.GetAttributes().Any(a => a.AttributeClass?.Name == "InjectionConstructorAttribute"))
            .ToImmutableArray();

        if (marked.Length == 1) return marked[0];

        var max = constructors.Max(c => c.Parameters.Length);
        var greedy = constructors.Where(c => c.Parameters.Length == max).ToArray();

        if (greedy.Length == 1) return greedy[0];

        builder.Report(
            Rules.AmbiguousConstructor,
            symbol.Locations[0],
            symbol.Name,
            max);
        return null;
    }
}