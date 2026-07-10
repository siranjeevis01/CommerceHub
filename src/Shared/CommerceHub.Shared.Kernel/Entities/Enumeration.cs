using System.Reflection;

namespace CommerceHub.Shared.Kernel.Entities;

public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Dictionary<int, TEnum> Enumerations = CreateEnumerations();

    public int Id { get; protected init; }
    public string Name { get; protected init; } = string.Empty;

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static TEnum? FromId(int id) => Enumerations.TryGetValue(id, out var enumeration) ? enumeration : null;
    public static TEnum? FromName(string name) => Enumerations.Values.SingleOrDefault(e => e.Name == name);
    public static IReadOnlyCollection<TEnum> GetValues() => Enumerations.Values.ToList().AsReadOnly();

    private static Dictionary<int, TEnum> CreateEnumerations()
    {
        var enumerationType = typeof(TEnum);
        var fields = enumerationType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => enumerationType.IsAssignableFrom(f.FieldType))
            .Select(f => (TEnum)f.GetValue(null)!);

        return fields.ToDictionary(f => f.Id);
    }

    public bool Equals(Enumeration<TEnum>? other)
    {
        if (other is null) return false;
        return GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj) => obj is Enumeration<TEnum> other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Name;
}
