using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

public sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    LocationInfo? Location
);