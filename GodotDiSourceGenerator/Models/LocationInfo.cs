using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GodotDiSourceGenerator;

public sealed record LocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan)
{
    public Location ToLocation()
        => Location.Create(FilePath, Span, LineSpan);

    public static LocationInfo? Create(SyntaxNode node)
        => Create(node.GetLocation());

    public static LocationInfo? Create(Location location)
    {
        return location.SourceTree is null
            ? null
            : new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }
}