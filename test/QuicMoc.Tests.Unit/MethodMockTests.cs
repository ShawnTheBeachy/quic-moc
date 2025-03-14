namespace QuicMoc.Tests.Unit;

public sealed class MethodMockTests
{
    [Test]
    public async Task Calls_ShouldReturnNumberOfCalls_WhenCallsMatch()
    {
        // Arrange.
        var mock = new Mock<IConfigure>().Quick();

        // Act.
        IConfigure sut = mock;
        sut.Setup<string>(x => x);
        sut.Setup<int>(x => x);
        sut.Setup<int>(x => x);

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(mock.Setup<string>().Calls).IsEqualTo(1);
        await Assert.That(mock.Setup<int>().Calls).IsEqualTo(2);
    }

    [Test]
    public async Task DifferentValues_ShouldBeReturned_WhenMultipleValuesArePassedToReturns()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet().Returns("foo", "bar");

        // Act.
        IMethodMockTests sut = mock;
        string value1 = sut.Greet(),
            value2 = sut.Greet(),
            value3 = sut.Greet();

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(value1).IsEqualTo("foo");
        await Assert.That(value2).IsEqualTo("bar");
        await Assert.That(value3).IsEqualTo("bar");
    }

    [Test]
    public async Task DifferentValues_ShouldBeReturned_WhenParametersAreSpecified()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet("foo").Returns("foo");
        mock.Greet("bar").Returns("bar");

        // Act.
        IMethodMockTests sut = mock;
        string value1 = sut.Greet(),
            value2 = sut.Greet("foo"),
            value3 = sut.Greet("bar");

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(value1).IsNull();
        await Assert.That(value2).IsEqualTo("foo");
        await Assert.That(value3).IsEqualTo("bar");
    }

    [Test]
    public async Task Generic_ShouldMatch_WhenAnyType()
    {
        // Arrange.
        var mock = new Mock<IConverter>().Quick();

        // Act.
        IConverter sut = mock;
        var value1 = sut.Convert<string, int>("");
        var value2 = sut.Convert<int, string>(0);

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(value1).IsDefault();
        await Assert.That(value2).IsDefault();
        await Assert.That(mock.Convert<AnyType, AnyType>().Calls).IsEqualTo(2);
    }

    [Test]
    public async Task Generic_ShouldMatch_WhenSameGeneric()
    {
        // Arrange.
        var mock = new Mock<IConverter>().Quick();
        mock.Convert<int, string>().Returns(x => x.ToString());

        // Act.
        IConverter sut = mock;
        var converted = sut.Convert<int, string>(3);

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(converted).IsEqualTo("3");
        await Assert.That(mock.Convert<int, string>().Calls).IsEqualTo(1);
    }

    [Test]
    public async Task Generic_ShouldNotMatch_WhenDifferentGeneric()
    {
        // Arrange.
        var mock = new Mock<IConverter>().Quick();
        mock.Convert<int, double>().Returns(x => x);

        // Act.
        IConverter sut = mock;
        var converted = sut.Convert<int, string>(3);

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(converted).IsNull();
        await Assert.That(mock.Convert<int, string>().Calls).IsEqualTo(1);
        await Assert.That(mock.Convert<string, int>().Calls).IsEqualTo(0);
    }

    [Test]
    public async Task Generic_ShouldWork_WhenTypesAreNullable()
    {
        // Arrange.
        var mock = new Mock<IConverter>().Quick();
        mock.Convert<int?, string?>().Returns(x => x?.ToString());

        // Act.
        IConverter sut = mock;
        var converted = sut.Convert<int?, string?>(null);

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(converted).IsNull();
        await Assert.That(mock.Convert<int?, string?>().Calls).IsEqualTo(1);
        await Assert.That(mock.Convert<int, string>().Calls).IsZero();
    }

    [Test]
    public async Task NewValue_ShouldBeReturned_WhenReturnValueIsOverwritten()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet().Returns("foo");

        // Act.
        IMethodMockTests sut = mock;
        var value1 = sut.Greet();
        mock.Greet().Returns("bar");
        var value2 = sut.Greet();

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(value1).IsEqualTo("foo");
        await Assert.That(value2).IsEqualTo("bar");
    }

    [Test]
    public async Task OnCalls_ShouldReturnValue_WhenCallIsInRange()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet().Returns("foo").OnCalls(1..2);
        mock.Greet().Returns("bar").OnCalls(3..5);

        // Act.
        IMethodMockTests sut = mock;
        string value1 = sut.Greet(),
            value2 = sut.Greet(),
            value3 = sut.Greet(),
            value4 = sut.Greet(),
            value5 = sut.Greet(),
            value6 = sut.Greet();

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(value1).IsEqualTo("foo");
        await Assert.That(value2).IsEqualTo("foo");
        await Assert.That(value3).IsEqualTo("bar");
        await Assert.That(value4).IsEqualTo("bar");
        await Assert.That(value5).IsEqualTo("bar");
        await Assert.That(value6).IsNull();
    }

    [Test]
    public async Task OnCalls_ShouldReturnValue_WhenRangeDoesNotHaveEnd()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet().Returns("foo").OnCalls(2..);

        // Act.
        IMethodMockTests sut = mock;
        string value1 = sut.Greet(),
            value2 = sut.Greet(),
            value3 = sut.Greet();

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(value1).IsNull();
        await Assert.That(value2).IsEqualTo("foo");
        await Assert.That(value3).IsEqualTo("foo");
    }

    [Test]
    public async Task OutParameter_ShouldBeSet_WhenReturnValueSetsIt()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet<AnyType>(Arg<string>.Any(), Arg<string>.Any())
            .Returns(
                (string _, out string g) =>
                {
                    g = "foo";
                }
            );

        // Act.
        IMethodMockTests sut = mock;
        sut.Greet<string>("foo", out var greeting);

        // Assert.
        await Assert.That(greeting).IsEqualTo("foo");
    }

    [Test]
    public async Task Type_ShouldBeFullyQualified_WhenCustomType()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet(Arg<Person>.Any()).Returns(person => $"Hello, {person.Name}!");

        // Act.
        IMethodMockTests sut = mock;
        var greeting = sut.Greet(new Person("Foo"));

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(greeting).IsEqualTo("Hello, Foo!");
        await Assert.That(mock.Greet(Arg<Person>.Any()).Calls).IsEqualTo(1);
    }
}

internal interface IConfigure
{
    void Setup<T>(Func<T, T> configure);
}

internal interface IConverter
{
    TR Convert<T, TR>(T value);
}

internal interface IMethodMockTests
{
    void Greet<T>(string name, out string greeting);
    string Greet();
    string Greet(Person person);
    string Greet(string name);
}

public sealed record Person(string Name);
