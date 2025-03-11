using System.Linq;
using Microsoft.CodeAnalysis;

namespace QuicMoc.Sources;

internal static class Call
{
    public static string MockCall(
        this IMethodSymbol method,
        string methodName
    ) =>
    $$"""
     internal readonly record struct {{methodName}}Call
     {
         {{string.Join("\n", method.Parameters.Select(param =>
            $$"""
             public required {{param.Type}} {{param.Name}} { get; init; }
             """
         ))}}
             
         public bool Matches(Matcher matcher) =>
            matcher({{string.Join(", ", method.Parameters.Select(param => param.Name))}});
     }
     """;
}