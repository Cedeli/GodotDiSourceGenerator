using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

public static class SourceGenerationHelper
{
    public const string Attributes = """
                                     using System;

                                     namespace GodotDiSourceGenerator
                                     {
                                            [AttributeUsage(AttributeTargets.Class)]
                                            public sealed class TransientServiceAttribute : Attribute
                                            {
                                                public TransientServiceAttribute(Type serviceType) => ServiceType = serviceType;
                                                public Type ServiceType { get; }
                                            }
                                         
                                            [AttributeUsage(AttributeTargets.Class)]
                                            public sealed class ScopedServiceAttribute : Attribute
                                            {
                                                public ScopedServiceAttribute(Type serviceType) => ServiceType = serviceType;
                                                public Type ServiceType { get; }
                                            }
                                     
                                            [AttributeUsage(AttributeTargets.Class)]
                                            public sealed class SingletonServiceAttribute : Attribute
                                            {
                                                public SingletonServiceAttribute(Type serviceType) => ServiceType = serviceType;
                                                public Type ServiceType { get; }
                                            }
                                         
                                            [AttributeUsage(AttributeTargets.Constructor)]
                                            public sealed class ConstructorAttribute : Attribute { }
                                         
                                            [AttributeUsage(AttributeTargets.Class)]
                                            public sealed class ScopeRootAttribute : Attribute { }
                                     }
                                     """;
    
    public static IncrementalValuesProvider<T> MergeProviders<T>(this IEnumerable<IncrementalValuesProvider<T>> sources)
    {
        var collected = sources.Select(p => p.Collect()).ToArray();

        var combined = collected.Aggregate((a, b) =>
            a.Combine(b).Select((pair, ct) => pair.Left.Concat(pair.Right).ToImmutableArray()));

        return combined.SelectMany((items, ct) => items);
    }
}