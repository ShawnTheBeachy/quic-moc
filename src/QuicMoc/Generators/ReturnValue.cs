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
                private readonly ReturnValueSignature _value;

                public ReturnValue(ReturnValueSignature value)
                {
                    _value = value;
                }

                public {{method.ReturnType(true)}} Value({{parameters.Parameters(
                method,
                replaceGenericsWithObject: true,
                includeTypeParams: false
            )}})
                    => _value({{parameters.Args(null)}});
                    
                public delegate {{method.ReturnType(true)}} ReturnValueSignature({{parameters.Parameters(method, replaceGenericsWithObject: true, includeTypeParams: false)}});
            }
            """;
}
