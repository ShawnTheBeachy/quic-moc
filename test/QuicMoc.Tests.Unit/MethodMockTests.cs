namespace QuicMoc.Tests.Unit;

public sealed class MethodMockTests
{
    [Test]
    public async Task Calls_ShouldReturnNumberOfCalls_WhenCallsMatch()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet("foo").Returns("foo");
        mock.Greet("bar").Returns("bar");

        // Act.
        IMethodMockTests sut = mock;
        sut.Greet("");
        sut.Greet("foo");
        sut.Greet("bar");
        sut.Greet("bar");

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(mock.Greet("foo").Calls).IsEqualTo(1);
        await Assert.That(mock.Greet("bar").Calls).IsEqualTo(2);
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
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet<AnyType>().Returns("foo");

        // Act.
        IMethodMockTests sut = mock;
        var value1 = sut.Greet<string>();
        var value2 = sut.Greet<int>();

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(value1).IsEqualTo("foo");
        await Assert.That(value2).IsEqualTo("foo");
        await Assert.That(mock.Greet<AnyType>().Calls).IsEqualTo(2);
    }

    [Test]
    public async Task Generic_ShouldMatch_WhenSameGeneric()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet<string>().Returns("foo");

        // Act.
        IMethodMockTests sut = mock;
        var greeting = sut.Greet<string>();

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(greeting).IsEqualTo("foo");
        await Assert.That(mock.Greet<string>().Calls).IsEqualTo(1);
    }

    [Test]
    public async Task Generic_ShouldNotMatch_WhenDifferentGeneric()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet<string>().Returns("foo");

        // Act.
        IMethodMockTests sut = mock;
        var greeting = sut.Greet<int>();

        // Assert.
        using var asserts = Assert.Multiple();
        await Assert.That(greeting).IsNull();
        await Assert.That(mock.Greet<int>().Calls).IsEqualTo(1);
        await Assert.That(mock.Greet<string>().Calls).IsEqualTo(0);
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
        mock.Greet(Arg<string>.Any(), Arg<string>.Any())
            .Returns(
                (string _, out string g) =>
                {
                    g = "foo";
                }
            );

        // Act.
        IMethodMockTests sut = mock;
        sut.Greet("foo", out var greeting);

        // Assert.
        await Assert.That(greeting).IsEqualTo("foo");
    }
}

internal interface IMethodMockTests
{
    string Greet<T>();
    void Greet(string name, out string greeting);
    string Greet();
    string Greet(string name);
}
