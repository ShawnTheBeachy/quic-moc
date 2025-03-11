using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class ReturnValue
{
    public static string MockReturnValue(
        IMethodSymbol method,
        IReadOnlyList<Parameter> parameters
    ) =>
        $$"""
            internal readonly record struct ReturnValue
            {
                private readonly Signature _value;

                public ReturnValue(Signature value)
                {
                    _value = value;
                }

                public {{method.ReturnType()}} Value({{parameters.Parameters(null)}})
                    => _value({{parameters.Args(null)}});
            }
            """;
}
