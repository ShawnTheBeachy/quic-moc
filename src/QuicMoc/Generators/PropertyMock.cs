using System.CodeDom.Compiler;
using QuicMoc.Internals;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class PropertyMock
{
    public static void MockProperties(
        this MockGenerationTarget target,
        IndentedTextWriter textWriter
    )
    {
        for (var i = 0; i < target.Properties.Length; i++)
        {
            var prop = target.Properties[i];
            var privateName = $"_{char.ToLower(prop.Name[0])}{prop.Name.Substring(1)}";
            textWriter.WriteLine($"#region {prop.Name}");
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine(
                $"{prop.Type} {target.Type}.{prop.Name}{(prop.IsReadOnly ? prop.MockReadOnlyProperty() : "")}"
            );

            if (!prop.IsReadOnly)
                prop.MockWriteableProperty(textWriter);

            textWriter.WriteLine(
                $"private {Sources.PropertyMock.ClassName}<{prop.Type}> {privateName} = new();"
            );
            textWriter.WriteLine($"public {prop.Type} {prop.Name}");
            textWriter.StartBlock();
            textWriter.WriteLine($"get => {privateName}{(prop.IsInterface ? ".Get()" : "")};");
            textWriter.WriteLine(
                $"set => {privateName}{(prop.IsInterface ? ".Set(value)" : " = value")};"
            );
            textWriter.EndBlock();
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine($"#endregion {prop.Name}");

            if (i < target.Properties.Length - 1)
                textWriter.WriteLineNoTabs("");
        }
    }

    private static string MockReadOnlyProperty(this Property property) => $" => {property.Name};";

    private static void MockWriteableProperty(this Property property, IndentedTextWriter textWriter)
    {
        textWriter.StartBlock();
        textWriter.WriteLine($"get => {property.Name};");
        textWriter.WriteLine($"set => {property.Name} = value;");
        textWriter.EndBlock();
    }
}
