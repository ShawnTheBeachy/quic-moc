using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuicMoc.Models;

internal sealed record Parameter
{
    public string? DefaultValue { get; }
    public bool IsNullable { get; }
    public bool IsOut => RefKind == RefKind.Out;
    public string Name { get; }
    public RefKind RefKind { get; }
    public ITypeSymbol Type { get; }

    public Parameter(IParameterSymbol symbol)
    {
        IsNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated;
        Name = symbol.Name;
        RefKind = symbol.RefKind;
        Type = symbol.Type;

        if (!symbol.HasExplicitDefaultValue)
            return;

        var dsr = symbol.DeclaringSyntaxReferences;

        if (dsr.Length < 1)
            return;

        var syntax = dsr[0].GetSyntax();

        if (syntax is not ParameterSyntax parameterSyntax)
            return;

        if (parameterSyntax.Default is not null)
            DefaultValue = parameterSyntax.Default.Value.GetText().ToString();
    }
}

internal static class ParameterExtensions
{
    public static string Args(
        this IEnumerable<Parameter> parameters,
        IMethodSymbol? method,
        string? suffix = null,
        bool includeTypes = false,
        bool replaceGenericsWithObject = false,
        bool includeTypeParams = true
    ) =>
        string.Join(
            ", ",
            parameters
                .Select(x =>
                    $"{x.Ref()}{(includeTypes ? $"{(replaceGenericsWithObject && method is not null && x.Type.IsGeneric(method) ? "object?" : x.Type.FullyQualifiedName(method))} " : "")}{x.Name}{suffix}"
                )
                .Concat(
                    includeTypeParams && method is not null
                        ? method.TypeParameters.Select(param => $"typeof({param.Name})")
                        : []
                )
        );

    public static string ArgsWithGenericCasts(
        this IEnumerable<Parameter> parameters,
        IMethodSymbol method
    ) =>
        string.Join(
            ", ",
            parameters.Select(x =>
                $"{x.Ref()}{(x.Type.IsGeneric(method) ? $"({x.Type.FullyQualifiedName(method)})" : "")}{x.Name}"
            )
        );

    public static string ArgWrappers(
        this IEnumerable<Parameter> parameters,
        IMethodSymbol method
    ) =>
        string.Join(
            ", ",
            parameters.Select(x =>
                $"Arg<{x.Type.FullyQualifiedName(method)}{(x.IsNullable ? "?" : "")}>? {x.Name} = {x.DefaultValue ?? "null"}"
            )
        );

    public static string FullyQualifiedName(this ITypeSymbol type, IMethodSymbol? method) =>
        method is not null && type.IsGeneric(method)
            ? type.Name
            : $"global::{type.ContainingNamespace}.{type.Name}";

    public static string Parameters(
        this IEnumerable<Parameter> parameters,
        IMethodSymbol? method,
        bool includeDefaults = true,
        bool replaceGenericsWithObject = false,
        bool includeTypeParams = true
    ) =>
        string.Join(
            ", ",
            parameters
                .Select(x =>
                    (
                        Type: $"{x.Ref()}{(!replaceGenericsWithObject || method is null ? x.Type.FullyQualifiedName(method) : x.Type.IsGeneric(method) ? "object?" : x.Type.FullyQualifiedName(method))}{(x.IsNullable ? "?" : "")}",
                        Name: $"{x.Name}{(includeDefaults && x.DefaultValue is not null ? $" = {x.DefaultValue}" : "")}"
                    )
                )
                .Concat(
                    includeTypeParams && method is not null
                        ? Enumerable
                            .Repeat("Type", method.Arity)
                            .Select((x, i) => (Type: x, Name: $"t{i}"))
                        : []
                )
                .Select(x => $"{x.Type} {x.Name}")
        );

    public static string Ref(this Parameter parameter) =>
        parameter.RefKind switch
        {
            RefKind.In => "in ",
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            _ => "",
        };
}
