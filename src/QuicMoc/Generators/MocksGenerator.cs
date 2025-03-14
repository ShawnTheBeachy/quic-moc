using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using QuicMoc.Internals;
using QuicMoc.Models;
using QuicMoc.Sources;

namespace QuicMoc.Generators;

[Generator]
public sealed class MocksGenerator : IIncrementalGenerator
{
    private static void GenerateCode(
        SourceProductionContext context,
        ImmutableArray<MockGenerationTarget> targets
    )
    {
        var uniqueTypes = new HashSet<string>();

        foreach (var target in targets)
        {
            if (!uniqueTypes.Add(target.Type))
                continue;

            var baseWriter = new StringWriter();
            var textWriter = new IndentedTextWriter(baseWriter);

            textWriter.Write(
                $"""
                // <auto-generated />
                #nullable enable

                namespace {Constants.Namespace};

                public sealed class MockFor{target.Name} : {target.Type}
                """
            );
            textWriter.WriteLine();
            textWriter.StartBlock();
            target.MockProperties(textWriter);
            textWriter.WriteLineNoTabs("");
            target.GenerateMethods(textWriter);
            textWriter.EndBlock();
            textWriter.WriteLineNoTabs("");
            GenerateExtension(target, textWriter);

            context.AddSource(
                $"MockFor{target.Type.Replace('.', '_')}.g.cs",
                SourceText.From(baseWriter.ToString(), Encoding.UTF8)
            );
        }
    }

    private static void GenerateExtension(
        MockGenerationTarget target,
        IndentedTextWriter textWriter
    )
    {
        textWriter.WriteLine("internal static partial class MockExtensions");
        textWriter.StartBlock();
        textWriter.WriteLine(
            $"public static MockFor{target.Name} Quick(this {Mock.ClassName}<{target.Type}> mock) => new();"
        );
        textWriter.EndBlock();
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
                (ctx, _) => MockGenerationTarget.TryCreate(ctx)
            )
            .Where(t => t is not null);

        // Generate the source code.
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Right!)
        );
    }
}
