using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QuicMoc.Internals;

namespace QuicMoc.Models;

internal sealed class MockGenerationTarget
{
    public EquatableArray<Method> Methods { get; }
    public string Name { get; }
    public EquatableArray<Property> Properties { get; }
    public string Type { get; }

    private MockGenerationTarget(ITypeSymbol type)
    {
        Name = type.Name;
        Type = type.ToDisplayString();
        var members = type.GetMembers();
        Properties = members
            .OfType<IPropertySymbol>()
            .Select(symbol => new Property(symbol))
            .ToImmutableArray();
        Methods = members
            .OfType<IMethodSymbol>()
            .Where(x => !x.Name.StartsWith("get_") && !x.Name.StartsWith("set_"))
            .Select((x, i) => new Method(x, i))
            .ToImmutableArray();
    }

    public static MockGenerationTarget? TryCreate(GeneratorSyntaxContext context)
    {
        if (context.Node is not GenericNameSyntax gns)
            return null;

        var proxyType = context.SemanticModel.GetTypeInfo(gns.TypeArgumentList.Arguments[0]).Type;

        if (proxyType is null)
            return null;

        return proxyType.IsAbstract ? new MockGenerationTarget(proxyType) : null;
    }
}
