using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotDiSourceGenerator;

[Generator]
public class DiRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("ServiceAttributes.g.cs", SourceGenerationHelper.Attributes));

        var serviceTypes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) =>
                    s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                    var model = ctx.SemanticModel;
                    var symbol = model.GetDeclaredSymbol(classDeclaration);

                    if (symbol == null) return null;
                    var attributes = symbol.GetAttributes();
                    foreach (var attribute in attributes)
                    {
                        var attributeName = attribute.AttributeClass?.Name;
                        switch (attributeName)
                        {
                            case null:
                                continue;
                            case "TransientServiceAttribute":
                            case "SingletonServiceAttribute":
                            {
                                var serviceType = (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!;
                                return new ServiceDescriptor(symbol.ToDisplayString(),
                                    serviceType.ToDisplayString(), attributeName.StartsWith("Singleton"));
                            }
                        }
                    }

                    return null;
                })
            .Where(descriptor => descriptor is not null);

        context.RegisterSourceOutput(serviceTypes.Collect(), static (ctx, list) =>
        {
            var descriptors = list.Distinct().ToList();
            if (descriptors.Count == 0) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("using Godot;");
            sb.AppendLine("");
            sb.AppendLine("public partial class InjectionContainer");
            sb.AppendLine("{");
            sb.AppendLine("     partial void RegisterGeneratedServices()");
            sb.AppendLine("     {");
            foreach (var d in descriptors.OfType<ServiceDescriptor>())
            {
                sb.AppendLine(d.IsSingleton
                    ? $"        RegisterSingleton<{d.Interface}, {d.Implementation}>();"
                    : $"        RegisterTransient<{d.Interface}, {d.Implementation}>();");
            }
            sb.AppendLine("     }");
            sb.AppendLine("}");
            
            ctx.AddSource("DiRegistration.g.cs", sb.ToString());
        });
    }
}