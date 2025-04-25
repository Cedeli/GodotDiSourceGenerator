namespace GodotDiSourceGenerator;

public record ServiceDescriptor(
    string Implementation,
    string Interface,
    bool IsSingleton,
    IReadOnlyList<string> ParameterTypes
);