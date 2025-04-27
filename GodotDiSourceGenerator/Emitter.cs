using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

internal static class Emitter
{
    internal static void EmitRegistry(SourceProductionContext context, List<ServiceDescriptor?> descriptors)
    {
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

            switch (d.Lifetime)
            {
                case Lifetime.Scoped:
                    sb.AppendLine($"        RegisterScoped<{d.Interface}>(() => new {d.Implementation}({args}));");
                    break;
                case Lifetime.Singleton:
                    sb.AppendLine($"        var instance_{d.Interface} = new {d.Implementation}({args});");
                    sb.AppendLine($"        RegisterSingleton<{d.Interface}>(instance_{d.Interface});");
                    break;
                case Lifetime.Transient:
                default:
                    sb.AppendLine($"        Register<{d.Interface}>(() => new {d.Implementation}({args}));");
                    break;
            }
        }

        sb.AppendLine("     }");
        sb.AppendLine("}");

        context.AddSource("Registry.generated.cs", sb.ToString());
    }

    internal static void EmitScope(SourceProductionContext context, List<ISymbol?> symbols)
    {
        if (symbols.Count == 0) return;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("using Godot;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("");
        sb.AppendLine("public partial class InjectionContainer");
        sb.AppendLine("{");
        sb.AppendLine("     private readonly Dictionary<Node, IServiceScope> _scopedRoots = new();");
        sb.AppendLine("");
        sb.AppendLine("     partial void RegisterScopeHandlers()");
        sb.AppendLine("     {");
        sb.AppendLine("         var tree = GetTree();");
        sb.AppendLine("         tree.NodeAdded += OnNodeAdded;");
        sb.AppendLine("         tree.NodeRemoved += OnNodeRemoved;");
        sb.AppendLine("     }");
        sb.AppendLine("");
        sb.AppendLine("     private void OnNodeAdded(Node node)");
        sb.AppendLine("     {");
        foreach (var s in symbols.OfType<ISymbol>())
        {
            sb.AppendLine($"         if (node is {s.Name})");
            sb.AppendLine("         {");
            sb.AppendLine("             _scopedRoots[node] = CreateScope();");
            sb.AppendLine("         }");
            sb.AppendLine("");
        }

        sb.AppendLine("     }");
        sb.AppendLine("");
        sb.AppendLine("     private void OnNodeRemoved(Node node)");
        sb.AppendLine("     {");
        sb.AppendLine("         if (_scopedRoots.TryGetValue(node, out var scope))");
        sb.AppendLine("         {");
        sb.AppendLine("             scope.Dispose();");
        sb.AppendLine("             _scopedRoots.Remove(node);");
        sb.AppendLine("         }");
        sb.AppendLine("     }");
        sb.AppendLine("}");

        context.AddSource($"ScopeLifecycle.generated.cs", sb.ToString());
    }
}