using System;

namespace GodotDiSourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TransientServiceAttribute(Type serviceType) : Attribute
{
    public Type ServiceType { get; } = serviceType;
}