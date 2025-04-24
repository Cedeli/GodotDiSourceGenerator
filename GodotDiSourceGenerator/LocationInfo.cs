using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GodotDiSourceGenerator;

public sealed record LocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan)
{
    public Location ToLocation()
        => Location.Create(FilePath, Span, LineSpan);

    public static LocationInfo? Create(Location? loc) => loc?.SourceTree is null
        ? null
        : new LocationInfo(
            Location.Create(loc.SourceTree.FilePath, loc.SourceSpan, loc.GetLineSpan().Span).SourceTree!.FilePath,
            loc.SourceSpan, loc.GetLineSpan().Span);
}