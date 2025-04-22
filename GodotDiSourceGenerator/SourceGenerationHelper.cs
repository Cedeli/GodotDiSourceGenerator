namespace GodotDiSourceGenerator;

public class SourceGenerationHelper
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
                                      public sealed class SingletonServiceAttribute : Attribute
                                      {
                                          public SingletonServiceAttribute(Type serviceType) => ServiceType = serviceType;
                                          public Type ServiceType { get; }
                                      }
                                  }
                                  """;
}