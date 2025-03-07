﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace QuicMoc;

internal static class Mock
{
    public static IncrementalGeneratorPostInitializationContext AddMockSource(
        this IncrementalGeneratorPostInitializationContext context
    )
    {
        context.AddSource(
            "Mock.g.cs",
            SourceText.From(
                $$"""
                // <auto-generated />
                namespace {{Constants.Namespace}};

                public static partial class Mock
                {
                    public static T For<T>() => default!;
                }
                """,
                Encoding.UTF8
            )
        );
        return context;
    }
}
