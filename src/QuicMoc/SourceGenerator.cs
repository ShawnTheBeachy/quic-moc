using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using QuicMoc.Generators;
using QuicMoc.Models;
using QuicMoc.Sources;

namespace QuicMoc;

[Generator]
public sealed class SourceGenerator : IIncrementalGenerator
{
    private static void GenerateCode(
        SourceProductionContext context,
        ImmutableArray<MockGenerationTarget> targets
    )
    {
        var uniqueTypes = new HashSet<string>();

        foreach (var target in targets)
        {
            if (!uniqueTypes.Add(target.FullTypeName))
                continue;

            var sourceCode = $$"""
                // <auto-generated />
                #nullable enable

                namespace {{Constants.Namespace}};

                public sealed class MockFor{{target.Type.Name}} : {{target.FullTypeName}}
                {
                    {{target.MockProperties()}}
                    
                    {{target.MockMethods()}}
                }

                internal static partial class MockExtensions
                {
                    public static MockFor{{target.Type.Name}} Quick(this {{Mock.ClassName}}<{{target.FullTypeName}}> mock) => new();
                }
                """;
            context.AddSource(
                $"{target.Type.ContainingNamespace.ToDisplayString().Replace('.', '_')}_MockFor{target.Type.Name}.g.cs",
                SourceText.From(sourceCode, Encoding.UTF8)
            );
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddAnyTypeSource().AddArgSource().AddMockSource().AddPropertyMockSource()
        );

        var provider = context
            .SyntaxProvider.CreateSyntaxProvider(
                (s, _) =>
                {
                    if (s is not GenericNameSyntax gns)
                        return false;

                    if (gns.Arity != 1)
                        return false;

                    return gns.Identifier.Text == "Mock";
                },
                (ctx, _) => new MockGenerationTarget(ctx)
            )
            .Where(t => t.Type.IsAbstract);

        // Generate the source code.
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, [.. t.Right.OfType<MockGenerationTarget>()])
        );
    }
}
