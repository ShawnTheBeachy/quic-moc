using System.CodeDom.Compiler;
using System.Linq;
using QuicMoc.Internals;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class MethodMock
{
    private static void GenerateDelegates(this Method method, IndentedTextWriter textWriter)
    {
        textWriter.WriteLineNoTabs("");
        textWriter.WriteLine(
            $"public delegate bool Matcher({method.Parameters.Where(x => !x.IsOut).Select(x => x.ToString(x.IsGeneric ? "object?" : x.Type)).Concat(method.TypeParameters.Select(x => $"Type {x.ToLowerInvariant()}")).Join(", ")});"
        );
        textWriter.WriteLine(
            $"public delegate {method.ReturnType} Signature{method.TypeParameters.Generics()}({method.Parameters.Select(x => x.ToString()).Join(", ")});"
        );
        textWriter.WriteLineNoTabs("");
    }

    private static void GenerateMatcher(this Method method, IndentedTextWriter textWriter)
    {
        var typeParams =
            method.Arity < 1
                ? []
                : Enumerable.Repeat("t", method.Arity).Select((x, i) => $"{x}{i}").ToArray();
        textWriter.WriteLine(
            $"{method.MockName}.Matcher matcher = ({method.Parameters.Where(x => !x.IsOut).Select(x => $"{x.Name}M").Concat(typeParams).Join(", ")}) =>"
        );
        textWriter.StartBlock();

        for (var i = 0; i < typeParams.Length; i++)
        {
            var typeParam = typeParams[i];
            textWriter.WriteLine(
                $"if (typeof({method.TypeParameters[i]}) != typeof(AnyType) && {typeParam} != typeof({method.TypeParameters[i]}))"
            );
            textWriter.WriteLineIndented("return false;");

            if (i < typeParams.Length - 1 || method.Parameters.Length > 0)
                textWriter.WriteLineNoTabs("");
        }

        foreach (var param in method.Parameters)
        {
            if (param.IsOut)
                continue;

            textWriter.WriteLine(
                $"if ({param.Name} is not null && !{param.Name}.Value.Matches({param.Name}M))"
            );
            textWriter.WriteLineIndented("return false;");
            textWriter.WriteLineNoTabs("");
        }

        textWriter.WriteLine("return true;");
        textWriter.EndBlock(";");
    }

    public static void GenerateMethods(
        this MockGenerationTarget target,
        IndentedTextWriter textWriter
    )
    {
        for (var i = 0; i < target.Methods.Length; i++)
        {
            var method = target.Methods[i];
            var privateName =
                $"_{method.UniqueName[0].ToString().ToLower()}{method.UniqueName.Substring(1)}";

            #region Properties
            textWriter.WriteLine(
                $"#region {method.Name}{method.TypeParameters.Generics()}({method.ParameterNames.Join(", ")})"
            );
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine(
                $"{method.ReturnType} {target.Type}.{method.Name}{method.TypeParameters.Generics()}({method.Parameters.Select(x => x.ToString()).Join(", ")})"
            );
            textWriter.WriteLineIndented(
                $"=> {privateName}.{method.Name}{method.TypeParameters.Generics()}({method.Parameters.Select(x => $"{x.Ref()}{x.Name}").Join(", ")});"
            );
            textWriter.WriteLine($"private readonly {method.MockName} {privateName} = new();");
            textWriter.WriteLine(
                $"public {method.MockName}.ReturnValuesBuilder{method.TypeParameters.Generics()} {method.Name}{method.TypeParameters.Generics()}({method.Parameters.Select(x => $"Arg<{x.Type}>? {x.Name} = null").Join(", ")})"
            );
            textWriter.StartBlock();
            GenerateMatcher(method, textWriter);
            textWriter.WriteLine(
                $"var returnValues = new {method.MockName}.ReturnValues(matcher, {privateName});"
            );
            textWriter.WriteLine(
                $"return new {method.MockName}.ReturnValuesBuilder{method.TypeParameters.Generics()}(returnValues);"
            );
            textWriter.EndBlock();
            textWriter.WriteLineNoTabs("");
            #endregion Properties

            #region Class definitions
            textWriter.WriteLine($"public sealed class {method.MockName}");
            textWriter.StartBlock();
            textWriter.WriteLine("private readonly List<Call> _calls = [];");
            textWriter.WriteLine("private readonly List<ReturnValues> _returnValues = [];");
            textWriter.WriteLine(
                "internal int Calls(Matcher matcher) => _calls.Count(call => call.Matches(matcher));"
            );
            textWriter.WriteLineNoTabs("");

            textWriter.WriteLine(
                $"internal {method.ReturnType} {method.Name}{method.TypeParameters.Generics()}({method.Parameters.Select(x => x.ToString()).Join(", ")})"
            );
            textWriter.StartBlock();
            method.GenerateOutSetters(textWriter);
            textWriter.WriteLine("var call = new Call");
            textWriter.StartBlock();

            var props = method
                .Parameters.Where(x => !x.IsOut)
                .Select(x =>
                    (
                        ParamName: x.Name,
                        PropName: $"{char.ToUpperInvariant(x.Name[0])}{x.Name.Substring(1)}"
                    )
                )
                .Concat(
                    method.TypeParameters.Select(x =>
                        (ParamName: $"typeof({x})", PropName: $"TypeOf{x}")
                    )
                )
                .OrderBy(x => x.PropName);

            foreach (var prop in props)
                textWriter.WriteLine($"{prop.PropName} = {prop.ParamName},");

            textWriter.EndBlock(";");
            textWriter.WriteLine("_calls.Add(call);");
            textWriter.WriteLine("int? index = null;");
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine("for (var i = 0; i < _returnValues.Count; i++)");
            textWriter.StartBlock();
            textWriter.WriteLine("var returnValues = _returnValues[i];");
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine(
                $"if (!returnValues.Matches{method.TypeParameters.Generics()}({method.Parameters.Where(x => !x.IsOut).Select(x => x.Name).Join(", ")}))"
            );
            textWriter.WriteLineIndented("continue;");
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine("if (returnValues.OnCallsRange is null)");
            textWriter.StartBlock();
            textWriter.WriteLineIndented("index = i;");
            textWriter.WriteLineIndented("continue;");
            textWriter.EndBlock();
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine("if (returnValues.OnCallsRange.Value.Start.Value > _calls.Count)");
            textWriter.WriteLineIndented("continue;");
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine("if (returnValues.OnCallsRange.Value.End.Value < _calls.Count");
            textWriter.WriteLineIndented("&& !returnValues.OnCallsRange.Value.End.IsFromEnd");
            textWriter.WriteLine(")");
            textWriter.WriteLineIndented("continue;");
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine("index = i;");
            textWriter.EndBlock();
            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine(
                method.ReturnsVoid ? "if (index is not null)" : "return index is null ? default! :"
            );
            textWriter.WriteLineIndented(
                $"_returnValues[index.Value].Value{method.TypeParameters.Generics()}({method.Parameters.Select(x => $"{x.Ref()}{x.Name}").Join(", ")});"
            );
            textWriter.EndBlock();

            method.GenerateDelegates(textWriter);
            method.GenerateCall(textWriter);
            method.GenerateReturnValues(textWriter);
            method.GenerateReturnValue(textWriter);
            textWriter.EndBlock();
            #endregion Class definitions

            textWriter.WriteLineNoTabs("");
            textWriter.WriteLine(
                $"#endregion {method.Name}{method.TypeParameters.Generics()}({method.ParameterNames.Join(", ")})"
            );

            if (i < target.Methods.Length - 1)
                textWriter.WriteLineNoTabs("");
        }
    }

    private static void GenerateOutSetters(this Method method, IndentedTextWriter textWriter)
    {
        foreach (var param in method.Parameters)
            if (param.IsOut)
                textWriter.WriteLine($"{param.Name} = default!;");
    }
}
