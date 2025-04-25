namespace GodotDiSourceGenerator;

public record ServiceDescriptor(
    string Implementation,
    string Interface,
    Lifetime Lifetime,
    IReadOnlyList<string> ParameterTypes
);