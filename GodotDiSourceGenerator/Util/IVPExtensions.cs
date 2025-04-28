using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

public static class IvpExtensions
{
    public static IncrementalValuesProvider<T> MergeProviders<T>(this IEnumerable<IncrementalValuesProvider<T>> sources)
    {
        var collected = sources.Select(p => p.Collect()).ToArray();

        var combined = collected.Aggregate((a, b) =>
            a.Combine(b).Select((pair, ct) => pair.Left.Concat(pair.Right).ToImmutableArray()));

        return combined.SelectMany((items, ct) => items);
    }
}