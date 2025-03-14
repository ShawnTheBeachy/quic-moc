using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuicMoc.Models;

internal sealed record Parameter
{
    private readonly string? _defaultValue;
    private readonly Method _method;
    public int Arity { get; }
    public bool IsGeneric =>
        !_method.TypeParameters.IsDefaultOrEmpty && _method.TypeParameters.Contains(Type);
    public bool IsOut => RefKind == RefKind.Out;
    public string Name { get; }
    public RefKind RefKind { get; }
    public string Type { get; }

    public Parameter(IParameterSymbol symbol, Method method)
    {
        _method = method;
        Arity = symbol.Type is INamedTypeSymbol nts ? nts.Arity : 0;
        Name = symbol.Name;
        RefKind = symbol.RefKind;
        Type = symbol.Type.ToDisplayString();

        if (!symbol.HasExplicitDefaultValue)
            return;

        var dsr = symbol.DeclaringSyntaxReferences;

        if (dsr.Length < 1)
            return;

        var syntax = dsr[0].GetSyntax();

        if (syntax is not ParameterSyntax parameterSyntax)
            return;

        if (parameterSyntax.Default is not null)
            _defaultValue = parameterSyntax.Default.Value.GetText().ToString();
    }

    private string Ref() =>
        RefKind switch
        {
            RefKind.In => "in ",
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            _ => "",
        };

    public string ToString(string overrideTypeName) =>
        $"{Ref()}{overrideTypeName} {Name}{(_defaultValue is null ? "" : $" = {_defaultValue}")}";

    public override string ToString() => ToString(Type);
}
