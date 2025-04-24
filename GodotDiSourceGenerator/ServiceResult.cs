using System.Collections.Immutable;

namespace GodotDiSourceGenerator;

public sealed record ServiceResult(
    ServiceDescriptor? Descriptor,
    ImmutableArray<DiagnosticInfo> Diagnostics
);