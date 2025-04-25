using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

public class DiagnosticBuilder
{
    private readonly ImmutableArray<DiagnosticInfo>.Builder _builder = ImmutableArray.CreateBuilder<DiagnosticInfo>();

    public void Report(DiagnosticDescriptor descriptor, Location location, params object?[] args)
    {
        Diagnostic.Create(descriptor, location, args);
        _builder.Add(new DiagnosticInfo(descriptor, LocationInfo.Create(location), args));
    }

    public ImmutableArray<DiagnosticInfo> Build() => _builder.ToImmutable();
}