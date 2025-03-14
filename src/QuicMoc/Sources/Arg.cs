using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace QuicMoc.Sources;

internal static class Arg
{
    public static IncrementalGeneratorPostInitializationContext AddArgSource(
        this IncrementalGeneratorPostInitializationContext context
    )
    {
        context.AddSource(
            "Arg.g.cs",
            SourceText.From(
                $$"""
                // <auto-generated />
                #nullable enable

                namespace {{Constants.Namespace}};

                public readonly record struct Arg<T>
                {
                    private readonly Func<T, bool>? _match;

                    private Arg(Func<T, bool>? match)
                    {
                        _match = match;
                    }

                    public static Arg<T> Any() => new Arg<T>(null);

                    public static Arg<T> Exactly(T? value) =>
                        new Arg<T>(x => (x is null && value is null)
                            || (x is not null && value is not null && x.Equals(value))
                        );

                    public override int GetHashCode() => 0;

                    public static Arg<T> Is(Func<T, bool> match) => new Arg<T>(match);

                    internal bool Matches(T other) => _match is null || _match(other);
                    
                    internal bool Matches(object other) => _match is null || (other is T otherT && _match(otherT));

                    public static Arg<T> Null() => new Arg<T>(x => x is null);
                    
                    public static implicit operator Arg<T>(T value) => Arg<T>.Exactly(value);
                }
                
                """,
                Encoding.UTF8
            )
        );
        return context;
    }
}
