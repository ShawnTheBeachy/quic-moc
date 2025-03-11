using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using QuicMoc.Models;

namespace QuicMoc.Generators;

internal static class ReturnValues
{
    public static string MockReturnValues(
        IMethodSymbol method,
        string methodName,
        string generics,
        IReadOnlyList<Parameter> parameters,
        IReadOnlyList<Parameter> inParameters,
        string args,
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
             
             internal bool Matches{{generics}}({{inParameters.Parameters(null)}}) => _matcher({{inParameters.Args(method)}});

             public void OnCalls(Range range) => OnCallsRange = range;
             
             public ReturnValues Returns(params {{(returnsVoid ? "Action" : method.ReturnType())}}[] returnValues)
             {
                 foreach (var returnValue in returnValues)
                 {
                    var rv = new ReturnValue(({{parameters.Parameters(null)}}) => 
                    {
                       {{string.Join("\n",
                            parameters.Where(x => x.IsOut).Select(x => $"{x.Name} = default!;")
                        )}}
                       {{(returnsVoid ? "returnValue()" : "return returnValue")}};
                    });
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

            internal {{method.ReturnType()}} Value({{parameters.Parameters(null)}})
            {
                var index = 0;

                for (var i = 0; i < _returnValues.Count; i++)
                {
                   if (i > _calls)
                       break;
                       
                   index = i;
                }

                _calls++;
                {{(returnsVoid ? "" : "return ")}}_returnValues[index].Value({{args}});
            }
        }
        """;
}
