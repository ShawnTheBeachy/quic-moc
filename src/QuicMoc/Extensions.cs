using Microsoft.CodeAnalysis;

namespace QuicMoc;

internal static class Extensions
{
    public static string ReturnType(this IMethodSymbol method) =>
        method.ReturnsVoid ? "void" : method.ReturnType.Name;
}
