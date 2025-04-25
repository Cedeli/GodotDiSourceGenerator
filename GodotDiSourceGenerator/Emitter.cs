using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace GodotDiSourceGenerator;

public static class Emitter
{
    public static void Emit(SourceProductionContext context, List<ServiceDescriptor?> descriptors)
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

        context.AddSource("DiRegistration.g.cs", sb.ToString());
    }
}