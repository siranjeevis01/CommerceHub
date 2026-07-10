using AutoFixture;
namespace CommerceHub.Shared.Testing;

public static class TestDataFactory
{
    private static readonly Fixture Fixture = new();

    static TestDataFactory()
    {
        Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public static T Create<T>() => Fixture.Create<T>();

    public static T CreateWith<T>(Action<T> action)
    {
        var instance = Fixture.Create<T>();
        action(instance);
        return instance;
    }

    public static List<T> CreateMany<T>(int count = 3) => Fixture.CreateMany<T>(count).ToList();

    public static void Customize(Action<IFixture> customization) =>
        customization(Fixture);

    public static void Freeze<T>(Action<T> action) =>
        Fixture.Register(() =>
        {
            var instance = Fixture.Create<T>();
            action(instance);
            return instance;
        });

    public static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}


