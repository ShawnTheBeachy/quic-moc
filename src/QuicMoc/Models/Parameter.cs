using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuicMoc.Models;

internal sealed record Parameter
{
    public string? DefaultValue { get; }
    public bool IsNullable { get; }
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
    public static string ArgWrappers(this IEnumerable<Parameter> parameters) =>
        string.Join(
            ", ",
            parameters.Select(x =>
                $"Arg<{x.Type}{(x.IsNullable ? "?" : "")}>? {x.Name} = {x.DefaultValue ?? "null"}"
            )
        );

    public static string Args(this IEnumerable<Parameter> parameters, string? suffix = null) =>
        string.Join(", ", parameters.Select(x => $"{x.Ref()} {x.Name}{suffix}"));

    public static string Discards(this IEnumerable<Parameter> parameters) =>
        string.Join(", ", parameters.Select(_ => '_'));

    public static string Parameters(
        this IEnumerable<Parameter> parameters,
        bool includeDefaults = true
    ) =>
        string.Join(
            ", ",
            parameters.Select(x =>
                $"{x.Ref()} {x.Type}{(x.IsNullable ? "?" : "")} {x.Name}{(includeDefaults && x.DefaultValue is not null ? $" = {x.DefaultValue}" : "")}"
            )
        );

    private static string Ref(this Parameter parameter) =>
        parameter.RefKind switch
        {
            RefKind.In => "in",
            RefKind.Out => "out",
            RefKind.Ref => "ref",
            _ => "",
        };
}
