namespace QuicMoc.Tests.Unit;

public sealed class PropertyMockTests
{
    [Test]
    public async Task GetAndSetProperty_ShouldWork_WhenPropertyIsMock()
    {
        // Arrange.
        var mock = new Mock<IPropertyMockTests>().Quick();
        var foo = new Mock<IFoo>().Quick();
        mock.Foo = foo;

        // Act.
        var gotFoo = mock.Foo;

        // Assert.
        await Assert.That(gotFoo).IsSameReferenceAs(foo);
    }

    [Test]
    public async Task SetProperty_ShouldWork_WhenPropertyIsGetOnly()
    {
        // Arrange.
        var mock = new Mock<IPropertyMockTests>().Quick();

        // Act.
        mock.GetOnly = "foo";

        // Assert.
        await Assert.That(mock.GetOnly).IsEqualTo("foo");
    }

    [Test]
    public async Task SetProperty_ShouldWork_WhenPropertyIsSettable()
    {
        // Arrange.
        var mock = new Mock<IPropertyMockTests>().Quick();

        // Act.
        mock.Settable = "foo";

        // Assert.
        await Assert.That(mock.Settable).IsEqualTo("foo");
    }
}

internal interface IPropertyMockTests
{
    IFoo Foo { get; set; }
    string GetOnly { get; }
    string Settable { get; set; }
}

internal interface IFoo;
