using QuicMoc;

var proxy = Mock.For<IFoo>();

var foo = new FooProxy();
foo.Greeting.Returns("Hello");
foo.Greeting = "Hey!";
foo.Greet.Returns((name, lastName) => $"Hey, {name} {lastName}!");
foo.Greet.Given("Shawn").Returns("Hello, Shawn!");

IFoo bar = foo;
Console.WriteLine(bar.Greeting);
Console.WriteLine(bar.Greet("Shawn"));
Console.WriteLine(bar.Greet("Amanda", "Beachy"));

interface IFoo
{
    string Greeting { get; }
    string Greet(string name, string? lastName = null);
}

sealed class FooProxy : IFoo
{
    string IFoo.Greeting => Greeting;
    public PropertyProxy<string> Greeting { get; set; } = "";

    string IFoo.Greet(string name, string? lastName) => Greet.Greet(name, lastName);

    public GreetMethodProxy Greet { get; } = new();
}

sealed class GreetMethodProxy
{
    private Signature _defaultReturnValue = null!;
    private readonly Dictionary<string, string> _returnValues = [];

    public GreetReturnBuilder Given(string name, string? lastName = null) => new(name, this);

    public string Greet(string name, string? lastName = null) =>
        _returnValues.GetValueOrDefault(name, _defaultReturnValue(name, lastName));

    public void Returns(string returnValue) => _defaultReturnValue = (_, _) => returnValue;

    public void Returns(Signature returnValue) => _defaultReturnValue = returnValue;

    public delegate string Signature(string name, string? lastName = null);

    public sealed class GreetReturnBuilder
    {
        private readonly string _given;
        private readonly GreetMethodProxy _proxy;

        internal GreetReturnBuilder(string given, GreetMethodProxy proxy)
        {
            _given = given;
            _proxy = proxy;
        }

        public void Returns(string returnValue) => _proxy._returnValues[_given] = returnValue;
    }
}

static class Ext
{
    public static T For<T>() => default!;

    public static T For<T>()
        where T : string => "";
}
