using QuicMoc;
using QuicMoc.ConsoleApp;

/*var mock = new Mock<IGreeter>().Quick();
mock.Greeting = "Hello";
mock.Greet(Arg<string>.Any()).Returns(name => $"{mock.Greeting}, {name}!").OnCalls(1..1);
mock.Greet(Arg<string>.Any())
    .Returns(name => $"We're all out of greetings, {name} :(")
    .OnCalls(2..);
mock.Greet("Shawn").Returns(name => $"{mock.Greeting}, dev {name}!");
mock.Greet(Arg<string>.Is(x => x.StartsWith('B'))).Returns(name => $"You're a b, {name}!");

mock.Greet(lastName: "Beachy").Returns((firstName, _) => $"Aha! {firstName} is one of us!");

IGreeter greeter = mock;
Console.WriteLine(greeter.Greet("Amanda"));
Console.WriteLine(greeter.Greet("Aisling"));
Console.WriteLine(greeter.Greet("Shawn"));
Console.WriteLine(greeter.Greet("Bob"));

Console.WriteLine(greeter.Greet("Aisling", "Beachy"));
Console.WriteLine(greeter.Greet("Amanda", "Martin"));*/

BenchmarkDotNet.Running.BenchmarkRunner.Run<Benchmarks>();
