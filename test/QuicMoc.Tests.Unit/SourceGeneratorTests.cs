/*using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;*/

namespace QuicMoc.Tests.Unit;

public sealed class SourceGeneratorTests
{
    private const string VectorClassText =
        @"
namespace TestNamespace;

public sealed class Tests
{
    public void Foo()
    {
        _ = new Mock<IFoo>().Quick();
        _ = new Mock<IFoo>().Quick();
    }

    public interface IFoo
    {
        string Greeting { get; }
        string Greet(string name, string? lastName = null);
    }
}
";

    /*[Test]
    public async Task Target_ShouldOutcome_WhenScenario()
    {
        // Arrange.
        var generator = new SourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // We need to create a compilation with the required source code.
        var compilation = CSharpCompilation.Create(
            typeof(SourceGeneratorTests).Assembly.FullName,
            [CSharpSyntaxTree.ParseText(VectorClassText)],
            [
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            ]
        );

        // Act.
        var runResult = driver.RunGenerators(compilation).GetRunResult();
        var generatedFileSyntax = runResult.GeneratedTrees.Single(t =>
            t.FilePath.EndsWith("PropertyProxy.g.cs")
        );

        // Assert.
        await Assert.That((await generatedFileSyntax.GetTextAsync()).ToString()).IsEqualTo("");
    }*/
}
