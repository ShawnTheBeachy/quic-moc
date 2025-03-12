using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using QuicMoc.Models;

namespace QuicMoc;

internal static class Extensions
{
    public static string Generics(this IEnumerable<ITypeParameterSymbol> typeParameters) =>
        $"<{string.Join(", ", typeParameters.Select(param => param.Name))}>";

    public static bool IsGeneric(this ITypeSymbol type, IMethodSymbol method) =>
        method.TypeParameters.Contains(type, SymbolEqualityComparer.Default);

    public static string MakeGeneric(
        this string name,
        IReadOnlyList<ITypeParameterSymbol> typeParameters
    ) => typeParameters.Any() ? $"{name}{typeParameters.Generics()}" : name;

    public static string ReturnType(
        this IMethodSymbol method,
        bool replaceGenericWithObject = false
    ) =>
        method.ReturnsVoid ? "void"
        : replaceGenericWithObject && method.ReturnType.IsGeneric(method) ? "object"
        : method.ReturnType.FullyQualifiedName(method);
}
