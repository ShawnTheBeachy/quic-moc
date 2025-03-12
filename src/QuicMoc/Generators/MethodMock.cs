using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class MethodMock
{
    private static string Delegates(
        IMethodSymbol method,
        IReadOnlyList<Parameter> parameters,
        IReadOnlyList<Parameter> inParameters
    ) =>
        $"""
        public delegate bool Matcher({inParameters.Parameters(method, replaceGenericsWithObject: true)});
        public delegate {method.ReturnType()} {"Signature".MakeGeneric(
            method.TypeParameters
        )}({parameters.Parameters(null)});
        """;

    private static string Matcher(
        IMethodSymbol method,
        string methodName,
        IReadOnlyList<Parameter> parameters
    )
    {
        var typeParams =
            method.Arity < 1
                ? []
                : Enumerable.Repeat("t", method.Arity).Select((x, i) => $"{x}{i}").ToArray();
        return $$"""
            {{methodName}}MethodMock.Matcher matcher = ({{parameters.Args(null, "M")}}{{(method.Arity < 1 ? "" : $"{(parameters.Count > 0 ? ", " : "")}{string.Join(", ", typeParams)}")}}) =>
            {
                {{(method.Arity < 1 ? "" : string.Join("\n\n", typeParams.Select((x, i) => $"if (typeof({method.TypeParameters[i].Name}) != typeof(AnyType) && {x} != typeof({method.TypeParameters[i].Name})) return false;")))}}

                {{string.Join("\n\n", parameters.Select(p =>
                    $"if ({p.Name} is not null && !{p.Name}.Value.Matches({p.Name}M)) return false;"
                ))}}

                return true;
            };
            """;
    }

    public static string MockMethods(this MockGenerationTarget target)
    {
        var methods = new string[target.Methods.Count];
        var counter = 0;

        for (var i = 0; i < target.Methods.Count; i++)
        {
            var method = target.Methods[i];
            var uniqueMethodName = $"{method.Name}_{counter++}";
            var parameters = method.Parameters.Select(x => new Parameter(x)).ToArray();
            var inParameters = parameters.Where(x => !x.IsOut).ToArray();
            var args = parameters.Args(null);
            var privateName =
                $"_{uniqueMethodName[0].ToString().ToLower()}{uniqueMethodName.Substring(1)}";
            var source = $$"""
                #region {{method.Name}}{{method.TypeParameters.Generics()}}({{args}})
                {{method.ReturnType()}} {{target.FullTypeName}}.{{method.Name.MakeGeneric(method.TypeParameters)}}({{parameters.Parameters(null, false)}})
                    => {{privateName}}.{{method.Name.MakeGeneric(method.TypeParameters)}}({{args}});

                private {{uniqueMethodName}}MethodMock {{privateName}} = new();
                public {{uniqueMethodName}}MethodMock.ReturnValues.{{"ReturnValuesBuilder".MakeGeneric(method.TypeParameters)}} {{method.Name.MakeGeneric(method.TypeParameters)}}({{parameters.ArgWrappers()}})
                {
                    {{Matcher(method, uniqueMethodName, inParameters)}}
                    var returnValues = new {{uniqueMethodName}}MethodMock.ReturnValues(matcher, {{privateName}});
                    return new {{uniqueMethodName}}MethodMock.ReturnValues.{{"ReturnValuesBuilder".MakeGeneric(method.TypeParameters)}}(returnValues);
                }

                public sealed class {{uniqueMethodName}}MethodMock
                {
                    private readonly List<{{uniqueMethodName}}Call> _calls = [];
                    private readonly List<ReturnValues> _returnValues = [];

                    internal int Calls(Matcher matcher) => _calls.Count(call => call.Matches(matcher));

                    internal {{method.ReturnType()}} {{method.Name.MakeGeneric(method.TypeParameters)}}({{parameters.Parameters(null)}})
                    {
                        {{string.Join("\n",
                            parameters.Where(x => x.RefKind != RefKind.None).Select(param =>
                                $"{param.Name} = null!;"
                            )
                        )}}

                        var call = new {{uniqueMethodName}}Call
                        {
                            {{string.Join(",\n",
                                inParameters
                                    .Select(x => (PropName: x.Name, ParamName: x.Name))
                                    .Concat(method.TypeParameters.Select((x, i) => (PropName: $"{x.Name.ToLower()}{i}", ParamName: $"typeof({x.Name})")))
                                    .Select(x => $"{x.PropName} = {x.ParamName}"))}}
                        };
                        _calls.Add(call);
                        int? index = null;

                        for (var i = 0; i < _returnValues.Count; i++)
                        {
                            var returnValues = _returnValues[i];

                            if (!returnValues.{{"Matches".MakeGeneric(method.TypeParameters)}}({{inParameters.Args(null)}}))
                                continue;

                            if (returnValues.OnCallsRange is null)
                            {
                                index = i;
                                continue;
                            }

                            if (returnValues.OnCallsRange.Value.Start.Value > _calls.Count)
                                continue;

                            if (returnValues.OnCallsRange.Value.End.Value < _calls.Count
                                && !returnValues.OnCallsRange.Value.End.IsFromEnd
                            )
                                continue;

                            index = i;
                        }

                        {{(
                            method.ReturnsVoid
                                ? "if (index is not null)"
                                : "return index is null ? default!: "
                        )}} _returnValues[index.Value].{{"Value".MakeGeneric(method.TypeParameters)}}({{args}});
                    }

                    {{Delegates(method, parameters, inParameters)}}
                    {{ReturnValues.MockReturnValues(
                    method,
                    uniqueMethodName,
                    parameters,
                    inParameters
                )}}
                    {{ReturnValue.MockReturnValue(method, parameters)}}
                    {{Call.MockCall(method, uniqueMethodName, inParameters)}}
                }
                #endregion {{method.Name}}{{method.TypeParameters.Generics()}}({{args}})
                """;
            methods[i] = source;
        }

        return string.Join("\n\n", methods);
    }
}
