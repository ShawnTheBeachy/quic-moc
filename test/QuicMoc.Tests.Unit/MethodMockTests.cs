namespace QuicMoc.Tests.Unit;

public sealed class MethodMockTests
{
    [Test]
    public async Task DifferentValues_ShouldBeReturned_WhenMultipleValuesArePassedToReturns()
    {
        // Arrange.
        var mock = new Mock<IMethodMockTests>().Quick();
        mock.Greet().Returns(() => "foo", () => "bar");

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
}

internal interface IMethodMockTests
{
    string Greet();
}
