using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using QuicMoc.Models;

namespace QuicMoc.Internals;

internal static class Extensions
{
    public static void EndBlock(this IndentedTextWriter writer, string? suffix = null)
    {
        writer.Indent--;
        writer.WriteLine($"}}{suffix}");
    }

    public static string Generics(this EquatableArray<string> types) =>
        types.Length < 1 ? "" : $"<{string.Join(", ", types)}>";

    public static string Join(this IEnumerable<string> items, string separator) =>
        string.Join(separator, items);

    public static string NonGenericOrObject(this Parameter parameter) =>
        parameter.IsGeneric || parameter.Arity > 0 ? "object?" : parameter.Type;

    public static string Ref(this Parameter parameter) =>
        parameter.RefKind switch
        {
            RefKind.In => "in ",
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            _ => "",
        };

    public static void StartBlock(this IndentedTextWriter writer)
    {
        writer.WriteLine('{');
        writer.Indent++;
    }

    public static void WriteLineIndented(this IndentedTextWriter writer, string value)
    {
        writer.Indent++;
        writer.WriteLine(value);
        writer.Indent--;
    }
}
