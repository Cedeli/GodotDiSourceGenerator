using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotDiSourceGenerator;

[Generator]
public class RegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("ServiceAttributes.g.cs", SourceGenerationHelper.Attributes));

        var results = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                    var model = ctx.SemanticModel;

                    var symbol = model.GetDeclaredSymbol(classDeclaration);

                    if (symbol == null) return new ServiceResult(null, ImmutableArray<DiagnosticInfo>.Empty);

                    var diagnosticBuilder = ImmutableArray.CreateBuilder<DiagnosticInfo>();
                    ServiceDescriptor? descriptor = null;

                    var attributes = symbol.GetAttributes();
                    foreach (var attribute in attributes)
                    {
                        var attributeName = attribute.AttributeClass?.Name;

                        if (attributeName is not ("TransientServiceAttribute" or "SingletonServiceAttribute")) continue;

                        var serviceType = (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!;

                        var constructors = symbol.Constructors
                            .Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic)
                            .ToImmutableArray();

                        if (constructors.Length <= 0)
                        {
                            var dd = new DiagnosticDescriptor(
                                "DI1001",
                                "No public constructors",
                                $"Type '{symbol.Name}' has no public constructors.",
                                "DependencyInjection",
                                DiagnosticSeverity.Error,
                                true
                            );

                            diagnosticBuilder.Add(new DiagnosticInfo(dd,
                                LocationInfo.Create(classDeclaration.GetLocation())));
                            continue;
                        }

                        var target = constructors
                            .Where(c => c.GetAttributes()
                                .Any(a => a.AttributeClass?.Name == "InjectionConstructorAttribute"))
                            .ToImmutableArray();

                        IMethodSymbol? selected = null;
                        if (target.Length == 1)
                        {
                            selected = target[0];
                        }
                        else
                        {
                            var maxParams = constructors.Max(c => c.Parameters.Length);
                            var greedy = constructors
                                .Where(c => c.Parameters.Length == maxParams)
                                .ToImmutableArray();
                            if (greedy.Length == 1)
                            {
                                selected = greedy[0];
                            }
                            else
                            {
                                var dd = new DiagnosticDescriptor(
                                    "DI1002",
                                    "Ambiguous constructor",
                                    $"Type '{symbol.Name}' has multiple constructors with {maxParams} parameters",
                                    "DependencyInjection",
                                    DiagnosticSeverity.Error,
                                    true
                                );
                                diagnosticBuilder.Add(new DiagnosticInfo(dd,
                                    LocationInfo.Create(classDeclaration.GetLocation())));
                            }
                        }

                        if (selected is null) return new ServiceResult(descriptor, diagnosticBuilder.ToImmutable());
                        var parameterTypes = selected.Parameters
                            .Select(p => p.Type
                                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                            .ToList();

                        descriptor = new ServiceDescriptor(symbol.ToDisplayString(),
                            serviceType.ToDisplayString(), attributeName.StartsWith("Singleton"),
                            parameterTypes);

                        return new ServiceResult(descriptor, diagnosticBuilder.ToImmutable());
                    }

                    return new ServiceResult(null, ImmutableArray<DiagnosticInfo>.Empty);
                })
            .Where(r => r.Descriptor is not null || r.Diagnostics.Any());

        var diagnostics = results
            .Select((result, _) => result.Diagnostics)
            .Where(arr => arr.Any())
            .Collect();

        context.RegisterSourceOutput(diagnostics, (ctx, diagnosticArrays) =>
        {
            foreach (var info in diagnosticArrays.SelectMany(a => a))
            {
                var diagnostic = Diagnostic.Create(info.Descriptor, info.Location?.ToLocation());
                ctx.ReportDiagnostic(diagnostic);
            }
        });

        var descriptors = results
            .Select((result, ctx) => result.Descriptor)
            .Where(d => d is not null)
            .Collect();

        context.RegisterSourceOutput(descriptors, static (ctx, list) =>
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
            foreach (var d in descriptors)
            {
                if (d == null) continue;
                var args = string.Join(", ", d.ParameterTypes.Select(t => $"Resolve<{t}>()"));

                if (d.IsSingleton)
                {
                    sb.AppendLine($"        var instance_{d.Interface} = new {d.Implementation}({args});");
                    sb.AppendLine($"        RegisterSingleton<{d.Interface}>(instance_{d.Interface});");
                }
                else
                {
                    sb.AppendLine($"        Register<{d.Interface}>(() => new {d.Implementation}({args}));");
                }
            }

            sb.AppendLine("     }");
            sb.AppendLine("}");

            ctx.AddSource("DiRegistration.g.cs", sb.ToString());
        });
    }
}