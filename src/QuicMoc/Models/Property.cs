using Microsoft.CodeAnalysis;

namespace QuicMoc.Models;

internal readonly record struct Property
{
    public bool IsReadOnly { get; }
    public string Name { get; }
    public string Type { get; }

    public Property(IPropertySymbol symbol)
    {
        IsReadOnly = symbol.IsReadOnly;
        Name = symbol.Name;
        Type = symbol.Type.ToDisplayString();
    }
}
