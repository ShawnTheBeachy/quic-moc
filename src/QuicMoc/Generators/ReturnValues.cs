using System.CodeDom.Compiler;
using System.Linq;
using QuicMoc.Internals;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class ReturnValues
{
    private static void GenerateOutSetters(this Method method, IndentedTextWriter textWriter)
    {
        foreach (var param in method.Parameters)
            if (param.IsOut)
                textWriter.WriteLine($"{param.Name} = default!;");
    }

    public static void GenerateReturnValues(this Method method, IndentedTextWriter textWriter)
    {
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("internal sealed class ReturnValues");
        textWriter.StartBlock();
        textWriter.WriteLine("private int _calls;");
        textWriter.WriteLine("private readonly Matcher _matcher;");
        textWriter.WriteLine($"private readonly {method.MockName} _methodMock;");
        textWriter.WriteLine("public int Calls => _methodMock.Calls(_matcher);");
        textWriter.WriteLine("public List<ReturnValue> Items { get; } = [];");
        textWriter.WriteLine("public Range? OnCallsRange { get; private set; }");
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine($"public ReturnValues(Matcher matcher, {method.MockName} methodMock)");
        textWriter.StartBlock();
        textWriter.WriteLine("_matcher = matcher;");
        textWriter.WriteLine("_methodMock = methodMock;");
        textWriter.WriteLine("methodMock._returnValues.Add(this);");
        textWriter.EndBlock();
        textWriter.WriteLineNoTabs("");

        var matchesArgs = method.Parameters.Where(x => !x.IsOut).ToArray();
        textWriter.WriteLine(
            $"public bool Matches{method.TypeParameters.Generics()}({matchesArgs.Select(x => x.ToString()).Join(", ")})"
        );
        textWriter.WriteLineIndented(
            $"=> _matcher({matchesArgs.Select(x => x.Name).Concat(method.TypeParameters.Select(x => $"typeof({x})")).Join(", ")});"
        );
        textWriter.WriteLineNoTabs("");

        textWriter.WriteLine("public void OnCalls(Range range) => OnCallsRange = range;");
        textWriter.WriteLineNoTabs("");

        textWriter.WriteLine(
            $"public {method.ReturnType} Value{method.TypeParameters.Generics()}({method.Parameters.Select(x => x.ToString()).Join(", ")})"
        );
        textWriter.StartBlock();
        textWriter.WriteLine("var index = 0;");
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("for (var i = 0; i < Items.Count; i++)");
        textWriter.StartBlock();
        textWriter.WriteLine("if (i > _calls)");
        textWriter.WriteLineIndented("break;");
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("index = i;");
        textWriter.EndBlock();
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("_calls++;");

        textWriter.WriteLine(
            $"{(method.ReturnsVoid ? "" : $"return {(method.ReturnTypeIsGeneric ? $"({method.ReturnType})" : "")}")}Items[index].Value({method.Parameters.Select(x => $"{x.Ref()}{x.Name}").Join(", ")});"
        );

        textWriter.EndBlock();
        textWriter.EndBlock();
        textWriter.WriteLineNoTabs("");
        method.GenerateReturnValuesBuilder(textWriter);
    }

    private static void GenerateReturnValuesBuilder(
        this Method method,
        IndentedTextWriter textWriter
    )
    {
        textWriter.WriteLine(
            $"public sealed class ReturnValuesBuilder{method.TypeParameters.Generics()}"
        );
        textWriter.StartBlock();
        textWriter.WriteLine("private readonly ReturnValues _returnValues;");
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("internal ReturnValuesBuilder(ReturnValues returnValues)");
        textWriter.StartBlock();
        textWriter.WriteLine("_returnValues = returnValues;");
        textWriter.EndBlock();
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("public int Calls => _returnValues.Calls;");
        textWriter.WriteLineNoTabs("");

        textWriter.WriteLine("public void OnCalls(Range range) => _returnValues.OnCalls(range);");
        textWriter.WriteLineNoTabs("");

        textWriter.WriteLine(
            $"public ReturnValuesBuilder{method.TypeParameters.Generics()} Returns(params {(method.ReturnsVoid ? "Action" : method.ReturnType)}[] returnValues)"
        );
        textWriter.StartBlock();
        textWriter.WriteLine("foreach (var returnValue in returnValues)");
        textWriter.StartBlock();
        textWriter.WriteLine(
            $"var rv = new ReturnValue(({method.Parameters.Select(x => x.ToString(x.IsGeneric ? "object" : x.Type)).Join(", ")}) =>"
        );
        textWriter.StartBlock();
        method.GenerateOutSetters(textWriter);
        textWriter.WriteLine($"{(method.ReturnsVoid ? "returnValue()" : "return returnValue")};");
        textWriter.EndBlock(");");
        textWriter.WriteLine("_returnValues.Items.Add(rv);");
        textWriter.EndBlock();
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("return this;");
        textWriter.EndBlock();
        textWriter.WriteLineNoTabs("");

        textWriter.WriteLine(
            $"public ReturnValuesBuilder{method.TypeParameters.Generics()} Returns(params Signature{method.TypeParameters.Generics()}[] returnValues)"
        );
        textWriter.StartBlock();
        textWriter.WriteLine("foreach (var returnValue in returnValues)");
        textWriter.StartBlock();
        textWriter.WriteLine(
            $"var rv = new ReturnValue(({method.Parameters.Select(x => x.ToString(x.IsGeneric ? "object?" : x.Type)).Join(", ")})"
        );
        textWriter.WriteLineIndented(
            $"=> returnValue({method.Parameters.Select(x => $"{x.Ref()}{(x.IsGeneric ? $"({x.Type}?)" : "")}{x.Name}").Join(", ")}));"
        );
        textWriter.WriteLine("_returnValues.Items.Add(rv);");
        textWriter.EndBlock();
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine("return this;");
        textWriter.EndBlock();
        textWriter.EndBlock();
    }
}
