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
        IReadOnlyList<Parameter> parameters,
        IReadOnlyList<Parameter> inParameters
    ) =>
        $$"""
        internal sealed class ReturnValues
        {
             private int _calls;
             private readonly Matcher _matcher;
             private readonly {{methodName}}MethodMock _methodMock;
             public List<ReturnValue> Items { get; } = [];
             public Range? OnCallsRange { get; private set; }

             public ReturnValues(Matcher matcher, {{methodName}}MethodMock methodMock)
             {
                 _matcher = matcher;
                 _methodMock = methodMock;
                 methodMock._returnValues.Add(this);
             }

             public int Calls => _methodMock.Calls(_matcher);

             public bool {{"Matches".MakeGeneric(method.TypeParameters)}}({{inParameters.Parameters(null)}}) => _matcher({{inParameters.Args(method)}});

             public void OnCalls(Range range) => OnCallsRange = range;

             public {{method.ReturnType()}} {{"Value".MakeGeneric(method.TypeParameters)}}({{parameters.Parameters(null)}})
             {
                var index = 0;

                for (var i = 0; i < Items.Count; i++)
                {
                   if (i > _calls)
                       break;

                   index = i;
                }

                _calls++;
                {{(method.ReturnsVoid ? "" : $"return {(method.ReturnType.IsGeneric(method) ? $"({method.ReturnType})" : "")}")}}
                    Items[index].Value({{string.Join(", ", parameters.Select(x => $"{x.Ref()} {x.Name}"))}});
             }
        }
        
        public sealed class {{"ReturnValuesBuilder".MakeGeneric(method.TypeParameters)}}
        {
            private readonly ReturnValues _returnValues;
            
            internal ReturnValuesBuilder(ReturnValues returnValues)
            {
                _returnValues = returnValues;
            }
        
            public int Calls => _returnValues.Calls;
        
            public void OnCalls(Range range) => _returnValues.OnCalls(range);
        
            public {{"ReturnValuesBuilder".MakeGeneric(method.TypeParameters)}} Returns(params {{(method.ReturnsVoid ? "Action" : method.ReturnType())}}[] returnValues)
            {
                foreach (var returnValue in returnValues)
                {
                   var rv = new ReturnValue(({{parameters.Parameters(method, replaceGenericsWithObject: true, includeTypeParams: false)}}) =>
                   {
                        {{string.Join("\n",
                                parameters.Where(x => x.IsOut).Select(x => $"{x.Name} = default!;")
                            )}}
                        {{(method.ReturnsVoid ? "returnValue()" : "return returnValue")}};
                   });
                   _returnValues.Items.Add(rv);
                }
                
                return this;
            }
        
            public {{"ReturnValuesBuilder".MakeGeneric(method.TypeParameters)}} Returns(params {{"Signature".MakeGeneric(method.TypeParameters)}}[] returnValues)
            {
                foreach (var returnValue in returnValues)
                {
                    var rv = new ReturnValue(({{parameters.Args(method, includeTypes: true, includeTypeParams: false, replaceGenericsWithObject: true)}}) =>
                       returnValue({{parameters.ArgsWithGenericCasts(method)}}));
                    _returnValues.Items.Add(rv);
                }
        
                return this;
            }
        }
        """;
}
