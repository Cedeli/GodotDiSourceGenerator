using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

internal static class Rules
{
    public static readonly DiagnosticDescriptor NoPublicConstructor = new(
        id: "DI1001",
        title: "No public constructor",
        messageFormat: "Type '{0}' must have at least one public constructor for dependency injection",
        category: "DependencyInjection",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor AmbiguousConstructor = new(
        id: "DI1002",
        title: "Ambiguous constructor",
        messageFormat:
        "Type '{0}' has multiple constructors with {1} parameters; consider annotating one with [Constructor]",
        category: "DependencyInjection",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
}