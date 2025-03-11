using System.Linq;
using QuicMoc.Models;

namespace QuicMoc.Sources;

internal static class MethodMock
{
    private static string Delegates(string returnType, string parameters) =>
        $"""
            public delegate bool Matcher({parameters});
            public delegate {returnType} Signature({parameters});
            """;

    public static string MockMethods(this MockGenerationTarget target)
    {
        var methods = new string[target.Methods.Count];
        var counter = 0;

        for (var i = 0; i < target.Methods.Count; i++)
        {
            var method = target.Methods[i];
            var uniqueMethodName = $"{method.Name}_{counter++}";
            var returnType = method.ReturnsVoid ? "void" : method.ReturnType.Name;
            var parametersArr = method.Parameters.Select(x => new Parameter(x)).ToArray();
            var parameters = parametersArr.Parameters();
            var args = parametersArr.Args();
            var discards = parametersArr.Discards();
            var privateName =
                $"_{uniqueMethodName[0].ToString().ToLower()}{uniqueMethodName.Substring(1)}";
            var source = $$"""
                #region {{method.Name}}({{args}})
                {{returnType}} {{target.FullTypeName}}.{{method.Name}}({{parametersArr.Parameters(
                    false
                )}}) => {{privateName}}.{{method.Name}}({{args}});

                private {{uniqueMethodName}}MethodMock {{privateName}} = new();
                public {{uniqueMethodName}}MethodMock.ReturnValues {{method.Name}}({{parametersArr.ArgWrappers()}})
                {
                    {{uniqueMethodName}}MethodMock.Matcher matcher = ({{parametersArr.Args(
                    "M"
                )}}) =>
                    {
                        {{string.Join("\n\n",
                            parametersArr.Select(p =>
                                $"if ({p.Name} is not null && !{p.Name}.Value.Matches({p.Name}M)) return false;"
                            )
                        )}}

                        return true;
                    };
                    return new {{uniqueMethodName}}MethodMock.ReturnValues(matcher, {{privateName}});
                }

                public sealed class {{uniqueMethodName}}MethodMock
                {
                    private readonly List<{{uniqueMethodName}}Call> _calls = [];
                    private readonly List<ReturnValues> _returnValues = [];
                    
                    internal int Calls(Matcher matcher) => _calls.Count(call => call.Matches(matcher));

                    internal {{returnType}} {{method.Name}}({{parameters}})
                    {
                        var call = new {{uniqueMethodName}}Call
                        {
                            {{string.Join(",\n", parametersArr.Select(param => $"{param.Name} = {param.Name}"))}}
                        };
                        _calls.Add(call);
                        int? index = null;

                        for (var i = 0; i < _returnValues.Count; i++)
                        {
                            var returnValues = _returnValues[i];

                            if (!returnValues.Matches({{args}}))
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

                        {{(method.ReturnsVoid ? "" : $"return index is null ? default! : _returnValues[index.Value].Value({args});")}}
                    }

                    {{Delegates(returnType, parameters)}}
                    {{ReturnValues(uniqueMethodName, returnType, parameters, args, discards, method.ReturnsVoid)}}
                    {{ReturnValue(returnType, parameters, args)}}
                    {{method.MockCall(uniqueMethodName)}}
                }
                #endregion {{method.Name}}({{args}})
                """;
            methods[i] = source;
        }

        return string.Join("\n\n", methods);
    }

    private static string ReturnValue(string returnType, string parameters, string args) =>
        $$"""
            internal readonly record struct ReturnValue
            {
                private readonly Signature _value;

                public ReturnValue(Signature value)
                {
                    _value = value;
                }

                public {{returnType}} Value({{parameters}}) => _value({{args}});
            }
            """;

    private static string ReturnValues(
        string methodName,
        string returnType,
        string parameters,
        string args,
        string discards,
        bool returnsVoid
    ) =>
        $$"""
            public sealed class ReturnValues
            {
                private int _calls;
                private readonly Matcher _matcher;
                private readonly {{methodName}}MethodMock _methodMock;
                private readonly List<ReturnValue> _returnValues = [];
                internal Range? OnCallsRange { get; private set; }

                internal ReturnValues(Matcher matcher, {{methodName}}MethodMock methodMock)
                {
                    _matcher = matcher;
                    _methodMock = methodMock;
                    methodMock._returnValues.Add(this);
                }
                
                public int Calls => _methodMock.Calls(_matcher);
                
                internal bool Matches({{parameters}}) => _matcher({{args}});

                public void OnCalls(Range range) => OnCallsRange = range;

                {{(returnsVoid ? "" :
                $$"""
                public ReturnValues Returns(params {{returnType}}[] returnValues)
                {
                    foreach (var returnValue in returnValues)
                    {
                        var rv = new ReturnValue(({{discards}}) => returnValue);
                        _returnValues.Add(rv);
                    }

                    return this;
                }

                public ReturnValues Returns(params Signature[] returnValues)
                {
                    foreach (var returnValue in returnValues)
                    {
                        var rv = new ReturnValue(returnValue);
                        _returnValues.Add(rv);
                    }

                    return this;
                }
                
                internal {{returnType}} Value({{parameters}})
                {
                    var index = 0;
                    
                    for (var i = 0; i < _returnValues.Count; i++)
                    {
                        if (i > _calls)
                            break;
                            
                        index = i;
                    }
                
                    _calls++;
                    return _returnValues[index].Value({{args}});
                }
                """)}}
            }
            """;
}
