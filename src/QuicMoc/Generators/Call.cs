using System.CodeDom.Compiler;
using System.Linq;
using QuicMoc.Internals;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class Call
{
    public static void GenerateCall(this Method method, IndentedTextWriter textWriter)
    {
        textWriter.WriteLine("internal readonly record struct Call");
        textWriter.StartBlock();
        var props = method
            .Parameters.Where(x => !x.IsOut)
            .Select(x =>
                (
                    Type: x.IsGeneric ? "object?" : x.Type,
                    Name: $"{char.ToUpperInvariant(x.Name[0])}{x.Name.Substring(1)}"
                )
            )
            .Concat(method.TypeParameters.Select(x => (Type: "Type", Name: $"TypeOf{x}")))
            .ToArray();

        foreach (var prop in props)
            textWriter.WriteLine($"public required {prop.Type} {prop.Name} {{ get; init; }}");

        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("public bool Matches(Matcher matcher) =>");
        textWriter.WriteLineIndented($"matcher({props.Select(x => x.Name).Join(", ")});");
        textWriter.EndBlock();
    }
}
