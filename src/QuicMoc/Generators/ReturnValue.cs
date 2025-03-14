using System.CodeDom.Compiler;
using System.Linq;
using QuicMoc.Internals;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class ReturnValue
{
    public static void GenerateReturnValue(this Method method, IndentedTextWriter textWriter)
    {
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("internal readonly record struct ReturnValue");
        textWriter.StartBlock();
        textWriter.WriteLine("private readonly ReturnValueSignature _value;");
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("public ReturnValue(ReturnValueSignature value)");
        textWriter.StartBlock();
        textWriter.WriteLine("_value = value;");
        textWriter.EndBlock();
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine(
            $"public {(method.ReturnTypeIsGeneric || method.ReturnTypeArity > 0 ? "object?" : method.ReturnType)} Value({method.Parameters.Select(x => x.ToString(x.NonGenericOrObject())).Join(", ")})"
        );
        textWriter.WriteLineIndented(
            $"=> _value({method.Parameters.Select(x => $"{x.Ref()}{x.Name}").Join(", ")});"
        );
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine(
            $"public delegate {(method.ReturnTypeIsGeneric || method.ReturnTypeArity > 0 ? "object?" : method.ReturnType)} ReturnValueSignature({method.Parameters.Select(x => x.ToString(x.NonGenericOrObject())).Join(", ")});"
        );
        textWriter.EndBlock();
    }
}
