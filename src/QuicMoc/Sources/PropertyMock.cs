using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace QuicMoc.Sources;

internal static class PropertyMock
{
    public const string ClassName = "PropertyMock";

    public static IncrementalGeneratorPostInitializationContext AddPropertyMockSource(
        this IncrementalGeneratorPostInitializationContext context
    )
    {
        context.AddSource(
            $"{ClassName}.g.cs",
            SourceText.From(
                $$"""
                // <auto-generated />
                namespace {{Constants.Namespace}};

                internal record struct {{ClassName}}<T>
                {
                    private T _value;

                    public {{ClassName}}()
                    {
                        _value = default(T)!;
                    }

                    public {{ClassName}}(T value)
                    {
                        _value = value;
                    }
                    
                    public T Get() => _value;

                    public override int GetHashCode() => _value.GetHashCode();
                    
                    public void Set(T value) => _value = value;

                    public override string ToString() => _value.ToString();

                    public static implicit operator T({{ClassName}}<T> mock) => mock._value;

                    public static implicit operator {{ClassName}}<T>(T value) => new(value);
                }
                
                """,
                Encoding.UTF8
            )
        );
        return context;
    }
}
