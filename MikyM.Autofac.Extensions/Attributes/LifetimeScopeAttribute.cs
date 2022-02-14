namespace MikyM.Autofac.Extensions.Attributes;

/// <summary>
/// Defines with which lifetime should the service be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LifetimeAttribute : Attribute
{
    /// <summary>
    /// Scope to use for the registration
    /// </summary>
    public Lifetime Scope { get; private set; }
    /// <summary>
    /// Type to use for owned registrations
    /// </summary>
    public Type? Owned { get; private set; }
    /// <summary>
    /// Tags to use for tagged registrations
    /// </summary>
    public IEnumerable<object> Tags { get; private set; } = new List<string>();

    /// <summary>
    /// Defines with which lifetime should the service be registered
    /// </summary>
    public LifetimeAttribute(Lifetime scope)
    {
        Scope = scope;
    }

    /// <summary>
    /// Defines with which lifetime should the service be registered
    /// </summary>
    public LifetimeAttribute(Lifetime scope, Type owned)
    {
        Scope = scope;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    /// <summary>
    /// Defines with which lifetime should the service be registered
    /// </summary>
    public LifetimeAttribute(Lifetime scope, IEnumerable<object> tags)
    {
        Scope = scope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }

    /// <summary>
    /// Defines with which lifetime should the service be registered
    /// </summary>
    public LifetimeAttribute(Type owned)
    {
        Scope = Lifetime.InstancePerOwned;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    /// <summary>
    /// Defines with which lifetime should the service be registered
    /// </summary>
    public LifetimeAttribute(IEnumerable<object> tags)
    {
        Scope = Lifetime.InstancePerMatchingLifetimeScope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }
}