using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuicMoc.Models;

internal sealed class MockGenerationTarget
{
    public string FullTypeName => $"{Type.ContainingNamespace}.{Type.Name}";
    public IReadOnlyList<IMethodSymbol> Methods { get; }
    public IReadOnlyList<IPropertySymbol> Properties { get; }
    public ITypeSymbol Type { get; }

    public MockGenerationTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not GenericNameSyntax gns)
            throw new Exception(
                $"Expected a {nameof(GenericNameSyntax)} node but instead received {context.Node.Kind()}."
            );

        var proxyType = gns.TypeArgumentList.Arguments[0];
        Type = context.SemanticModel.GetTypeInfo(proxyType).Type!;
        var members = Type.GetMembers();
        Properties = members.OfType<IPropertySymbol>().ToArray();
        Methods = members
            .OfType<IMethodSymbol>()
            .Where(x => !x.Name.StartsWith("get_") && !x.Name.StartsWith("set_"))
            .ToArray();
    }
}
