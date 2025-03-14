using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using QuicMoc.Internals;

namespace QuicMoc.Models;

internal sealed record Method
{
    private readonly int _uniqueIndex;
    public int Arity => TypeParameters.Length;
    public string MockName => $"{UniqueName}MethodMock";
    public string Name { get; }
    public IEnumerable<string> ParameterNames =>
        Parameters.IsDefaultOrEmpty ? [] : Parameters.Select(x => x.Name);
    public EquatableArray<Parameter> Parameters { get; }
    public bool ReturnsVoid { get; }
    public string ReturnType { get; }
    public bool ReturnTypeIsGeneric =>
        !TypeParameters.IsDefaultOrEmpty && TypeParameters.Contains(ReturnType);
    public EquatableArray<string> TypeParameters { get; }
    public string UniqueName => $"{Name}_{_uniqueIndex}";

    public Method(IMethodSymbol symbol, int uniqueIndex)
    {
        _uniqueIndex = uniqueIndex;
        Name = symbol.Name;

        var parameters = new Parameter[symbol.Parameters.Length];

        for (var i = 0; i < symbol.Parameters.Length; i++)
            parameters[i] = new Parameter(symbol.Parameters[i], this);

        Parameters = parameters.ToImmutableArray();
        ReturnsVoid = symbol.ReturnsVoid;
        ReturnType = symbol.ReturnsVoid ? "void" : symbol.ReturnType.ToDisplayString();
        TypeParameters = symbol.TypeParameters.Select(x => x.ToDisplayString()).ToImmutableArray();
    }
}
