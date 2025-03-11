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
                public {{uniqueMethodName}}MethodMock.ReturnValues {{method.Name}}({{parametersArr.ArgWrappers()}})
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
                    var returnValues = new {{uniqueMethodName}}MethodMock.ReturnValues(matcher);
                    {{privateName}}.AddReturnValues(returnValues);
                    return returnValues;
                }

                public sealed class {{uniqueMethodName}}MethodMock
                {
                    private readonly List<ReturnValues> _returnValues = [];

                    internal {{returnType}} {{method.Name}}({{parameters}})
                    {
                        for (var i = _returnValues.Count - 1; i >= 0; i--)
                        {
                            var returnValues = _returnValues[i];

                            if (returnValues.Matches({{args}}))
                                return returnValues.Value({{args}});
                        }

                        return default!;
                    }
                    
                    internal void AddReturnValues(ReturnValues returnValues) => _returnValues.Add(returnValues);

                    {{Delegates(returnType, parameters)}}
                    {{ReturnValues(returnType, parameters, args, anonArgs, method.ReturnsVoid)}}
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
            public sealed class ReturnValue
            {
                private int _index;
                private readonly IReadOnlyList<Signature> _values;
                internal Range? OnCallsRange { get; private set; }

                internal ReturnValue(IReadOnlyList<Signature> values)
                {
                    _values = values;
                }

                public void OnCalls(Range range) => OnCallsRange = range;
                
                internal {{returnType}} Value({{parameters}})
                {
                    var value = _values[_index]({{args}});
                    
                    if (_index < _values.Count - 1)
                        _index++;
                        
                    return value;
                }
            }
            """;

    private static string ReturnValues(
        string returnType,
        string parameters,
        string args,
        string anonymousArgs,
        bool returnsVoid
    ) =>
        $$"""
            public sealed class ReturnValues
            {
                private int _calls = 0;
                private readonly Matcher _matcher;
                private readonly List<ReturnValue> _values = [];
                
                internal ReturnValues(Matcher matcher)
                {
                    _matcher = matcher;
                }

                internal bool Matches({{parameters}}) => _matcher({{args}});
                
                {{(returnsVoid ? "" :
                $$"""
                  public ReturnValue Returns({{returnType}} returnValue)
                  {
                      var rv = new ReturnValue([({{anonymousArgs}}) => returnValue]);
                      _values.Add(rv);
                      return rv;
                  }

                  public ReturnValue Returns(params Signature[] returnValues)
                  {
                      var rv = new ReturnValue(returnValues);
                      _values.Add(rv);
                      return rv;
                  }
                  """)}}

                internal {{returnType}} Value({{parameters}})
                {
                    _calls++;
            
                    if (_values.Count < 1)
                        {{(returnsVoid ? "return" : "return default!")}};

                    if (_values.Count == 1)
                        return _values[0].Value({{args}});
                    
                    {{returnType}} value;
                    
                    foreach (var returnValue in _values)
                    {
                        if (returnValue.OnCallsRange is null)
                        {
                            value = returnValue.Value({{args}});
                            continue;
                        }
                            
                        if (returnValue.OnCallsRange.Value.Start.Value > _calls)
                            continue;
                            
                        if (returnValue.OnCallsRange.Value.End.Value < _calls
                            && !returnValue.OnCallsRange.Value.End.IsFromEnd
                        )
                            continue;
                            
                        value = returnValue.Value({{args}});
                    }
                    
                    return default!;
                }
            }
            """;
}
