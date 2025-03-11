using System.Collections.Generic;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class ReturnValue
{
    public static string MockReturnValue(string returnType, IReadOnlyList<Parameter> parameters) =>
        $$"""
            internal readonly record struct ReturnValue
            {
                private readonly Signature _value;

                public ReturnValue(Signature value)
                {
                    _value = value;
                }

                public {{returnType}} Value({{parameters.Parameters()}}) => _value({{parameters.Args()}});
            }
            """;
}
