using System;

namespace GodotDiSourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ScopedServiceAttribute(Type serviceType) : Attribute
{
    public Type ServiceType { get; } = serviceType;
}