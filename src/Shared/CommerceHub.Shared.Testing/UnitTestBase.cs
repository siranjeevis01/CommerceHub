using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using Xunit;

namespace CommerceHub.Shared.Testing;

public abstract class UnitTestBase : IDisposable
{
    protected IFixture Fixture { get; }
    protected IServiceProvider ServiceProvider { get; }

    protected UnitTestBase()
    {
        Fixture = new Fixture()
            .Customize(new AutoMoqCustomization { ConfigureMembers = true, GenerateDelegates = true });

        Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        ServiceProvider = Fixture.Create<IServiceProvider>();
    }

    protected Mock<T> GetMock<T>() where T : class =>
        Fixture.Freeze<Mock<T>>();

    protected T Create<T>() => Fixture.Create<T>();

    protected T CreateWith<T>(Action<T> action)
    {
        var instance = Fixture.Create<T>();
        action(instance);
        return instance;
    }

    protected List<T> CreateMany<T>(int count = 3) => Fixture.CreateMany<T>(count).ToList();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}

public abstract class UnitTestBase<TSut> : UnitTestBase where TSut : class
{
    protected TSut Sut { get; }

    protected UnitTestBase()
    {
        Sut = Create<TSut>();
    }

    protected new Mock<TDependency> GetMock<TDependency>() where TDependency : class =>
        Fixture.Freeze<Mock<TDependency>>();
}
