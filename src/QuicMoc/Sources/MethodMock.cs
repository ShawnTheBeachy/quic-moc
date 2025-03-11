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
            var anonArgs = parametersArr.Discards();
            var privateName =
                $"_{uniqueMethodName[0].ToString().ToLower()}{uniqueMethodName.Substring(1)}";
            var source = $$"""
                #region {{method.Name}}({{args}})
                {{returnType}} {{target.FullTypeName}}.{{method.Name}}({{parametersArr.Parameters(
                    false
                )}}) => {{privateName}}.{{method.Name}}({{args}});

                private {{uniqueMethodName}}MethodMock {{privateName}} = new();
                public {{uniqueMethodName}}MethodMock.ReturnValuesBuilder {{method.Name}}({{parametersArr.ArgWrappers()}})
                {
                    {{uniqueMethodName}}MethodMock.Matcher matcher = ({{parametersArr.Args(
                    "M"
                )}}) =>
                    {
                        {{string.Join(
                    "\n\n",
                    parametersArr.Select(p =>
                        $"if ({p.Name} is not null && !{p.Name}.Value.Matches({p.Name}M)) return false;"
                    )
                )}}

                        return true;
                    };
                    return new {{uniqueMethodName}}MethodMock.ReturnValuesBuilder(matcher, {{privateName}});
                }

                public sealed class {{uniqueMethodName}}MethodMock
                {
                    private int _calls;
                    private readonly List<ReturnValue> _returnValues = [new ReturnValue(({{anonArgs}}) => true, ({{anonArgs}}) => {{(method.ReturnsVoid ? " { }" : "default!")}})];

                    internal {{returnType}} {{method.Name}}({{parameters}})
                    {
                        _calls++;
                    
                        for (var i = _returnValues.Count - 1; i >= 0; i--)
                        {
                            var returnValue = _returnValues[i];
                            
                            if (!returnValue.Matches({{args}}))
                                continue;
                                
                            if (returnValue.OnCallsRange is null)
                                return returnValue.Value({{args}});
                                
                            if (returnValue.OnCallsRange.Value.Start.Value > _calls)
                                continue;

                            if (returnValue.OnCallsRange.Value.End.Value < _calls
                                && !returnValue.OnCallsRange.Value.End.IsFromEnd
                            )
                                continue;
                        
                            return returnValue.Value({{args}});
                        }

                        {{(method.ReturnsVoid ? "" : "return default!;")}}
                    }
                    
                    {{Delegates(returnType, parameters)}}
                    {{ReturnValuesBuilder( uniqueMethodName, returnType, anonArgs, method.ReturnsVoid)}}
                    {{ReturnValue(returnType, parameters, args)}}
                }
                #endregion {{method.Name}}({{args}})
                """;
            methods[i] = source;
        }

        return string.Join("\n\n", methods);
    }

    private static string ReturnValue(string returnType, string parameters, string args) =>
        $$"""
            internal sealed class ReturnValue
            {
                private int _calls;
                private readonly Matcher _matcher;
                private readonly Signature _value;
                internal Range? OnCallsRange { get; private set; }

                public ReturnValue(Matcher matcher, Signature value)
                {
                    _matcher = matcher;
                    _value = value;
                }

                public bool Matches({{parameters}}) => _matcher({{args}});

                public void OnCalls(Range range) => OnCallsRange = range;
                
                public {{returnType}} Value({{parameters}})
                {
                    _calls++;
                    return _value({{args}});
                }
            }
            """;

    private static string ReturnValuesBuilder(
        string methodName,
        string returnType,
        string anonymousArgs,
        bool returnsVoid
    ) =>
        $$"""
            public sealed class ReturnValuesBuilder
            {
                private readonly Matcher _matcher;
                private readonly {{methodName}}MethodMock _methodMock;
                private readonly List<ReturnValue> _returnValues = [];
                
                internal ReturnValuesBuilder(Matcher matcher, {{methodName}}MethodMock methodMock)
                {
                    _matcher = matcher;
                    _methodMock = methodMock;
                }
                
                public void OnCalls(Range range)
                {
                    foreach (var returnValue in _returnValues)
                        returnValue.OnCalls(range);
                }

                {{(returnsVoid ? "" :
                $$"""
                public ReturnValuesBuilder Returns(params {{returnType}}[] returnValues)
                {
                    foreach (var returnValue in returnValues)
                    {
                        var rv = new ReturnValue(_matcher, ({{anonymousArgs}}) => returnValue);
                        _methodMock._returnValues.Add(rv);
                        _returnValues.Add(rv);
                    }
                    
                    return this;
                }

                public ReturnValuesBuilder Returns(params Signature[] returnValues)
                {
                    foreach (var returnValue in returnValues)
                    {
                        var rv = new ReturnValue(_matcher, returnValue);
                        _methodMock._returnValues.Add(rv);
                        _returnValues.Add(rv);
                    }
                    
                    return this;
                }
                """)}}
            }
            """;
}
