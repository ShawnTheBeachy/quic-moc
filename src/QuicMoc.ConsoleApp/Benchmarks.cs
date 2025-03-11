using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Moq;
using NSubstitute;

namespace QuicMoc.ConsoleApp;

[SimpleJob(runtimeMoniker: RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class Benchmarks
{
    private IReadOnlyList<string> _greetings = [];

    [GlobalSetup]
    public void Setup()
    {
        _greetings = Enumerable.Repeat("Hello", 2_000).ToArray();
    }

    [Benchmark]
    public void Moq()
    {
        foreach (var greeting in _greetings)
        {
            var moq = new Moq.Mock<IGreeter>();
            moq.Setup(x => x.Greeting).Returns(greeting);
            moq.Setup(x => x.Greet(It.Is<string>(xx => xx.StartsWith('C'))))
                .Returns((string name) => $"You're a c, {name}!");
            moq.Setup(x => x.Greet(It.IsAny<string>()))
                .Returns((string name) => $"{moq.Object.Greeting}, {name}!");
            moq.Setup(x => x.Greet("Bilbo")).Returns("Happy eleventyith birthday!");

            moq.Object.Greet("Bilbo");
            moq.Object.Greet("Frodo");
            moq.Object.Greet("Cecilia");
        }
    }

    [Benchmark]
    public void NSubstitute()
    {
        foreach (var greeting in _greetings)
        {
            var substitute = Substitute.For<IGreeter>();
            substitute.Greeting.Returns(greeting);
            substitute
                .Greet(Arg.Is<string>(x => x.StartsWith('C')))
                .Returns(callInfo => $"You're a c, {callInfo.ArgAt<string>(0)}!");
            substitute
                .Greet(Arg.Any<string>())
                .Returns(callInfo => $"{substitute.Greeting}, {callInfo.ArgAt<string>(0)}!");
            substitute.Greet("Bilbo").Returns("Happy eleventyith birthday!");

            substitute.Greet("Bilbo");
            substitute.Greet("Frodo");
            substitute.Greet("Cecilia");
        }
    }

    [Benchmark]
    public void QuicMoc()
    {
        foreach (var greeting in _greetings)
        {
            var mock = new Mock<IGreeter>().Quick();
            mock.Greeting = greeting;
            mock.Greet(Arg<string>.Any()).Returns(name => $"{mock.Greeting}, {name}!");
            mock.Greet("Bilbo").Returns("Happy eleventyith birthday!");
            mock.Greet(Arg<string>.Is(x => x.StartsWith('C')))
                .Returns(name => $"You're a c, {name}!");

            IGreeter greeter = mock;
            greeter.Greet("Bilbo");
            greeter.Greet("Frodo");
            greeter.Greet("Cecilia");
        }
    }
}

public interface IGreeter
{
    string Greeting { get; }
    string Greet(string name);
    string Greet(string firstName, string lastName);
}
