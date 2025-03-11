﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class Call
{
    public static string MockCall(
        IMethodSymbol method,
        string methodName,
        IReadOnlyList<Parameter> parameters
    )
    {
        var props = parameters
            .Select(param => (Type: param.Type.Name, Name: param.Name))
            .Concat(method.Generics())
            .ToArray();
        return $$"""
             internal readonly record struct {{methodName}}Call
             {
                 {{string.Join("\n", props.Select(prop =>
                     $$"""
                       public required {{prop.Type}} {{prop.Name}} { get; init; }
                       """
                 ))}}
                     
                 public bool Matches(Matcher matcher) =>
                    matcher({{string.Join(", ", props.Select(param => param.Name))}});
             }
             """;
    }
}
