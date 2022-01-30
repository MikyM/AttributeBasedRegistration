namespace MikyM.Autofac.Extensions.Attributes;

/// <summary>
/// Defines with which lifetime should the service be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LifetimeAttribute : Attribute
{
    public Lifetime Scope { get; private set; }
    public Type? Owned { get; private set; }
    public IEnumerable<object> Tags { get; private set; } = new List<string>();

    public LifetimeAttribute(Lifetime scope)
    {
        Scope = scope;
    }

    public LifetimeAttribute(Lifetime scope, Type owned)
    {
        Scope = scope;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    public LifetimeAttribute(Lifetime scope, IEnumerable<object> tags)
    {
        Scope = scope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }

    public LifetimeAttribute(Type owned)
    {
        Scope = Lifetime.InstancePerOwned;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    public LifetimeAttribute(IEnumerable<object> tags)
    {
        Scope = Lifetime.InstancePerMatchingLifetimeScope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }
}