using Microsoft.CodeAnalysis;

namespace QuicMoc.Models;

internal readonly record struct Property
{
    public bool IsInterface { get; }
    public bool IsReadOnly { get; }
    public string Name { get; }
    public string Type { get; }

    public Property(IPropertySymbol symbol)
    {
        IsInterface = symbol.Type.TypeKind == TypeKind.Interface;
        IsReadOnly = symbol.IsReadOnly;
        Name = symbol.Name;
        Type = symbol.Type.ToDisplayString();
    }
}
