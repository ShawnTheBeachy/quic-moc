using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using QuicMoc.Models;

namespace QuicMoc.Sources;

internal static class Call
{
    public static string MockCall(
        this IMethodSymbol method,
        string methodName,
        IReadOnlyList<Parameter> parameters
    )
    {
        var notOutParameters = parameters.Where(x => !x.IsOut).ToArray();
        return $$"""
                 internal readonly record struct {{methodName}}Call
                 {
                     {{string.Join("\n", notOutParameters.Select(param =>
                         $$"""
                           public required {{param.Type}} {{param.Name}} { get; init; }
                           """
                     ))}}
                         
                     public bool Matches(Matcher matcher) =>
                        matcher({{string.Join(", ", notOutParameters.Select(param => param.Name))}});
                 }
                 """;
    }
}
